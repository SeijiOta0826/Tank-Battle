using UnityEngine;

/// <summary>
/// HP回復ゾーンの見た目を緑色の半透明にする補助スクリプト（任意）。
/// HealZone と同じオブジェクトにアタッチしてください。
/// </summary>
[RequireComponent(typeof(Renderer))]
public class HealZoneVisual : MonoBehaviour
{
    /// <summary>
    /// 初期化時に呼ばれ、マテリアルを半透明（Transparent）に変更します。
    /// </summary>
    void Awake()
    {
        // 自身のアタッチされているレンダラーを取得
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        // 実行時マテリアルを取得し、透過度（アルファ値）を設定
        // （Built-in RP 想定。URP の場合は Inspector でマテリアル指定推奨）
        Material mat = renderer.material;
        Color c = mat.color;
        c.a = 0.4f; // 透過度だけ設定
        mat.color = c;

        // 標準シェーダー（Standard Shader）の設定を書き換え、透明度を有効化する
        mat.SetFloat("_Mode", 3f); // 3 = Transparent モード
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0); // 深度の書き込みを無効化（半透明描画の基本）
        
        // シェーダーのキーワードを調整し、アルファブレンドを有効にする
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        
        // 描画順序を半透明オブジェクト向け（3000）に設定
        mat.renderQueue = 3000;
    }
}
