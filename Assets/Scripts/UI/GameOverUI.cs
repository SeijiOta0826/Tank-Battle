using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤー HP0 時に「Game Restart」などの大きな文字を表示します。
/// Game シーンの GameSceneRoot に置くか、実行時に自動生成されます。
/// </summary>
public class GameOverUI : MonoBehaviour
{
    // シングルトンインスタンス
    public static GameOverUI Instance { get; private set; }

    [SerializeField]
    string restartMessage = "Game Restart";

    [SerializeField]
    int fontSize = 72;

    GameObject panelRoot;
    Text messageText;

    void Awake()
    {
        // 既にインスタンスが存在する場合は自身を破棄
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // UIが未作成の場合は構築する
        BuildUIIfNeeded();
        // 初期状態では非表示にする
        Hide();
    }

    /// <summary>
    /// インスタンスが存在するか確認し、なければ自動で生成して返します。
    /// </summary>
    public static GameOverUI EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        // GameSceneRootを探す、なければ作成する
        GameObject root = GameObject.Find("GameSceneRoot");
        if (root == null)
        {
            root = new GameObject("GameSceneRoot");
        }

        // GameOverUIコンポーネントを取得、なければ追加する
        GameOverUI ui = root.GetComponent<GameOverUI>();
        if (ui == null)
        {
            ui = root.AddComponent<GameOverUI>();
        }

        return ui;
    }

    /// <summary>
    /// 必要なUI要素（CanvasやTextなど）をスクリプトから生成して構築します。
    /// </summary>
    void BuildUIIfNeeded()
    {
        // 既に構築済みの場合は処理をスキップ
        if (panelRoot != null)
        {
            return;
        }

        // Canvasの生成と設定
        GameObject canvasObject = new GameObject("GameOverCanvas");
        canvasObject.transform.SetParent(transform);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // 最前面に表示
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        // パネル（背景）の生成と設定
        panelRoot = new GameObject("GameRestartPanel");
        panelRoot.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = panelRoot.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one; // 画面全体に広げる
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 半透明の黒背景を設定
        Image bg = panelRoot.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);

        // メッセージテキスト用のオブジェクト生成と設定
        GameObject textObject = new GameObject("Message");
        textObject.transform.SetParent(panelRoot.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f); // 中央配置
        textRect.sizeDelta = new Vector2(900f, 200f);
        textRect.anchoredPosition = Vector2.zero;

        messageText = textObject.AddComponent<Text>();
        messageText.alignment = TextAnchor.MiddleCenter;
        
        // フォントの設定（OSのフォントから動的に生成、失敗時はレガシーフォントを使用）
        Font uiFont = Font.CreateDynamicFontFromOSFont(new string[] { "Yu Gothic", "Meiryo", "MS Gothic", "Arial" }, fontSize);
        if (uiFont != null)
        {
            messageText.font = uiFont;
        }
        else
        {
            messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        messageText.fontSize = fontSize;
        messageText.color = Color.white;
        messageText.text = restartMessage;
    }

    /// <summary>
    /// ゲームオーバーUIを表示します。
    /// </summary>
    public void Show()
    {
        BuildUIIfNeeded();
        if (messageText != null)
        {
            messageText.text = restartMessage;
        }

        panelRoot.SetActive(true);
    }

    /// <summary>
    /// ゲームオーバーUIを非表示にします。
    /// </summary>
    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    /// <summary>
    /// 指定された秒数だけUIを表示し、その後非表示にします。
    /// </summary>
    public IEnumerator ShowAndWait(float displaySeconds)
    {
        Show();
        yield return new WaitForSeconds(displaySeconds);
        Hide();
    }
}
