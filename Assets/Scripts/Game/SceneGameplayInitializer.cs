using UnityEngine;

/// <summary>
/// Game シーン開始時に、戦車・弾・カメラ・床の設定をそろえます。
/// </summary>
public class SceneGameplayInitializer : MonoBehaviour
{
    [Header("発射間隔（1秒あたりの発射数。小さいほど間隔が長い）")]
    [SerializeField]
    float shotsPerSecond = 0.65f;

    [Header("床のスケール（Plane）")]
    [SerializeField]
    Vector3 groundScale = new Vector3(12f, 1f, 12f);

    [Header("床の色")]
    [SerializeField]
    Color groundColor = Color.black;

    [Header("ダウンロードした自作ステージを使う場合はチェックを外す")]
    [SerializeField]
    bool autoSetupGround = false;

    [Header("敵の移動速度")]
    [SerializeField]
    float enemyMoveSpeed = 6f;

    [Header("灰色ブロック障害物")]
    [SerializeField]
    bool spawnObstacles = true;

    [SerializeField]
    int obstacleCount = 14;

    /// <summary>
    /// ゲームシーン起動時に必要なステージ・戦車・カメラおよびチュートリアル等の全アセットを統合配置する。
    /// 入力: なし, 出力: なし, 副作用: シーン内障害物やゾーンの動的生成、および各種マネージャーコンポーネントのアタッチ
    /// </summary>
    void Awake()
    {
        GameOverUI.EnsureExists();
        SetupGround();
        SetupObstacles();
        
        // 背景、霧、見えない壁の設定を追加
        if (GetComponent<EnvironmentSetup>() == null)
        {
            gameObject.AddComponent<EnvironmentSetup>();
        }

        // プレイヤーと敵の初期位置からステージ中心を計算
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        
        Vector3 playerPos = player != null ? player.transform.position : new Vector3(0, 0, -15f);
        Vector3 enemyPos = enemy != null ? enemy.transform.position : new Vector3(0, 0, 15f);
        
        playerPos.y = Mathf.Max(playerPos.y, 0.5f);
        enemyPos.y = Mathf.Max(enemyPos.y, 0.5f);

        Vector3 stageCenter = (playerPos + enemyPos) / 2f;
        stageCenter.y = GetGroundHeight(stageCenter.x, stageCenter.z, 0.1f);

        SetupZones(stageCenter);
        SpawnBaseFlags(playerPos, enemyPos);

        HealZoneSpawner healSpawner = GetComponent<HealZoneSpawner>();
        if (healSpawner == null) healSpawner = gameObject.AddComponent<HealZoneSpawner>();
        healSpawner.SetCenterPosition(stageCenter);

        AttackBuffZoneSpawner buffSpawner = GetComponent<AttackBuffZoneSpawner>();
        if (buffSpawner == null) buffSpawner = gameObject.AddComponent<AttackBuffZoneSpawner>();
        buffSpawner.SetCenterPosition(stageCenter);

        if (player != null)
        {
            Vector3 startPos = playerPos;
            Quaternion startRot = player.transform.rotation;
            
            player.transform.position = startPos;
            player.transform.rotation = startRot;

            SetupTank(player, true);
            SetupTankPhysics(player, 6f);

            TankHealth h = player.GetComponent<TankHealth>();
            if (h != null) h.SetRespawnPoint(startPos, startRot);
        }

        if (enemy != null)
        {
            Vector3 startPos = enemyPos;
            Quaternion startRot = enemy.transform.rotation;
            
            enemy.transform.position = startPos;
            enemy.transform.rotation = startRot;

            SetupTank(enemy, false);
            SetupTankPhysics(enemy, 6f);

            TankHealth h = enemy.GetComponent<TankHealth>();
            if (h != null) h.SetRespawnPoint(startPos, startRot);

            EnemyTankAI enemyAi = enemy.GetComponent<EnemyTankAI>();
            if (enemyAi != null)
            {
                enemyAi.Configure(enemyMoveSpeed);
            }
        }

        GameObject bulletTemplate = SetupBulletTemplate();
        WireShootersBulletPrefab(bulletTemplate);
        SetupCamera(player != null ? player.transform : null);

        // 初回プレイ時のみ説明用のオープニング演出マネージャーを生成する
        if (gameObject.GetComponent<BattleTutorialManager>() == null)
        {
            gameObject.AddComponent<BattleTutorialManager>();
        }
    }

    /// <summary>
    /// キャプチャーゾーンやプログレスバーなどのゾーン関連UI・オブジェクトをセットアップします。
    /// </summary>
    /// <param name="centerPos">ステージの中心位置</param>
    void SetupZones(Vector3 centerPos)
    {
        if (FindFirstObjectByType<CaptureProgressBarUI>() == null)
        {
            GameObject ui = new GameObject("CaptureUI");
            ui.AddComponent<CaptureProgressBarUI>();
        }

        if (FindFirstObjectByType<CaptureZone>() == null)
        {
            GameObject capture = new GameObject("CaptureZone");
            capture.transform.position = centerPos;
            
            SphereCollider col = capture.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 12f;

            MeshRenderer rend = capture.AddComponent<MeshRenderer>();
            rend.material = new Material(Shader.Find("Sprites/Default"));
            rend.material.color = new Color(0.4f, 0.2f, 1.0f, 0.25f); // 青紫 (Blue-Purple)

            ZoneVisualWall wall = capture.AddComponent<ZoneVisualWall>();
            wall.radius = 12f;
            wall.height = 3f;

            capture.AddComponent<CaptureZone>();
            capture.AddComponent<HealZoneVisual>(); // 透過やアニメーション補助
            capture.AddComponent<CaptureZoneItemSpawner>(); // 定期的なアイテム出現
        }
    }

    /// <summary>
    /// プレイヤーと敵の開始位置にそれぞれのフラグ（旗）を生成します。
    /// </summary>
    void SpawnBaseFlags(Vector3 playerPos, Vector3 enemyPos)
    {

        CreateFlag(playerPos, new Color(0.2f, 0.5f, 1f), "PlayerFlag");
        CreateFlag(enemyPos, new Color(1f, 0.2f, 0.2f), "EnemyFlag");
    }

    /// <summary>
    /// 指定された位置に指定した色のフラグ（旗オブジェクト）を生成します。
    /// </summary>
    /// <param name="position">生成する位置</param>
    /// <param name="flagColor">旗の色</param>
    /// <param name="name">オブジェクト名</param>
    void CreateFlag(Vector3 position, Color flagColor, string name)
    {
        GameObject flagRoot = new GameObject(name);
        flagRoot.transform.position = position;

        // 旗竿
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.transform.SetParent(flagRoot.transform);
        pole.transform.localPosition = new Vector3(4f, 4f, 0f);
        pole.transform.localScale = new Vector3(0.2f, 4f, 0.2f);
        Renderer poleRend = pole.GetComponent<Renderer>();
        if (poleRend != null) poleRend.material.color = Color.gray;

        // 旗の布部分
        GameObject cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cloth.transform.SetParent(flagRoot.transform);
        cloth.transform.localPosition = new Vector3(4f + 1.5f, 7f, 0f);
        cloth.transform.localScale = new Vector3(3f, 2f, 0.1f);
        Renderer clothRend = cloth.GetComponent<Renderer>();
        if (clothRend != null) clothRend.material.color = flagColor;

        // 戦車の邪魔にならないようにコライダーを削除
        Destroy(pole.GetComponent<Collider>());
        Destroy(cloth.GetComponent<Collider>());
    }

    /// <summary>
    /// ステージの床（Ground）のスケールと色を設定します。
    /// </summary>
    void SetupGround()
    {
        if (!autoSetupGround) return;

        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            return;
        }

        ground.transform.localScale = groundScale;

        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = groundColor;
        }
    }

    /// <summary>
    /// ステージ上にブロックなどの障害物をランダムに生成します。
    /// </summary>
    void SetupObstacles()
    {
        if (!spawnObstacles)
        {
            return;
        }

        StageObstacleSpawner spawner = GetComponent<StageObstacleSpawner>();
        if (spawner == null)
        {
            spawner = gameObject.AddComponent<StageObstacleSpawner>();
        }

        spawner.ApplySettings(
            obstacleCount,
            new Vector2(52f, 52f),
            new Vector2(1.5f, 4.5f));
        spawner.SpawnBlocks();
    }

    /// <summary>
    /// 戦車オブジェクトの物理パラメータ（質量など）を設定します。
    /// </summary>
    /// <param name="tank">設定対象の戦車オブジェクト</param>
    /// <param name="mass">設定する質量</param>
    static void SetupTankPhysics(GameObject tank, float mass)
    {
        Rigidbody rb = tank.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = mass;
        }
    }

    /// <summary>
    /// シーン内の各戦車のシューターに対して、使用する弾のPrefabを紐づけます。
    /// </summary>
    /// <param name="bulletTemplate">紐づける弾のPrefab（テンプレート）</param>
    void WireShootersBulletPrefab(GameObject bulletTemplate)
    {
        if (bulletTemplate == null)
        {
            return;
        }

        foreach (TankAutoShooter shooter in FindObjectsByType<TankAutoShooter>(FindObjectsSortMode.None))
        {
            shooter.SetBulletPrefab(bulletTemplate);
        }
    }

    /// <summary>
    /// 戦車に必要なコンポーネント（表示用、ダメージ演出用など）を追加・設定します。
    /// </summary>
    /// <param name="tank">設定対象の戦車オブジェクト</param>
    /// <param name="isPlayer">プレイヤーかどうか</param>
    void SetupTank(GameObject tank, bool isPlayer)
    {
        if (tank.GetComponent<TankVisualSetup>() == null)
        {
            tank.AddComponent<TankVisualSetup>();
        }

        if (tank.GetComponent<TankDamageFlash>() == null)
        {
            tank.AddComponent<TankDamageFlash>();
        }

        Transform firePoint = TankVisualSetup.EnsureNozzleHierarchy(
            tank.transform,
            isPlayer ? new Color(0.2f, 0.35f, 0.45f) : new Color(0.45f, 0.2f, 0.2f),
            isPlayer);

        TankAutoShooter shooter = tank.GetComponent<TankAutoShooter>();
        if (shooter != null)
        {
            shooter.ConfigureFromScene(
                firePoint,
                firePoint.parent, // プレイヤーも敵もノズル（firePoint.parent）を独立して回転させる
                shotsPerSecond,
                isPlayer);
        }

        if (isPlayer)
        {

            if (tank.GetComponent<PlayerHealthUI>() == null)
            {
                tank.AddComponent<PlayerHealthUI>();
            }
        }
        else
        {
            if (tank.GetComponent<EnemyHealthUI>() == null)
            {
                tank.AddComponent<EnemyHealthUI>();
            }
        }
    }

    /// <summary>
    /// シーン内にある弾のテンプレートオブジェクトを取得し、物理設定などを初期化します。
    /// </summary>
    /// <returns>初期化された弾のテンプレートオブジェクト</returns>
    GameObject SetupBulletTemplate()
    {
        GameObject bullet = GameObject.Find("BulletTemplate");
        if (bullet == null)
        {
            return null;
        }

        bullet.transform.localScale = new Vector3(2f, 2f, 2f);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.mass = 0.02f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        SphereCollider col = bullet.GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = false;
        }

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.ConfigurePhysicsTemplate();
        }

        return bullet;
    }

    /// <summary>
    /// メインカメラに追従用のスクリプトなどを追加し、ターゲットを設定します。
    /// </summary>
    /// <param name="playerTransform">追従対象（プレイヤー）のTransform</param>
    void SetupCamera(Transform playerTransform)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        SimpleFollowCamera oldFollow = cam.GetComponent<SimpleFollowCamera>();
        if (oldFollow != null)
        {
            Destroy(oldFollow);
        }

        ThirdPersonCamera fpsCamera = cam.GetComponent<ThirdPersonCamera>();
        if (fpsCamera == null)
        {
            fpsCamera = cam.gameObject.AddComponent<ThirdPersonCamera>();
        }

        if (playerTransform != null)
        {
            fpsCamera.SetTarget(playerTransform);
        }

        if (cam.GetComponent<CameraShake>() == null)
        {
            cam.gameObject.AddComponent<CameraShake>();
        }
    }

    /// <summary>
    /// 指定した座標（x, z）における地面の高さをRaycastで取得します。
    /// </summary>
    /// <param name="x">X座標</param>
    /// <param name="z">Z座標</param>
    /// <param name="defaultY">地面が見つからなかった場合のデフォルト高さ</param>
    /// <returns>地面の高さ（Y座標）</returns>
    float GetGroundHeight(float x, float z, float defaultY)
    {
        RaycastHit[] hits = Physics.RaycastAll(new Vector3(x, 100f, z), Vector3.down, 200f);
        float highestY = -9999f;
        bool hitGround = false;
        foreach (var hit in hits)
        {
            if (hit.collider.isTrigger) continue;
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Enemy") || hit.collider.name.Contains("Bullet") || hit.collider.name.Contains("Block")) continue;

            if (hit.point.y > highestY)
            {
                highestY = hit.point.y;
                hitGround = true;
            }
        }
        return hitGround ? highestY + 0.1f : defaultY;
    }
}
