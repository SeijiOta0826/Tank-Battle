using System.Collections;
using UnityEngine;

/// <summary>
/// プレイヤーがダメージを受けたときなどに画面を揺らします。
/// Main Camera にアタッチしてください。
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField]
    float defaultDuration = 0.25f;

    [SerializeField]
    float defaultMagnitude = 0.35f;

    public Vector3 CurrentShakeOffset { get; private set; }
    Coroutine shakeCoroutine;

    /// <summary>
    /// シングルトンインスタンスの初期化を行います。
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// 画面の揺れを開始します。引数がマイナスの場合はデフォルト値を使用します。
    /// </summary>
    /// <param name="duration">揺れる時間（秒）</param>
    /// <param name="magnitude">揺れの大きさ</param>
    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        if (duration < 0f)
        {
            duration = defaultDuration;
        }

        if (magnitude < 0f)
        {
            magnitude = defaultMagnitude;
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    /// <summary>
    /// 実際の揺れ処理を行うコルーチンです。
    /// </summary>
    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            CurrentShakeOffset = new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        CurrentShakeOffset = Vector3.zero;
        shakeCoroutine = null;
    }
}
