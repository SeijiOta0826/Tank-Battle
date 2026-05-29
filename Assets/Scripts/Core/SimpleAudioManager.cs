using UnityEngine;

/// <summary>
/// プログラムから動的にオーディオクリップを生成し、再生するシンプルなサウンドマネージャーです。
/// シングルトンパターンを使用して、シーン間で共有されます。
/// </summary>
public class SimpleAudioManager : MonoBehaviour
{
    private static SimpleAudioManager _instance;
    
    /// <summary>
    /// SimpleAudioManagerのシングルトンインスタンスを取得します。
    /// </summary>
    public static SimpleAudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SimpleAudioManager");
                _instance = go.AddComponent<SimpleAudioManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    AudioClip shootClip;
    AudioClip explosionClip;
    AudioClip damageClip;
    AudioClip captureTickClip;

    /// <summary>
    /// 起動時に呼ばれ、シングルトンの初期化とオーディオクリップの生成を行います。
    /// </summary>
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        GenerateClips();
    }

    /// <summary>
    /// ゲーム内で使用する各種効果音（発砲音、爆発音など）の波形データを生成します。
    /// </summary>
    void GenerateClips()
    {
        shootClip = CreateTone(0.15f, 600f, 200f, 1); 
        explosionClip = CreateNoise(0.6f);
        damageClip = CreateTone(0.3f, 300f, 50f, 1); 
        captureTickClip = CreateTone(0.05f, 800f, 800f, 0); 
    }

    /// <summary>
    /// 指定したパラメータに基づいてトーン（サイン波または矩形波）のオーディオクリップを生成します。
    /// </summary>
    /// <param name="duration">再生時間（秒）</param>
    /// <param name="startFreq">開始周波数（Hz）</param>
    /// <param name="endFreq">終了周波数（Hz）</param>
    /// <param name="type">波形タイプ（0: サイン波, 1: 矩形波）</param>
    /// <returns>生成されたAudioClip</returns>
    AudioClip CreateTone(float duration, float startFreq, float endFreq, int type)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        float phase = 0f;
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            phase += 2f * Mathf.PI * freq / sampleRate;

            float val = 0;
            if (type == 0) val = Mathf.Sin(phase); // Sine
            else if (type == 1) val = Mathf.Sign(Mathf.Sin(phase)); // Square
            
            // Envelope (fade out)
            val *= (1f - t);
            data[i] = val * 0.3f;
        }
        AudioClip clip = AudioClip.Create("Tone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// ホワイトノイズを用いたオーディオクリップ（主に爆発音用）を生成します。
    /// </summary>
    /// <param name="duration">再生時間（秒）</param>
    /// <returns>生成されたAudioClip</returns>
    AudioClip CreateNoise(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            data[i] = Random.Range(-1f, 1f) * (1f - t) * 0.4f;
        }
        AudioClip clip = AudioClip.Create("Noise", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>発砲音を指定位置で再生します。</summary>
    public void PlayShoot(Vector3 pos) { AudioSource.PlayClipAtPoint(shootClip, pos, 0.4f); }
    /// <summary>爆発音を指定位置で再生します。</summary>
    public void PlayExplosion(Vector3 pos) { AudioSource.PlayClipAtPoint(explosionClip, pos, 0.7f); }
    /// <summary>ダメージ音を指定位置で再生します。</summary>
    public void PlayDamage(Vector3 pos) { AudioSource.PlayClipAtPoint(damageClip, pos, 0.5f); }
    /// <summary>キャプチャー時のティック音を指定位置で再生します。</summary>
    public void PlayCaptureTick(Vector3 pos) { AudioSource.PlayClipAtPoint(captureTickClip, pos, 0.3f); }
}
