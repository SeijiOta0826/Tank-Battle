using System.Collections;
using UnityEngine;

/// <summary>
/// 戦車が常に弾を撃ち続けるコンポーネント。
/// プレイヤー: 十字キーで向けたノズル方向 / 敵: プレイヤー方向
/// </summary>
public class TankAutoShooter : MonoBehaviour
{
    [SerializeField]
    Team team = Team.Player;

    [SerializeField]
    GameObject bulletPrefab;

    [SerializeField]
    Transform firePoint;

    [SerializeField]
    float shotsPerSecond = 0.65f;

    [SerializeField]
    Transform aimTransform;

    [SerializeField]
    bool aimAtPlayer;

    [SerializeField]
    float aimRotationSpeed = 100f;

    [Header("弾道を山なりにするための上向き補正（敵用）")]
    [SerializeField]


    float nextFireTime;
    float baseShotsPerSecond;
    float fireRateMultiplier = 1f;
    Coroutine fireRateBoostCoroutine;

    [HideInInspector]
    public bool hasAttackBuff = false;
    GameObject redAuraObject;

    public int extraDamage = 0;
    public float extraBlastRadius = 0f;

    TankHealth health;
    Transform playerTransform;

    // コンポーネントと初期値の取得
    void Awake()
    {
        health = GetComponent<TankHealth>();
        baseShotsPerSecond = shotsPerSecond;

        // 照準の基準が未設定の場合は発射口、もしくは自身のTransformを使用
        if (aimTransform == null)
        {
            aimTransform = firePoint != null ? firePoint : transform;
        }
    }

    // 開始時のプレイヤー追跡設定と照準UIの追加
    void Start()
    {
        // プレイヤーを狙う設定の場合はプレイヤーのTransformを検索して保持
        if (aimAtPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // プレイヤーの場合はクロスヘア（照準UI）を追加
        if (team == Team.Player)
        {
            gameObject.AddComponent<PlayerCrosshair>();
        }

        // 最初の射撃までのクールダウンを設定
        nextFireTime = Time.time + 0.5f;
    }

    // 毎フレームの更新処理。照準の更新と射撃の判定を行う
    void Update()
    {
        // 必須項目が設定されていない、または死亡している場合は何もしない
        if (bulletPrefab == null || firePoint == null)
        {
            return;
        }

        if (health != null && health.IsDead)
        {
            return;
        }

        UpdateAim();

        float currentRate = baseShotsPerSecond;
        if (team == Team.Enemy)
        {
            currentRate *= 1.5f; // 敵の射撃テンポを1.5倍に強化
        }

        // 攻撃力バフがある場合は射撃速度を2倍にし、赤いオーラを表示する
        if (hasAttackBuff)
        {
            currentRate *= 2f;
            if (redAuraObject == null)
            {
                redAuraObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(redAuraObject.GetComponent<Collider>());
                redAuraObject.transform.SetParent(transform);
                redAuraObject.transform.localPosition = Vector3.up * 0.5f;
                redAuraObject.transform.localScale = new Vector3(3f, 3f, 3f);
                
                // オーラのマテリアル設定（半透明の赤）
                Renderer r = redAuraObject.GetComponent<Renderer>();
                Material mat = r.material;
                mat.color = new Color(1f, 0.1f, 0.1f, 0.3f);
                mat.SetFloat("_Mode", 3f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            redAuraObject.SetActive(true);
        }
        else
        {
            currentRate *= fireRateMultiplier; // 既存の時限バフ用
            if (redAuraObject != null) redAuraObject.SetActive(false);
        }

        // 次のフレームのためにリセット（ゾーン内にいればOnTriggerStayで再びtrueになる）
        hasAttackBuff = false;

        // 射撃可能なタイミングであれば射撃を実行
        if (Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + (1f / Mathf.Max(currentRate, 0.1f));
        }
    }

    /// <summary>
    /// 照準を更新します。敵ならプレイヤーの方向、プレイヤーならカメラ中心の向く方向へ砲塔を回転させます。
    /// </summary>
    void UpdateAim()
    {
        if (aimAtPlayer && playerTransform != null && aimTransform != null)
        {
            // プレイヤーの未来位置を予測（偏差射撃）
            Vector3 targetPos = playerTransform.position;
            Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                float dist = Vector3.Distance(aimTransform.position, targetPos);
                float bulletSpeed = 22f; // 敵の弾速（EnemyLaunchSpeed）
                float timeToHit = dist / bulletSpeed;
                
                // 予測位置の計算（プレイヤーの現在速度 × 着弾までの時間）
                targetPos += playerRb.velocity * timeToHit;
            }

            // 敵から予測位置へのベクトルを計算（高さは無視）
            Vector3 toPlayer = targetPos - aimTransform.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
                // 直撃させるために、重力落下分だけ少し上を向くように修正（-18度 -> -3度）
                Quaternion pitched = target * Quaternion.Euler(-3f, 0f, 0f);
                // 目標の回転に向けて滑らかに回転させる
                aimTransform.rotation = Quaternion.RotateTowards(
                    aimTransform.rotation,
                    pitched,
                    aimRotationSpeed * Time.deltaTime);
            }
        }
        else if (team == Team.Player && aimTransform != null)
        {
            // プレイヤーの場合は画面中央からのRayキャストで目標地点を決定
            Vector3 targetDir = GetPlayerTargetDirection();
            if (targetDir.sqrMagnitude > 0.01f)
            {
                // 照準の方向より少し上（-10度）を向くように補正
                Quaternion target = Quaternion.LookRotation(targetDir, Vector3.up) * Quaternion.Euler(-10f, 0f, 0f);
                // マズル角度を照準角度に合わせるスピードをさらに上げる（3.0f -> 8.0f）
                aimTransform.rotation = Quaternion.Slerp(
                    aimTransform.rotation,
                    target,
                    8.0f * Time.deltaTime);
            }
        }
    }

    public void Configure(GameObject bullet, Transform fire, Team ownerTeam, bool aimPlayer)
    {
        bulletPrefab = bullet;
        firePoint = fire;
        team = ownerTeam;
        aimAtPlayer = aimPlayer;
        aimTransform = fire != null ? fire.parent : transform;
    }

    /// <summary>SceneGameplayInitializer から Inspector 相当の値を設定</summary>
    public void ConfigureFromScene(
        Transform fire,
        Transform aim,
        float fireRate,
        bool isPlayer)
    {
        firePoint = fire;
        aimTransform = aim != null ? aim : fire;
        baseShotsPerSecond = fireRate;
        shotsPerSecond = fireRate;
        team = isPlayer ? Team.Player : Team.Enemy;
        aimAtPlayer = !isPlayer;
    }

    public void SetBulletPrefab(GameObject prefab)
    {
        bulletPrefab = prefab;
    }

    /// <summary>
    /// 弾を発射します。弾オブジェクトを生成し、初期化を行います。
    /// </summary>
    void Fire()
    {
        Vector3 fireDirection = GetFireDirection();

        // 弾を生成して向きを設定
        GameObject bulletObject = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(fireDirection, Vector3.up));

        bulletObject.SetActive(true);

        // 弾の初期化メソッドを呼び出し、チームと方向を渡す
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(team, fireDirection);
            bullet.ApplyModifiers(extraDamage, extraBlastRadius);
        }

        // 発射音を再生
        SimpleAudioManager.Instance?.PlayShoot(firePoint.position);
    }

    /// <summary>
    /// プレイヤー画面中央に基づく目標方向を計算します。
    /// </summary>
    Vector3 GetPlayerTargetDirection()
    {
        if (Camera.main == null)
            return firePoint.forward;

        // 画面中央からレイを飛ばす
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = ray.GetPoint(1000f); // 何も当たらなかった場合のデフォルト点

        // 全ヒットを取得して距離順にソート
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // 自身の弾除けなどを除外し、最初に当たった物をターゲットとする
        foreach (var hit in hits)
        {
            if (!hit.collider.transform.IsChildOf(transform.root))
            {
                targetPoint = hit.point;
                break;
            }
        }

        // 発射口からターゲット点への方向ベクトルを返す
        return (targetPoint - firePoint.position).normalized;
    }

    /// <summary>
    /// 実際の発射方向を計算します。
    /// プレイヤーの場合は照準に基づく補正を加えます。
    /// </summary>
    Vector3 GetFireDirection()
    {
        if (team == Team.Player)
        {
            // 照準の方向に直接撃つように補正をかける（マズルの遅れの影響を減らす）
            Vector3 targetDir = GetPlayerTargetDirection();
            // 重力落下分を少し加味して上に向ける
            Vector3 adjustedDir = Quaternion.Euler(-3f, 0f, 0f) * targetDir;
            // 完全に照準依存だと違和感が出る場合があるため、マズルの向きとブレンドする（照準依存度70%）
            return Vector3.Lerp(firePoint.forward, adjustedDir, 0.7f).normalized;
        }

        // 敵の場合は発射口の正面にそのまま撃つ
        return firePoint.forward;
    }

    /// <summary>
    /// 一定時間、連射速度を上げるバフを適用します。
    /// </summary>
    public void ApplyFireRateBoost(float multiplier, float durationSeconds)
    {
        if (fireRateBoostCoroutine != null)
        {
            StopCoroutine(fireRateBoostCoroutine);
        }

        fireRateBoostCoroutine = StartCoroutine(FireRateBoostRoutine(multiplier, durationSeconds));
    }

    // 指定時間経過後にバフを解除するコルーチン
    IEnumerator FireRateBoostRoutine(float multiplier, float durationSeconds)
    {
        fireRateMultiplier = multiplier;
        yield return new WaitForSeconds(durationSeconds);
        fireRateMultiplier = 1f;
        fireRateBoostCoroutine = null;
    }
}
