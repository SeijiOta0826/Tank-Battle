using UnityEngine;

/// <summary>
/// 床（Plane）の上に、ランダムな灰色のブロックを配置します。
/// </summary>
public class StageObstacleSpawner : MonoBehaviour
{
    [Header("ブロックの個数")]
    [SerializeField]
    int blockCount = 14;

    [Header("配置エリアの半分の幅・奥行き（Plane Scale 12 なら 55 前後）")]
    [SerializeField]
    Vector2 areaHalfSize = new Vector2(52f, 52f);

    [Header("ブロックの一辺の長さ（ランダム範囲）")]
    [SerializeField]
    Vector2 blockScaleRange = new Vector2(1.5f, 4.5f);

    [SerializeField]
    Color blockColor = new Color(0.45f, 0.45f, 0.45f);

    [Header("同じ位置に何度も作らない（シーン再読み込み用）")]
    [SerializeField]
    bool clearExistingOnSpawn = true;

    /// <summary>
    /// ブロックの生成設定を外部から適用します。
    /// </summary>
    /// <param name="count">生成するブロックの個数</param>
    /// <param name="halfSize">配置エリアの半分のサイズ</param>
    /// <param name="scaleRange">ブロックのスケールの最小・最大値</param>
    public void ApplySettings(int count, Vector2 halfSize, Vector2 scaleRange)
    {
        blockCount = count;
        areaHalfSize = halfSize;
        blockScaleRange = scaleRange;
    }

    /// <summary>
    /// 指定された設定に基づいてブロック（障害物）を生成します。
    /// 既存のブロックがある場合は削除してから再生成します。
    /// </summary>
    public void SpawnBlocks()
    {
        // すでにブロック群が存在する場合は削除する（重複防止）
        if (clearExistingOnSpawn)
        {
            Transform existing = transform.Find("Obstacles");
            if (existing != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(existing.gameObject); // プレイ中はDestroyを使用
                }
                else
                {
                    DestroyImmediate(existing.gameObject); // エディタモードではDestroyImmediateを使用
                }
            }
        }

        // 新しいブロックの親となる空のゲームオブジェクトを作成
        GameObject parent = new GameObject("Obstacles");
        parent.transform.SetParent(transform);

        // 指定された個数分だけブロックを生成
        for (int i = 0; i < blockCount; i++)
        {
            CreateBlock(parent.transform, i);
        }
    }

    /// <summary>
    /// 単一のブロックを生成し、ランダムなサイズと位置に配置します。
    /// </summary>
    /// <param name="parent">ブロックの親となるTransform</param>
    /// <param name="index">ブロックのインデックス番号（名前用）</param>
    void CreateBlock(Transform parent, int index)
    {
        // キューブのプリミティブを生成
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = "Block_" + index;

        // ランダムな基準サイズを決定し、Y軸のみ少しばらつきを持たせる
        float size = Random.Range(blockScaleRange.x, blockScaleRange.y);
        block.transform.localScale = new Vector3(size, size * Random.Range(0.6f, 1.4f), size);

        // 指定エリア内でランダムなX, Z座標を決定
        float posX = Random.Range(-areaHalfSize.x, areaHalfSize.x);
        float posZ = Random.Range(-areaHalfSize.y, areaHalfSize.y);
        
        // キューブの中心がY=0ではなく、底面がY=0になるように高さを調整
        float posY = block.transform.localScale.y * 0.5f;

        // ブロックの位置と親を設定
        block.transform.position = new Vector3(posX, posY, posZ);
        block.transform.SetParent(parent);

        // マテリアルの色を指定した色に変更
        Renderer renderer = block.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = blockColor;
        }
    }
}
