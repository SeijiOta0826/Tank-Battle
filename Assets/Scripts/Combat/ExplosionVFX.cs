using UnityEngine;

/// <summary>
/// 弾の着弾・爆発用 VFX（ParticleSystem）。
/// Prefab が無い場合は実行時にパーティクルを自動生成します。
///
/// 【Prefab 構成（推奨）】
/// - 空オブジェクト
/// - ParticleSystem（火花・煙）
/// - この ExplosionVFX スクリプト
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ExplosionVFX : MonoBehaviour
{
    [Header("再生終了後にオブジェクトを消すまでの余裕時間（秒）")]
    [SerializeField]
    float destroyDelay = 1.2f;

    [Header("自動生成時のスケール倍率")]
    [SerializeField]
    float scaleMultiplier = 1f;

    ParticleSystem ps;

    // コンポーネント取得
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    // 生成直後に再生を開始し、指定時間後に自身を破棄する
    void Start()
    {
        ps.Play();
        Destroy(gameObject, destroyDelay);
    }

    /// <summary>
    /// 着弾位置に爆発 VFX を出す。Prefab があればそれを、無ければ自動生成。
    /// </summary>
    /// <param name="position">ワールド座標</param>
    /// <param name="scale">大きさの倍率（パワーアップ取得時は小さめ 0.6 など）</param>
    public static void SpawnAt(Vector3 position, float scale = 1f)
    {
        // 爆発音を再生
        SimpleAudioManager.Instance?.PlayExplosion(position);

        // Bullet の Inspector に Explosion Prefab を割り当てた場合はそちらを優先
        if (Bullet.GlobalExplosionPrefab != null)
        {
            GameObject instance = Instantiate(Bullet.GlobalExplosionPrefab, position, Quaternion.identity);
            instance.transform.localScale *= scale;
            return;
        }

        // Prefabがない場合はランタイムでパーティクルを生成
        CreateRuntimeExplosion(position, scale);
    }

    /// <summary>コードだけで ParticleSystem 爆発を作る（Prefab 未設定時）</summary>
    public static GameObject CreateRuntimeExplosion(Vector3 position, float scale = 1f)
    {
        GameObject root = new GameObject("ExplosionVFX_Runtime");
        root.transform.position = position;
        root.transform.localScale = Vector3.one * scale;

        // パーティクルシステムの追加と設定
        ParticleSystem particleSystem = root.AddComponent<ParticleSystem>();
        ConfigureExplosionParticles(particleSystem);

        // マテリアル剥げ（ピンク化）対策
        ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            // マテリアル剥げ（ピンク化）を防ぐため、利用可能なパーティクル・スプライト用シェーダーから作成します
            Shader shader = Shader.Find("Particles/Standard Unlit") ?? 
                            Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? 
                            Shader.Find("Sprites/Default");
            if (shader != null)
            {
                renderer.material = new Material(shader);
            }
        }

        // クラス自身のアタッチと初期設定（生成後の破棄処理などを委譲）
        ExplosionVFX vfx = root.AddComponent<ExplosionVFX>();
        vfx.scaleMultiplier = scale;
        vfx.destroyDelay = 1.2f;

        return root;
    }

    /// <summary>
    /// パーティクルシステムの各モジュールに対して、爆発らしく見えるパラメータを設定します。
    /// </summary>
    static void ConfigureExplosionParticles(ParticleSystem particleSystem)
    {
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // ── Main（全体）──
        ParticleSystem.MainModule main = particleSystem.main;
        main.duration = 0.4f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.75f, 0.2f),
            new Color(1f, 0.35f, 0.05f));
        main.maxParticles = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;

        // ── Emission（一度だけバースト）──
        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 25, 35)
        });

        // ── Shape（球状に飛び散る）──
        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f;

        // ── Color over Lifetime（消えるときに暗く）──
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
            particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.8f, 0.3f), 0f),
                new GradientColorKey(new Color(0.4f, 0.1f, 0.05f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        // ── Size over Lifetime（広がって消える）──
        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime =
            particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.5f),
            new Keyframe(1f, 1.2f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        particleSystem.Play();
    }
}
