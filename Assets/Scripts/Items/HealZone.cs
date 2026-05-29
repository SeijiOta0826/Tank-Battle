using UnityEngine;

/// <summary>
/// プレイヤーが留まっている間、一定間隔で HP を回復するゾーン。
/// 「励ます要素」としてステージの安全地帯などに配置します。
///
/// 【配置】床の上に薄い Cube / Cylinder
/// - Box Collider: Is Trigger オン
/// - Mesh Renderer は半透明の緑など（任意）
/// </summary>
public class HealZone : MonoBehaviour
{
    [Header("1回の回復量")]
    [SerializeField]
    int healAmountPerTick = 1;

    [Header("回復の間隔（秒）")]
    [SerializeField]
    float healInterval = 1f;

    [Header("ゾーン内にいる間だけ回復（Trigger 必須）")]
    [SerializeField]
    bool requireStayInside = true;

    float healTimer;

    /// <summary>
    /// 他のコライダーがこのゾーン内に留まっている間呼ばれます。
    /// </summary>
    /// <param name="other">ゾーン内にいるオブジェクトのコライダー</param>
    void OnTriggerStay(Collider other)
    {
        // 滞在判定が必要ない設定の場合は無視（※現仕様では基本的に true を想定）
        if (!requireStayInside)
        {
            return;
        }

        TryHeal(other);
    }

    /// <summary>
    /// 他のコライダーがゾーンに入った瞬間に呼ばれます。
    /// </summary>
    /// <param name="other">侵入したオブジェクトのコライダー</param>
    void OnTriggerEnter(Collider other)
    {
        // 入った瞬間にも少し待ってから回復（Stay と併用するためタイマーをリセット）
        healTimer = 0f;
    }

    /// <summary>
    /// ゾーン内にいるオブジェクトに対して回復を試みます。
    /// </summary>
    /// <param name="other">対象オブジェクトのコライダー</param>
    void TryHeal(Collider other)
    {
        // 戦車のHPコンポーネントを取得
        TankHealth health = other.GetComponent<TankHealth>();
        
        // 取得できない、またはすでに死亡している場合は回復しない
        if (health == null || health.IsDead)
        {
            return;
        }

        // すでにHPが最大の場合は回復しない
        if (health.CurrentHp >= health.MaxHp)
        {
            return;
        }

        // タイマーを進める
        healTimer += Time.deltaTime;
        
        // インターバル（回復間隔）に達していなければ処理終了
        if (healTimer < healInterval)
        {
            return;
        }

        // インターバルに達したのでタイマーをリセットし回復処理を実行
        healTimer = 0f;
        bool healed = health.Heal(healAmountPerTick);
        
        // 実際に回復できた場合のみログを出力
        if (healed)
        {
            Debug.Log($"[HealZone] {other.name} を {healAmountPerTick} 回復。");
        }
    }
}
