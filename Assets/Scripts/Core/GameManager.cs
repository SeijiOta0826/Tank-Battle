using System.Collections;
using UnityEngine;

/// <summary>
/// ゲーム全体の進行・状態を管理する中心クラス。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    GameState currentState = GameState.Title;

    public GameState CurrentState => currentState;

    [SerializeField]
    int score;

    public int Score => score;

    [Header("「Game Restart」表示時間（秒）")]
    [SerializeField]


    bool isHandlingGameOver;

    // シングルトンの初期化処理
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // シーン遷移してもGameManagerが破棄されないように設定
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// ゲームのステータスを変更します。
    /// </summary>
    void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"[GameManager] 状態変更 → {currentState}");
    }

    /// <summary>
    /// ゲームを開始し、プレイ状態へ遷移します。
    /// </summary>
    public void StartGame()
    {
        score = 0;
        SetState(GameState.Playing);
        SceneLoader.LoadGame();
    }

    /// <summary>
    /// ゲームクリア処理を行います。
    /// </summary>
    public void CompleteGame()
    {
        SetState(GameState.GameClear);
        SceneLoader.LoadGameClear();
    }

    /// <summary>
    /// タイトル画面に戻ります。
    /// </summary>
    public void ReturnToTitle()
    {
        SetState(GameState.Title);
        SceneLoader.LoadTitle();
    }

    /// <summary>
    /// ゲームオーバー処理をトリガーします。
    /// </summary>
    public void TriggerGameOver()
    {
        if (isHandlingGameOver)
        {
            return; // 既にゲームオーバー処理中の場合は重複して処理しない
        }

        SetState(GameState.GameOver);
        SceneLoader.LoadGameOver();
    }

    /// <summary>
    /// スコアを加算します。
    /// </summary>
    public void AddScore(int points)
    {
        score += points;
    }

    /// <summary>
    /// 敵に攻撃を当てた際などの「ヒットストップ（時間停止）」演出を発生させます。
    /// </summary>
    public void TriggerHitStop(float duration = 0.05f)
    {
        StartCoroutine(HitStopRoutine(duration));
    }

    // 時間スケールを0にして一時停止させ、指定時間後に元に戻すコルーチン
    IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    // Escキーが押されたらゲームを終了する
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

