using System.Collections;
using UnityEngine;

/// <summary>
/// マップ上に定期的に回復ゾーン（HealZone）を生成するスポナー。
/// </summary>
public class HealZoneSpawner : MonoBehaviour
{
    [SerializeField]
    float spawnInterval = 15f;

    [SerializeField]


    Vector3 centerPosition = Vector3.zero;

    /// <summary>
    /// 生成の基準となる中心座標を設定します。
    /// </summary>
    /// <param name="center">中心座標</param>
    public void SetCenterPosition(Vector3 center)
    {
        centerPosition = center;
    }

    /// <summary>
    /// 起動時に呼ばれ、一定間隔でゾーンを生成するコルーチンを開始します。
    /// </summary>
    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// 指定された間隔（spawnInterval）ごとにゾーンを生成し続けるコルーチン。
    /// </summary>
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnZone();
        }
    }

    /// <summary>
    /// 回復ゾーンを生成し、各種コンポーネントをアタッチして初期化します。
    /// </summary>
    void SpawnZone()
    {
        // マップの広さを取得し、生成範囲を決定する
        EnvironmentSetup env = FindFirstObjectByType<EnvironmentSetup>();
        Vector2 playArea = env != null ? env.playAreaSize : new Vector2(80f, 80f);
        float margin = 4f; // 壁や端に近すぎないようにマージンを設ける
        
        // ランダムなX座標とZ座標を計算
        float randX = Random.Range(-playArea.x / 2f + margin, playArea.x / 2f - margin);
        float randZ = Random.Range(-playArea.y / 2f + margin, playArea.y / 2f - margin);
        
        // 地面の高さを取得してY座標を決定
        float spawnY = GetGroundHeight(randX, randZ, 0.1f);
        Vector3 spawnPos = new Vector3(randX, spawnY, randZ);

        // 新しいゲームオブジェクトを作成
        GameObject go = new GameObject("HealZone");
        go.transform.position = spawnPos;

        // トリガーとなるスフィアコライダーを追加
        SphereCollider col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 4f;

        // メッシュレンダラーを追加し、半透明の緑色マテリアルを設定
        MeshRenderer rend = go.AddComponent<MeshRenderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"));
        rend.material.color = new Color(0.2f, 1f, 0.2f, 0.15f);

        // ゾーンの視覚的な壁（円柱状のエフェクトなど）を表示するスクリプトを追加
        ZoneVisualWall wall = go.AddComponent<ZoneVisualWall>();
        wall.radius = 4f;
        wall.height = 3f;

        // 回復処理の本体と、回転などの視覚効果スクリプトを追加
        go.AddComponent<HealZone>();
        go.AddComponent<HealZoneVisual>();

        // 次のゾーンが生成される少し前に、このゾーンを自動的に破棄する
        Destroy(go, spawnInterval - 1f); // Destroy before next one spawns
    }

    /// <summary>
    /// 指定した座標の地面の高さ（Y座標）を取得します。
    /// </summary>
    /// <param name="x">調べるX座標</param>
    /// <param name="z">調べるZ座標</param>
    /// <param name="defaultY">地面が見つからなかった場合のデフォルトの高さ</param>
    /// <returns>地面の高さ</returns>
    float GetGroundHeight(float x, float z, float defaultY)
    {
        // 遥か上空から下に向かってRayを飛ばす
        RaycastHit[] hits = Physics.RaycastAll(new Vector3(x, 100f, z), Vector3.down, 200f);
        float highestY = -9999f;
        bool hitGround = false;
        
        foreach (var hit in hits)
        {
            // トリガーは無視する
            if (hit.collider.isTrigger) continue;
            
            // プレイヤー、敵、弾、ブロックなどの動的オブジェクト・障害物は無視する
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Enemy") || hit.collider.name.Contains("Bullet") || hit.collider.name.Contains("Block")) continue;

            // より高い地面（オブジェクト）があれば更新
            if (hit.point.y > highestY)
            {
                highestY = hit.point.y;
                hitGround = true;
            }
        }
        
        // 地面が見つかれば少しだけ上（+0.1f）の高さを返す
        return hitGround ? highestY + 0.1f : defaultY;
    }
}
