/// <summary>
/// シーン名を一箇所にまとめた定数クラスです。
/// 文字列の打ち間違いを防ぐため、SceneManager.LoadScene では必ずここを使います。
///
/// 【重要】Unity の File → Build Settings に登録するシーン名と、
/// 実際の .unity ファイル名（拡張子なし）が一致している必要があります。
/// </summary>
public static class SceneNames
{
    /// <summary>タイトル画面のシーン名</summary>
    public const string Title = "Title";
    /// <summary>ゲーム本編のシーン名</summary>
    public const string Game = "Game";
    /// <summary>ゲームクリア画面のシーン名</summary>
    public const string GameClear = "GameClear";
    /// <summary>ゲームオーバー画面のシーン名</summary>
    public const string GameOver = "GameOver";
}
