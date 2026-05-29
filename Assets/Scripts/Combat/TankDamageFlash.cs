using System.Collections;
using UnityEngine;

/// <summary>
/// ダメージを受けたとき、戦車の見た目を赤く点滅させます。
/// </summary>
public class TankDamageFlash : MonoBehaviour
{
    [SerializeField]
    float flashDuration = 0.35f;

    [SerializeField]
    int flashCount = 3;

    [SerializeField]
    Color flashColor = new Color(1f, 0.15f, 0.15f);

    Renderer[] renderers;
    Color[] originalColors;
    Coroutine flashCoroutine;

    // 子オブジェクトのすべてのRendererを取得し、元の色を保存しておく
    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
    }

    /// <summary>
    /// 回復した時の緑色の点滅エフェクトを再生します。
    /// </summary>
    public void PlayHealFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine(Color.green));
    }

    /// <summary>
    /// ダメージを受けた時の赤色の点滅エフェクトを再生します。
    /// </summary>
    public void PlayFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine(flashColor));
    }

    // 指定された色で一定回数点滅するコルーチン
    IEnumerator FlashRoutine(Color colorToFlash)
    {
        // 1回の点滅（オンとオフ）にかかる時間の半分を計算
        float interval = flashDuration / (flashCount * 2);

        for (int i = 0; i < flashCount; i++)
        {
            SetAllColors(colorToFlash);
            yield return new WaitForSeconds(interval);
            RestoreColors();
            yield return new WaitForSeconds(interval);
        }

        flashCoroutine = null;
    }

    void SetAllColors(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                renderers[i].material.color = color;
            }
        }
    }

    void RestoreColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }
}
