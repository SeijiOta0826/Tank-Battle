using UnityEngine;

/// <summary>
/// ステージ下に置く「落下死亡」ゾーン。
/// プレイヤーがここに触れると即ゲームオーバーになります。
///
/// 【配置】床より下に大きな Box（Is Trigger オン）を置く
/// </summary>
public class FallDeathZone : MonoBehaviour
{
    /// <summary>
    /// 他のオブジェクトがこのゾーン（トリガー）に侵入した際に呼ばれます。
    /// プレイヤーが侵入した場合は即死処理を行います。
    /// </summary>
    /// <param name="other">侵入したオブジェクトのコライダー</param>
    void OnTriggerEnter(Collider other)
    {
        TankHealth health = other.GetComponent<TankHealth>();
        if (health == null)
        {
            return;
        }

        if (health.Team != Team.Player)
        {
            return;
        }

        Debug.Log("[FallDeathZone] プレイヤーが落下しました。");
        health.KillInstant();
    }
}
