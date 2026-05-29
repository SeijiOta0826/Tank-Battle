/// <summary>
/// パワーアップアイテムの種類。
/// PowerUp コンポーネントの Inspector で選びます。
/// </summary>
public enum PowerUpType
{
    /// <summary>即座に HP を回復</summary>
    Heal,

    /// <summary>一定時間、移動速度アップ</summary>
    MoveSpeedBoost,

    /// <summary>一定時間、弾の発射レートアップ</summary>
    FireRateBoost,

    /// <summary>最大HPを永続的に増加し、回復する</summary>
    MaxHpUp,

    /// <summary>攻撃力を永続的に増加する</summary>
    AttackUp,

    /// <summary>弾の爆風範囲を永続的に拡大する</summary>
    BlastRadiusUp
}
