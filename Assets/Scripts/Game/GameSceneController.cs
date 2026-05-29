using UnityEngine;

/// <summary>
/// ゲーム本編シーン（Game シーン）の進行を扱う雛形です。
/// 戦車・弾丸・敵などの実装は別スクリプトに分け、
/// 「クリア条件を満たしたら GameManager.CompleteGame() を呼ぶ」役割だけ持ちます。
///
/// 【アタッチ先】Game シーン内の空オブジェクト「GameSceneRoot」など
/// </summary>
public class GameSceneController : MonoBehaviour
{
    bool hasCleared;

    [Header("デバッグ：敵を倒さずにクリアしたいときだけ使う（通常はオフ推奨）")]
    [SerializeField]
    bool enableDebugClearKey;

    [SerializeField]
    KeyCode debugClearKey = KeyCode.C;

    /// <summary>プレイヤーの現在のスコア</summary>
    public int PlayerScore { get; private set; } = 0;
    /// <summary>敵の現在のスコア</summary>
    public int EnemyScore { get; private set; } = 0;
    /// <summary>勝利（または敗北）に必要なスコア</summary>
    public int ScoreToWin { get; private set; } = 2;

    /// <summary>
    /// 初期化処理。GameManagerが存在すれば現在の状態をログ出力します。
    /// </summary>
    void Start()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("[GameScene] ゲームシーン開始。状態: " + GameManager.Instance.CurrentState);
        }
    }

    /// <summary>
    /// 毎フレーム呼ばれる更新処理。デバッグ用クリアキーの入力監視などを行います。
    /// </summary>
    void Update()
    {
        // 本番のクリア条件は Enemy の TankHealth 死亡時に TriggerGameClear が呼ばれます
        if (enableDebugClearKey && Input.GetKeyDown(debugClearKey))
        {
            TriggerGameClear();
        }
    }

    /// <summary>
    /// ゾーンがキャプチャされたときに呼ばれ、スコアの更新と勝敗判定を行います。
    /// </summary>
    /// <param name="winner">キャプチャしたチーム</param>
    public void OnZoneCaptured(Team winner)
    {
        if (hasCleared) return;

        if (winner == Team.Player)
        {
            PlayerScore++;
            if (PlayerScore >= ScoreToWin)
            {
                TriggerGameClear();
            }
        }
        else
        {
            EnemyScore++;
            if (EnemyScore >= ScoreToWin)
            {
                hasCleared = true;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerGameOver();
                }
                else
                {
                    Debug.LogWarning("[GameScene] GameManager が見つからないため、直接 GameOver シーンをロードします。");
                    SceneLoader.LoadGameOver();
                }
            }
        }
    }

    /// <summary>
    /// ゲームクリア処理を実行します。
    /// GameManagerがあればそちらのクリア処理を呼び出し、なければ直接シーン遷移します。
    /// </summary>
    public void TriggerGameClear()
    {
        if (hasCleared)
        {
            return;
        }

        hasCleared = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteGame();
        }
        else
        {
            Debug.LogWarning("[GameScene] GameManager が見つからないため、直接 GameClear シーンをロードします。");
            SceneLoader.LoadGameClear();
        }
    }
}
