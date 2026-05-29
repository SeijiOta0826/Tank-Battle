using UnityEngine;

/// <summary>
/// ゲームオーバー画面（GameOver シーン）専用の制御スクリプトです。
///
/// 【アタッチ先】GameOver シーン内の空オブジェクト
/// </summary>
public class GameOverScreenController : MonoBehaviour
{
    float startTime;

    void Start()
    {
        // 画面表示開始時刻を記録（フェードインなどの演出用）
        startTime = Time.time;
    }

    void Update()
    {
        // エンターキーでコンティニュー
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (GameManager.Instance != null)
            {
                // コンティニューなのでゲームを最初からリスタート
                GameManager.Instance.StartGame();
            }
            else
            {
                // GameManagerが存在しない場合は直接ゲームシーンをロード
                SceneLoader.LoadGame(); // フォールバック
            }
        }
    }

    void OnGUI()
    {
        // 経過時間に基づいてフェードインの透明度を計算
        float elapsed = Time.time - startTime;
        float alpha = Mathf.Clamp01(elapsed / 2.0f); // 2秒かけてフェードイン
        
        // 全体のGUIの透明度を設定
        GUI.color = new Color(1, 1, 1, alpha);

        // タイトルのスタイル設定
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 120;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.2f, 0.2f); // 赤系
        titleStyle.font = GUI.skin.label.font;
        
        // タイトルの影のスタイル設定
        GUIStyle shadowStyle = new GUIStyle(titleStyle);
        shadowStyle.normal.textColor = Color.black;

        float width = Screen.width;
        float height = Screen.height;
        string text = "Game Over";

        // 影とメインテキストを描画
        GUI.Label(new Rect(5, -50 + 5, width, height), text, shadowStyle);
        GUI.Label(new Rect(0, -50, width, height), text, titleStyle);

        // 2秒経過後にコンティニュー案内のテキストを表示
        if (elapsed > 2.0f)
        {
            GUIStyle startStyle = new GUIStyle(titleStyle);
            startStyle.fontSize = 40;
            // PingPongを使ってアルファ値を上下させ、点滅エフェクトを作成
            float blinkAlpha = Mathf.PingPong((elapsed - 2.0f) * 1.5f, 1f);
            startStyle.normal.textColor = new Color(1f, 1f, 1f, blinkAlpha);
            
            // コンティニュー案内のテキストを描画
            GUI.Label(new Rect(0, 150, width, height), "Press ENTER to Continue", startStyle);
        }
    }
}
