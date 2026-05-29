using UnityEngine;

/// <summary>
/// 目標地点の制圧ゲージ（綱引き）を画面上部に表示するUI。
/// </summary>
public class CaptureProgressBarUI : MonoBehaviour
{
    public static CaptureProgressBarUI Instance { get; private set; }

    [Header("現在の制圧状況 (-100 〜 100)")]
    public float captureProgress = 0f; // -100: 敵完全制圧, 100: プレイヤー完全制圧

    public bool isPlayerInZone = false;

    static Font fallbackFont;

    Texture2D playerTexture;
    Texture2D enemyTexture;
    Texture2D bgTexture;

    /// <summary>
    /// 初期化処理。各陣営と背景のテクスチャを生成します。
    /// </summary>
    void Awake()
    {
        Instance = this;

        playerTexture = CreateColorTexture(new Color(0.2f, 0.5f, 1f, 0.9f));
        enemyTexture = CreateColorTexture(new Color(1f, 0.3f, 0.3f, 0.9f));
        bgTexture = CreateColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.9f));
    }

    /// <summary>
    /// 指定された色の1x1テクスチャを生成します。
    /// </summary>
    Texture2D CreateColorTexture(Color c)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, c);
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// GUIを描画します。プログレスバーやスコア、テキストを表示します。
    /// </summary>
    void OnGUI()
    {
        float width = Screen.width * 0.6f;
        float height = 30f;
        float x = (Screen.width - width) / 2f;
        float y = 30f;

        // 中央を 0 として、-100〜100 の割合で幅を決定
        float playerRatio = Mathf.Clamp01((captureProgress + 100f) / 200f);
        
        float playerWidth = width * playerRatio;
        float enemyWidth = width - playerWidth;

        // プレイヤー領域 (左側)
        if (playerWidth > 0)
        {
            GUI.DrawTexture(new Rect(x, y, playerWidth, height), playerTexture);
        }

        // 敵領域 (右側)
        if (enemyWidth > 0)
        {
            GUI.DrawTexture(new Rect(x + playerWidth, y, enemyWidth, height), enemyTexture);
        }

        // 枠線の代わり（少しだけ上下にはみ出す背景を引くなど）
        GUI.DrawTexture(new Rect(x - 2, y - 2, width + 4, 2), bgTexture);
        GUI.DrawTexture(new Rect(x - 2, y + height, width + 4, 2), bgTexture);
        GUI.DrawTexture(new Rect(x - 2, y, 2, height), bgTexture);
        GUI.DrawTexture(new Rect(x + width, y, 2, height), bgTexture);

        // 中央の境界線
        GUI.DrawTexture(new Rect(x + width / 2f - 1f, y, 2f, height), bgTexture);

        // テキスト表示
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        // OSにインストールされている日本語対応フォントを動的に生成して適用する
        if (fallbackFont == null)
        {
            fallbackFont = Font.CreateDynamicFontFromOSFont(new string[] { "Yu Gothic", "Meiryo", "MS Gothic", "Arial" }, 24);
        }
        if (fallbackFont != null)
        {
            style.font = fallbackFont;
        }

        GameSceneController scene = FindFirstObjectByType<GameSceneController>();
        int pScore = scene != null ? scene.PlayerScore : 0;
        int eScore = scene != null ? scene.EnemyScore : 0;
        int winScore = scene != null ? scene.ScoreToWin : 2;

        string scoreText = $"PLAYER  {pScore} - {eScore}  ENEMY ( {winScore} Point Match )";
        
        GUIStyle scoreStyle = new GUIStyle(style);
        scoreStyle.fontSize = 24;
        scoreStyle.normal.textColor = Color.yellow;
        GUIStyle scoreShadow = new GUIStyle(scoreStyle);
        scoreShadow.normal.textColor = Color.black;
        
        GUI.Label(new Rect(x + 2, y - 30 + 2, width, height), scoreText, scoreShadow);
        GUI.Label(new Rect(x, y - 30, width, height), scoreText, scoreStyle);

        string statusText = "目標地点を奪え！";
        if (captureProgress > 0) statusText = $"プレイヤー優勢 {Mathf.FloorToInt(playerRatio * 100)}%";
        else if (captureProgress < 0) statusText = $"敵優勢 {Mathf.FloorToInt((1f - playerRatio) * 100)}%";

        // 文字の影
        GUIStyle shadowStyle = new GUIStyle(style);
        shadowStyle.normal.textColor = Color.black;
        GUI.Label(new Rect(x + 1, y + 1, width, height), statusText, shadowStyle);
        GUI.Label(new Rect(x, y, width, height), statusText, style);

        GUIStyle warningStyle = new GUIStyle(style);
        warningStyle.fontSize = 40;
        warningStyle.normal.textColor = isPlayerInZone ? Color.yellow : new Color(1f, 0.4f, 0.4f);
            
        GUIStyle warningShadow = new GUIStyle(warningStyle);
        warningShadow.normal.textColor = Color.black;

        string warningText = isPlayerInZone ? "守り切れ！！" : "ゾーンに向かえ！";
        float warnY = Screen.height * 0.75f;
        GUI.Label(new Rect(2, warnY + 2, Screen.width, 50), warningText, warningShadow);
        GUI.Label(new Rect(0, warnY, Screen.width, 50), warningText, warningStyle);

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
