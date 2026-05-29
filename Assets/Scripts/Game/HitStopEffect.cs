using System.Collections;
using UnityEngine;

/// <summary>
/// 一時的に Time.timeScale を 0 にしてヒットストップ（演出上の停止）を行うクラスです。
/// </summary>
public class HitStopEffect : MonoBehaviour
{
    /// <summary>
    /// 指定した秒数だけヒットストップ演出を発生させます。
    /// </summary>
    /// <param name="duration">停止させる秒数（現実時間）</param>
    public static void Trigger(float duration)
    {
        GameObject go = new GameObject("HitStopEffect");
        DontDestroyOnLoad(go);
        HitStopEffect effect = go.AddComponent<HitStopEffect>();
        effect.StartCoroutine(effect.HitStopRoutine(duration));
    }

    /// <summary>
    /// ヒットストップのコルーチン処理です。
    /// Time.timeScale を操作し、一定時間後に元に戻します。
    /// </summary>
    /// <param name="duration">停止させる秒数</param>
    IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        Destroy(gameObject);
    }
}
