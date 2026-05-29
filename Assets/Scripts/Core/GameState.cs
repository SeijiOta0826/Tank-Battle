/// <summary>
/// ゲーム全体の「今どの段階か」を表す列挙型です。
/// GameManager がこの値を持ち、シーンやUIはこの状態を見て動きを変えます。
/// </summary>
public enum GameState
{
    /// <summary>タイトル画面（まだプレイしていない）</summary>
    Title,

    /// <summary>ゲーム本編プレイ中</summary>
    Playing,

    /// <summary>一時停止中（将来のポーズ機能用。今は未使用でもOK）</summary>
    Paused,

    /// <summary>ステージクリア・ゲームクリア画面</summary>
    GameClear,

    /// <summary>ゲームオーバー（プレイヤー敗北）</summary>
    GameOver
}
