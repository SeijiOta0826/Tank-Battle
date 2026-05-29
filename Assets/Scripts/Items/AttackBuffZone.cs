using UnityEngine;

/// <summary>
/// ゾーン内にいる戦車の攻撃速度（連射速度）をアップさせる。
/// </summary>
public class AttackBuffZone : MonoBehaviour
{


    /// <summary>
    /// 他のコライダーがこのゾーン（トリガー）内に留まっている間に毎フレーム呼ばれます。
    /// </summary>
    /// <param name="other">ゾーン内にいるオブジェクトのコライダー</param>
    void OnTriggerStay(Collider other)
    {
        // 侵入したオブジェクトの親からTankAutoShooterコンポーネントを取得
        TankAutoShooter shooter = other.GetComponentInParent<TankAutoShooter>();
        
        // 取得できた場合（戦車だった場合）、攻撃力バフのフラグをオンにする
        if (shooter != null)
        {
            shooter.hasAttackBuff = true;
        }
    }
}
