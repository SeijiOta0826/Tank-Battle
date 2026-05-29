/// <summary>
/// 戦車・弾丸がどちらの陣営かを表します。
/// 味方の弾が味方に当たらないよう、Bullet と TankHealth で照合します。
/// </summary>
public enum Team
{
    /// <summary>プレイヤーチーム</summary>
    Player,

    /// <summary>敵チーム</summary>
    Enemy
}
