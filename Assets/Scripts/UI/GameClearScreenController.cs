using UnityEngine;

/// <summary>
/// ゲームクリア画面（GameClear シーン）専用の制御スクリプトです。
///
/// 【アタッチ先】GameClear シーン内の空オブジェクト「GameClearUI」など
/// </summary>
public class GameClearScreenController : MonoBehaviour
{
    float startTime;

    /// <summary>
    /// 初期化処理。フォグを無効化し、背景色を白に設定します。
    /// </summary>
    void Start()
    {
        startTime = Time.time;

        // フォグの設定を無効にする（ゲーム中からの設定持ち越しを防ぐため）
        RenderSettings.fog = false;

        // カメラの背景色を白にする
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.white;
        }
    }

    /// <summary>
    /// 毎フレームの入力をチェックし、Enterキーでタイトルに戻ります。
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnReturnToTitleClicked();
        }
    }

    /// <summary>
    /// ゲームクリア画面のGUIを描画します。フェードイン効果とテキストの点滅を行います。
    /// </summary>
    void OnGUI()
    {
        float elapsed = Time.time - startTime;
        float alpha = Mathf.Clamp01(elapsed / 2.0f); // 2秒かけてフェードイン
        
        GUI.color = new Color(1, 1, 1, alpha);

        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 120;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.08f, 0.55f, 0.38f); // 鮮やかなロイヤルティール (Vibrant Royal Teal/Green)
        titleStyle.font = GUI.skin.label.font;
        
        GUIStyle shadowStyle = new GUIStyle(titleStyle);
        shadowStyle.normal.textColor = new Color(0.85f, 0.87f, 0.9f); // 白背景になじむ薄いグレーの影

        float width = Screen.width;
        float height = Screen.height;
        string text = "Game Clear";

        GUI.Label(new Rect(4, -50 + 4, width, height), text, shadowStyle);
        GUI.Label(new Rect(0, -50, width, height), text, titleStyle);

        if (elapsed > 2.0f)
        {
            GUIStyle startStyle = new GUIStyle(titleStyle);
            startStyle.fontSize = 40;
            float blinkAlpha = Mathf.PingPong((elapsed - 2.0f) * 1.5f, 1f);
            startStyle.normal.textColor = new Color(0.25f, 0.25f, 0.27f, blinkAlpha); // 白背景用にダークグレーで点滅
            
            GUI.Label(new Rect(0, 150, width, height), "Press ENTER to return to Title", startStyle);
        }
    }

    /// <summary>
    /// タイトル画面に戻る処理を実行します。
    /// </summary>
    public void OnReturnToTitleClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToTitle();
        }
        else
        {
            SceneLoader.LoadTitle();
        }
    }
}
