using UnityEngine;

/// <summary>
/// 【旧】簡易球体爆発。現在は ExplosionVFX（ParticleSystem）を推奨します。
/// 互換のため残しています。新規では ExplosionVFX を使ってください。
/// </summary>
public class BulletExplosionEffect : MonoBehaviour
{
    [SerializeField]
    float duration = 0.35f;

    [SerializeField]
    float maxScale = 1.5f;

    float timer;
    Vector3 startScale;

    /// <summary>コードから一瞬だけ爆発っぽい球を出す（非推奨：ExplosionVFX.SpawnAt を使用）</summary>
    public static void SpawnSimple(Vector3 position)
    {
        ExplosionVFX.SpawnAt(position);
    }

    // 初期化時に元のスケールを保存しておく
    void Awake()
    {
        startScale = transform.localScale;
    }

    // 毎フレーム呼ばれ、経過時間に応じてスケールを拡大する
    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration; // 0.0～1.0の進行度

        // 再生時間を超えたら自身を破棄する
        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // 時間経過に合わせて徐々にスケールを大きくする
        transform.localScale = startScale * Mathf.Lerp(0.2f, maxScale, t);
    }
}
