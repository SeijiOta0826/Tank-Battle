using UnityEngine;

/// <summary>
/// タイトル画面（Title シーン）専用の制御スクリプトです。
///
/// 【アタッチ先】Title シーン内の空オブジェクト「TitleUI」など
/// 【やること】プレイヤーの操作でゲーム開始 → GameManager.StartGame()
/// </summary>
public class TitleScreenController : MonoBehaviour
{
    [Header("ゲーム開始のキー（ボタン未設定時の仮操作）")]
    [SerializeField]
    KeyCode startKey = KeyCode.Space;

    [Header("デバッグ用：起動直後に自動でゲーム開始するか")]
    [SerializeField]
    bool autoStartForDebug;

    [Header("爆発エフェクト設定")]
    [SerializeField] float explosionIntervalMin = 0.2f;
    [SerializeField] float explosionIntervalMax = 1.2f;
    float explosionTimer;

    void Start()
    {
        // フォグの設定を無効にする（ゲーム中からの設定持ち越しを防ぐため）
        RenderSettings.fog = false;

        // カメラの背景色を白にする
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.white;
        }

        // タイトルに来たときは状態を Title に揃える（GameManager が既にある場合）
        if (GameManager.Instance != null)
        {
            Debug.Log("[TitleScreen] タイトル画面を表示しました");
        }

        // デバッグ用に自動開始が有効な場合は即座にゲームを開始
        if (autoStartForDebug)
        {
            OnStartButtonClicked();
        }

        // 初回の爆発エフェクトまでのタイマーを設定
        explosionTimer = Random.Range(explosionIntervalMin, explosionIntervalMax);
    }

    void Update()
    {
        // アクティブなシーンがタイトルでない場合は処理しない
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != SceneNames.Title) return;

        // ランダム爆発の生成タイマーの更新
        explosionTimer -= Time.deltaTime;
        if (explosionTimer <= 0f)
        {
            SpawnRandomExplosion();
            // 次の爆発までの時間をランダムに再設定
            explosionTimer = Random.Range(explosionIntervalMin, explosionIntervalMax);
        }

        // UIボタンがまだ無い段階でも、キーボード操作でゲーム開始できるようにする
        if (Input.GetKeyDown(startKey) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnStartButtonClicked();
        }
    }

    /// <summary>
    /// カメラの前方にランダムな位置を計算し、爆発エフェクトを生成します。
    /// </summary>
    void SpawnRandomExplosion()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        
        // カメラの前方にランダムな位置を計算
        float z = Random.Range(5f, 20f);
        float x = Random.Range(-15f, 15f);
        float y = Random.Range(-8f, 8f);
        Vector3 spawnPos = cam.transform.position + cam.transform.forward * z + cam.transform.right * x + cam.transform.up * y;
        
        // ランダムなスケールで爆発エフェクトを生成
        ExplosionVFX.SpawnAt(spawnPos, Random.Range(0.8f, 2.5f));
    }

    void OnGUI()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != SceneNames.Title) return;

        // Tank Battle タイトルロゴ
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 120;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.12f, 0.16f, 0.23f); // 高級感のあるディープスレート (Deep Slate Blue)
        titleStyle.font = GUI.skin.label.font;
        
        GUIStyle shadowStyle = new GUIStyle(titleStyle);
        shadowStyle.normal.textColor = new Color(0.85f, 0.87f, 0.9f); // 白背景になじむ薄いグレーの影

        float width = Screen.width;
        float height = Screen.height;
        string text = "Tank Battle";

        // 影
        GUI.Label(new Rect(4, -50 + 4, width, height), text, shadowStyle);
        // メイン文字
        GUI.Label(new Rect(0, -50, width, height), text, titleStyle);

        // Enterで開始の案内
        GUIStyle startStyle = new GUIStyle(titleStyle);
        startStyle.fontSize = 40;
        
        // 点滅エフェクト（白背景用にダークグレーで点滅）
        float alpha = Mathf.PingPong(Time.time * 1.5f, 1f);
        startStyle.normal.textColor = new Color(0.25f, 0.25f, 0.27f, alpha);
        
        GUI.Label(new Rect(0, 150, width, height), "Press ENTER to Start", startStyle);
    }

    /// <summary>
    /// UI の「スタート」ボタンの OnClick からもこのメソッドを指定してください。
    /// Inspector: Button → On Click () → TitleUI をドラッグ → TitleScreenController.OnStartButtonClicked
    /// </summary>
    public void OnStartButtonClicked()
    {
        // GameManager はタイトルシーンに置いておく想定
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "[TitleScreen] GameManager が見つかりません。Title シーンに GameManager オブジェクトを置いてください。");
            return;
        }

        GameManager.Instance.StartGame();
    }
}
