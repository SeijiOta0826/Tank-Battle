using UnityEngine;

/// <summary>
/// ステージの終端を隠すためのスモッグ（Fog）とスカイボックス設定、
/// および画面外へ落ちないための移動制限（透明な壁）を自動生成するスクリプトです。
/// </summary>
public class EnvironmentSetup : MonoBehaviour
{
    [Header("スモッグ (Fog) 設定")]
    public bool enableFog = true;
    public Color fogColor = new Color(0.6f, 0.6f, 0.6f);
    public float fogDensity = 0.02f;

    [Header("移動制限 (Invisible Walls)")]
    public bool createWalls = true;
    public Vector2 playAreaSize = new Vector2(80f, 80f); // 広めの移動範囲
    public float wallHeight = 20f;

    /// <summary>
    /// シーン開始時に呼ばれ、Fogや背景色、透明な壁の生成を行います。
    /// </summary>
    void Awake()
    {
        // 1. スモッグ(Fog)の設定
        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = fogDensity;
        }

        // 2. カメラの背景色をFogに合わせる（Skyboxがない場合でも自然に馴染むように）
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = fogColor;
            // Skyboxがあればそれを使い、なければ背景色で塗りつぶす
            Camera.main.clearFlags = CameraClearFlags.Skybox; 
        }

        // 3. 画面外落下防止の透明な壁（Collider）を配置する
        if (createWalls)
        {
            GameObject wallsRoot = new GameObject("InvisibleWalls");
            
            // 厚みを2mにしてすり抜けにくくする
            // 奥 (Z+)
            CreateInvisibleWall(wallsRoot.transform, new Vector3(0, wallHeight / 2, playAreaSize.y / 2), new Vector3(playAreaSize.x, wallHeight, 2f));
            // 手前 (Z-)
            CreateInvisibleWall(wallsRoot.transform, new Vector3(0, wallHeight / 2, -playAreaSize.y / 2), new Vector3(playAreaSize.x, wallHeight, 2f));
            // 右 (X+)
            CreateInvisibleWall(wallsRoot.transform, new Vector3(playAreaSize.x / 2, wallHeight / 2, 0), new Vector3(2f, wallHeight, playAreaSize.y));
            // 左 (X-)
            CreateInvisibleWall(wallsRoot.transform, new Vector3(-playAreaSize.x / 2, wallHeight / 2, 0), new Vector3(2f, wallHeight, playAreaSize.y));
        }
    }

    /// <summary>
    /// 透明な壁（コライダーのみのオブジェクト）を生成して配置します。
    /// </summary>
    /// <param name="parent">壁オブジェクトの親となるTransform</param>
    /// <param name="position">配置するローカル座標</param>
    /// <param name="scale">壁のサイズ（BoxColliderのサイズ）</param>
    void CreateInvisibleWall(Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject wall = new GameObject("InvisibleWall");
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        
        BoxCollider col = wall.AddComponent<BoxCollider>();
        col.size = scale;
        // 物理的な壁として機能させるため、isTrigger にはしない
    }

    /// <summary>
    /// エディタ上で壁の配置範囲を視覚的に確認するためのギズモを描画します。
    /// </summary>
    void OnDrawGizmos()
    {
        if (!createWalls) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        // 壁の範囲を赤いワイヤーフレームで描画
        Gizmos.DrawWireCube(new Vector3(0, wallHeight / 2, 0), new Vector3(playAreaSize.x, wallHeight, playAreaSize.y));
    }
}
