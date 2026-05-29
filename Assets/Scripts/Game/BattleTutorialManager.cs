using UnityEngine;

/// <summary>
/// ゲーム開始時に1回だけ実行されるチュートリアル演出マネージャー。
/// </summary>
public class BattleTutorialManager : MonoBehaviour
{
    // アプリケーション起動後の初プレイ時のみチュートリアルを実行するためのセッションライフサイクル管理フラグ
    static bool hasShownTutorial = false;

    GameObject player;
    GameObject enemy;
    ThirdPersonCamera fpsCamera;
    Transform captureZoneTransform;

    PlayerTankMovement playerMove;
    EnemyTankAI enemyMove;
    TankAutoShooter playerShooter;
    TankAutoShooter enemyShooter;

    bool isTutorialActive = false;
    bool isReturningToPlayer = false;
    float returnTimer = 0f;
    const float CAMERA_RETURN_DURATION = 1.5f;

    static Font fallbackFont;

    /// <summary>
    /// セッション初回実行時のみ演出を有効化し、重複実行を防ぐための初期化処理。
    /// 入力: なし, 出力: なし, 副作用: シーン再ロード時等に不要な自身を破棄
    /// </summary>
    void Awake()
    {
        if (hasShownTutorial)
        {
            Destroy(this);
            return;
        }

        hasShownTutorial = true;
        isTutorialActive = true;
    }

    /// <summary>
    /// チュートリアル対象の戦車を固定し、カメラを目標物にフォーカスする。
    /// 入力: なし, 出力: なし, 副作用: 戦車のアクティブコンポーネントおよび物理の無効化、カメラターゲットの変更
    /// </summary>
    void Start()
    {
        if (!isTutorialActive) return;

        player = GameObject.FindGameObjectWithTag("Player");
        enemy = GameObject.FindGameObjectWithTag("Enemy");

        GameObject cz = GameObject.Find("CaptureZone");
        if (cz != null)
        {
            captureZoneTransform = cz.transform;
        }

        if (Camera.main != null)
        {
            fpsCamera = Camera.main.GetComponent<ThirdPersonCamera>();
        }

        if (player != null)
        {
            playerMove = player.GetComponent<PlayerTankMovement>();
            playerShooter = player.GetComponent<TankAutoShooter>();

            if (playerMove != null) playerMove.enabled = false;
            if (playerShooter != null) playerShooter.enabled = false;

            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
                playerRb.isKinematic = true;
            }
        }

        if (enemy != null)
        {
            enemyMove = enemy.GetComponent<EnemyTankAI>();
            enemyShooter = enemy.GetComponent<TankAutoShooter>();

            if (enemyMove != null) enemyMove.enabled = false;
            if (enemyShooter != null) enemyShooter.enabled = false;

            Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.velocity = Vector3.zero;
                enemyRb.angularVelocity = Vector3.zero;
                enemyRb.isKinematic = true;
            }
        }

        if (fpsCamera != null && captureZoneTransform != null)
        {
            // 直径24mの青ゾーンがカメラ画面内に完全に収まるように引きの視野を確保する
            fpsCamera.SetOrbitDistance(26f);
            fpsCamera.SetTarget(captureZoneTransform);
        }
    }

    /// <summary>
    /// プレイヤーの開始入力の検知、およびカメラの復帰シーケンスの経過監視を行う。
    /// 入力: なし, 出力: なし, 副作用: 入力検知によるカメラ切り替えの開始
    /// </summary>
    void Update()
    {
        if (isTutorialActive)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                StartCameraReturn();
            }
        }
        else if (isReturningToPlayer)
        {
            returnTimer += Time.deltaTime;
            if (returnTimer >= CAMERA_RETURN_DURATION)
            {
                StartBattle();
            }
        }
    }

    /// <summary>
    /// 演出から通常プレイへシームレスに移行するためのカメラ復帰の開始処理。
    /// 入力: なし, 出力: なし, 副作用: カメラ距離およびターゲットのリセット（Lerp追従の開始）
    /// </summary>
    void StartCameraReturn()
    {
        isTutorialActive = false;
        isReturningToPlayer = true;
        returnTimer = 0f;

        if (fpsCamera != null && player != null)
        {
            fpsCamera.SetOrbitDistance(12f);
            fpsCamera.SetTarget(player.transform, true);
        }
    }

    /// <summary>
    /// カメラが元の位置に戻り終えたタイミングで戦闘を開始する。
    /// 入力: なし, 出力: なし, 副作用: プレイヤーと敵の操作ロック解除、物理演算の再有効化、このマネージャーの破棄
    /// </summary>
    void StartBattle()
    {
        isReturningToPlayer = false;

        if (player != null)
        {
            if (playerMove != null) playerMove.enabled = true;
            if (playerShooter != null) playerShooter.enabled = true;

            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
            }
        }

        if (enemy != null)
        {
            if (enemyMove != null) enemyMove.enabled = true;
            if (enemyShooter != null) enemyShooter.enabled = true;

            Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.isKinematic = false;
            }
        }

        Destroy(this);
    }

    /// <summary>
    /// バトル開始条件である青ゾーン制圧のルールを説明するテキストUIを描画する。
    /// 入力: なし, 出力: なし, 副作用: GUIへの描画処理
    /// </summary>
    void OnGUI()
    {
        if (!isTutorialActive) return;

        GUIStyle textStyle = new GUIStyle();
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.fontSize = 52;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.normal.textColor = Color.white;

        if (fallbackFont == null)
        {
            fallbackFont = Font.CreateDynamicFontFromOSFont(new string[] { "Yu Gothic", "Meiryo", "MS Gothic", "Arial" }, 54);
        }
        if (fallbackFont != null)
        {
            textStyle.font = fallbackFont;
        }

        string message = "青ゾーンの中にとどまって、上のバーのポイントをためてください！";
        GUIStyle shadowStyle = new GUIStyle(textStyle);
        shadowStyle.normal.textColor = Color.black;

        float mainTextY = Screen.height * 0.35f;
        // 文字サイズが大きいため影のオフセットを4pxに広げて視認性を維持する
        GUI.Label(new Rect(4, mainTextY + 4, Screen.width, 200), message, shadowStyle);
        GUI.Label(new Rect(0, mainTextY, Screen.width, 200), message, textStyle);

        string enterMessage = "Press Enter to Start";
        GUIStyle enterStyle = new GUIStyle(textStyle);
        enterStyle.fontSize = 40;
        enterStyle.normal.textColor = Color.yellow;

        GUIStyle enterShadow = new GUIStyle(enterStyle);
        enterShadow.normal.textColor = Color.black;

        float enterTextY = Screen.height * 0.6f;
        GUI.Label(new Rect(4, enterTextY + 4, Screen.width, 100), enterMessage, enterShadow);
        GUI.Label(new Rect(0, enterTextY, Screen.width, 100), enterMessage, enterStyle);
    }

    /// <summary>
    /// オブジェクト破棄時に静的フォントアセットを解放し、エディタのメモリ警告を防ぐ。
    /// 入力: なし, 出力: なし, 副作用: fallbackFontの破棄および参照のクリア
    /// </summary>
    void OnDestroy()
    {
        if (fallbackFont != null)
        {
            Destroy(fallbackFont);
            fallbackFont = null;
        }
    }
}
