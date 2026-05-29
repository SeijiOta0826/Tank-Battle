using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーンの読み込みだけを担当するクラスです。
/// 「どのシーンに遷移するか」の知識は SceneNames に、
/// 「いつ遷移するか」は GameManager や各画面の Controller が決めます。
/// </summary>
public static class SceneLoader
{
    /// <summary>タイトル画面へ遷移</summary>
    public static void LoadTitle()
    {
        Load(SceneNames.Title);
    }

    /// <summary>ゲーム本編シーンへ遷移</summary>
    public static void LoadGame()
    {
        Load(SceneNames.Game);
    }

    /// <summary>ゲームクリア画面へ遷移</summary>
    public static void LoadGameClear()
    {
        Load(SceneNames.GameClear);
    }

    /// <summary>ゲームオーバー画面へ遷移</summary>
    public static void LoadGameOver()
    {
        Load(SceneNames.GameOver);
    }

    /// <summary>
    /// 指定したシーン名で読み込みます。
    /// Build Settings に登録されていないシーン名を渡すとエラーになります。
    /// </summary>
    /// <param name="sceneName">読み込むシーンの名前</param>
    static void Load(string sceneName)
    {
        Debug.Log($"[SceneLoader] シーン読み込み: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
