using UnityEngine;

/// <summary>
/// プレイヤーのHPバーおよび操作説明を画面上に表示するUIクラスです。
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    TankHealth playerHealth;
    GUIStyle textStyle;
    GUIStyle bgStyle;
    GUIStyle fillStyle;
    Texture2D bgTex;
    Texture2D fillTex;
    static Font fallbackFont;

    void Start()
    {
        // プレイヤーオブジェクトをタグで検索し、HPコンポーネントを取得
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<TankHealth>();
        }

        // テキスト表示用のスタイルを初期化
        textStyle = new GUIStyle();
        textStyle.fontSize = 24;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.normal.textColor = Color.white;
        textStyle.alignment = TextAnchor.MiddleLeft;

        // 背景用のテクスチャ（半透明の黒）を生成
        bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
        bgTex.Apply();

        // ゲージの塗りつぶし用のテクスチャ（緑）を生成
        fillTex = new Texture2D(1, 1);
        fillTex.SetPixel(0, 0, new Color(0.2f, 0.8f, 0.2f, 1f));
        fillTex.Apply();

        // 背景用のスタイルを設定
        bgStyle = new GUIStyle();
        bgStyle.normal.background = bgTex;

        // ゲージ塗りつぶし用のスタイルを設定
        fillStyle = new GUIStyle();
        fillStyle.normal.background = fillTex;
    }

    void OnGUI()
    {
        // プレイヤーのHPが取得できていない場合は描画しない
        if (playerHealth == null) return;

        // HPバーの配置とサイズを計算
        float width = 300f;
        float height = 30f;
        float x = Screen.width - width - 20f;
        float y = 20f;

        // Draw BG
        // HPバーの背景を描画
        GUI.Box(new Rect(x, y, width, height), "", bgStyle);

        // Draw Fill
        // 現在のHPの割合を計算し、それに応じてゲージを描画
        float hpRatio = (float)playerHealth.CurrentHp / Mathf.Max(1, playerHealth.MaxHp);
        hpRatio = Mathf.Clamp01(hpRatio);
        GUI.Box(new Rect(x, y, width * hpRatio, height), "", fillStyle);

        // Draw Text
        // 現在のHPと最大HPを数値で描画
        GUI.Label(new Rect(x + 10f, y, width, height), $"HP: {playerHealth.CurrentHp} / {playerHealth.MaxHp}", textStyle);

        // Draw Controls
        // 操作説明パネルの配置とサイズを計算
        float controlsWidth = 320f;
        float controlsHeight = 110f;
        float cx = 20f;
        float cy = Screen.height - controlsHeight - 20f;
        
        // 操作説明パネルの背景を描画
        GUI.Box(new Rect(cx, cy, controlsWidth, controlsHeight), "", bgStyle);
        GUIStyle controlStyle = new GUIStyle(textStyle);
        controlStyle.fontSize = 28;
        
        // 操作説明用のフォントをOSのフォントから動的に生成
        if (fallbackFont == null)
        {
            fallbackFont = Font.CreateDynamicFontFromOSFont(new string[] { "Yu Gothic", "Meiryo", "MS Gothic", "Arial" }, 28);
        }
        if (fallbackFont != null)
        {
            controlStyle.font = fallbackFont;
        }

        // 操作説明テキストの影用のスタイルを設定
        GUIStyle shadowStyle = new GUIStyle(controlStyle);
        shadowStyle.normal.textColor = Color.black;

        // 操作説明テキストの描画（影とメインテキストを重ねて描画）
        string controlsText = "WASD : 移動\n十字キー : カメラ";
        GUI.Label(new Rect(cx + 14f, cy + 14f, controlsWidth, controlsHeight), controlsText, shadowStyle);
        GUI.Label(new Rect(cx + 12f, cy + 12f, controlsWidth, controlsHeight), controlsText, controlStyle);
    }
}
