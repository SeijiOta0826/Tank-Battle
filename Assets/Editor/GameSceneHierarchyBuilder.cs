#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game シーンの Hierarchy に床・戦車などを配置する Editor 専用ツール。
/// メニュー: Poop → Setup Game Scene Hierarchy
/// </summary>
public static class GameSceneHierarchyBuilder
{
    const string GameScenePath = "Assets/Scenes/Game.unity";

    public static void BuildFromCommandLine()
    {
        SetupGameScene();
    }

    [MenuItem("Poop/Setup Game Scene Hierarchy")]
    public static void SetupGameScene()
    {
        if (!System.IO.File.Exists(GameScenePath))
        {
            Debug.LogError("Game シーンが見つかりません: " + GameScenePath);
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        BuildHierarchy();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameSceneHierarchyBuilder] Game シーンを保存しました。");
    }

    public static void BuildHierarchy()
    {
        EnsureGameSceneRoot();
        EnsureGround();
        GameObject player = EnsurePlayer();
        EnsureEnemy();
        EnsureBulletTemplate();
        EnsureCamera(player.transform);
    }

    static void EnsureGameSceneRoot()
    {
        GameObject root = GameObject.Find("GameSceneRoot");
        if (root == null)
        {
            root = new GameObject("GameSceneRoot");
        }

        if (root.GetComponent<GameSceneController>() == null)
        {
            root.AddComponent<GameSceneController>();
        }

        if (root.GetComponent<CombatVFXSetup>() == null)
        {
            root.AddComponent<CombatVFXSetup>();
        }

        if (root.GetComponent<SceneGameplayInitializer>() == null)
        {
            root.AddComponent<SceneGameplayInitializer>();
        }

        if (root.GetComponent<GameOverUI>() == null)
        {
            root.AddComponent<GameOverUI>();
        }
    }

    static void EnsureGround()
    {
        if (GameObject.Find("Ground") != null)
        {
            return;
        }

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(12f, 1f, 12f);

        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial.color = Color.black;
        }
    }

    static GameObject EnsurePlayer()
    {
        GameObject tank = GameObject.FindGameObjectWithTag("Player");
        if (tank == null)
        {
            tank = CreateTank("Player", "Player", new Vector3(-6f, 0.5f, 0f), new Color(0.35f, 0.75f, 1f), Team.Player, true);
        }
        else
        {
            UpgradeTank(tank, true);
        }

        return tank;
    }

    static void EnsureEnemy()
    {
        GameObject tank = GameObject.FindGameObjectWithTag("Enemy");
        if (tank == null)
        {
            CreateTank("Enemy", "Enemy", new Vector3(6f, 0.5f, 0f), new Color(1f, 0.35f, 0.4f), Team.Enemy, false);
        }
        else
        {
            UpgradeTank(tank, false);
        }
    }

    static GameObject CreateTank(string objectName, string tag, Vector3 position, Color bodyColor, Team team, bool isPlayer)
    {
        GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tank.name = objectName;
        tank.tag = tag;
        tank.transform.position = position;
        tank.transform.localScale = Vector3.one * 1.2f;

        Object.DestroyImmediate(tank.GetComponent<Collider>());
        SphereCollider bodyCollider = tank.AddComponent<SphereCollider>();
        bodyCollider.isTrigger = false;

        Renderer renderer = tank.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial.color = bodyColor;
        }

        Rigidbody rb = tank.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        TankHealth health = tank.AddComponent<TankHealth>();
        health.Configure(team, 3);

        if (isPlayer)
        {
            tank.AddComponent<PlayerTankMovement>();
        }
        else
        {
            EnemyTankAI ai = tank.AddComponent<EnemyTankAI>();
            ai.Configure(6f);
        }

        tank.AddComponent<TankVisualSetup>();
        tank.AddComponent<TankDamageFlash>();
        tank.AddComponent<TankAutoShooter>();

        UpgradeTank(tank, isPlayer);
        return tank;
    }

    static void UpgradeTank(GameObject tank, bool isPlayer)
    {
        Transform firePoint = TankVisualSetup.EnsureNozzleHierarchy(
            tank.transform,
            isPlayer ? new Color(0.2f, 0.35f, 0.45f) : new Color(0.45f, 0.2f, 0.2f),
            isPlayer);

        Transform nozzlePivot = tank.transform.Find("NozzlePivot");

        TankAutoShooter shooter = tank.GetComponent<TankAutoShooter>();
        if (shooter == null)
        {
            shooter = tank.AddComponent<TankAutoShooter>();
        }

        GameObject bulletTemplate = EnsureBulletTemplate();
        shooter.SetBulletPrefab(bulletTemplate);
        shooter.ConfigureFromScene(
            firePoint,
            tank.transform,
            0.65f,
            isPlayer);

        Rigidbody tankRb = tank.GetComponent<Rigidbody>();
        if (tankRb != null)
        {
            tankRb.mass = 6f;
        }

        if (isPlayer)
        {
            PlayerTurretAim oldAim = tank.GetComponent<PlayerTurretAim>();
            if (oldAim != null)
            {
                Object.DestroyImmediate(oldAim);
            }
        }
    }

    static GameObject EnsureBulletTemplate()
    {
        GameObject bullet = GameObject.Find("BulletTemplate");
        if (bullet == null)
        {
            GameObject root = GameObject.Find("GameSceneRoot");
            bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "BulletTemplate";
            bullet.transform.localScale = Vector3.one * 0.35f;
            if (root != null)
            {
                bullet.transform.SetParent(root.transform);
            }
        }

        Object.DestroyImmediate(bullet.GetComponent<Collider>());
        SphereCollider col = bullet.AddComponent<SphereCollider>();
        col.isTrigger = false;

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = bullet.AddComponent<Rigidbody>();
        }

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (bullet.GetComponent<Bullet>() == null)
        {
            bullet.AddComponent<Bullet>();
        }

        bullet.GetComponent<Bullet>().ConfigurePhysicsTemplate();
        bullet.SetActive(false);
        return bullet;
    }

    static void EnsureCamera(Transform followTarget)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObject = new GameObject("Main Camera");
            camObject.tag = "MainCamera";
            cam = camObject.AddComponent<Camera>();
            camObject.AddComponent<AudioListener>();
        }

        SimpleFollowCamera old = cam.GetComponent<SimpleFollowCamera>();
        if (old != null)
        {
            Object.DestroyImmediate(old);
        }

        ThirdPersonCamera third = cam.GetComponent<ThirdPersonCamera>();
        if (third == null)
        {
            third = cam.gameObject.AddComponent<ThirdPersonCamera>();
        }

        third.SetTarget(followTarget);

        if (cam.GetComponent<CameraShake>() == null)
        {
            cam.gameObject.AddComponent<CameraShake>();
        }
    }
}
#endif
