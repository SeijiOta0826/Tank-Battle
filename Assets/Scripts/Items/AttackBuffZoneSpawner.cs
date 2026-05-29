using System.Collections;
using UnityEngine;

/// <summary>
/// マップ上に定期的に攻撃バフゾーンを生成するスポナー。
/// </summary>
public class AttackBuffZoneSpawner : MonoBehaviour
{
    [SerializeField]
    float spawnInterval = 25f;

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
    /// 攻撃バフゾーンを生成し、設定を初期化します。
    /// </summary>
    void SpawnZone()
    {
        // マップの広さを取得し、生成範囲を決定する
        EnvironmentSetup env = FindFirstObjectByType<EnvironmentSetup>();
        Vector2 playArea = env != null ? env.playAreaSize : new Vector2(80f, 80f);
        float margin = 4f; // 端に生成されないようにするためのマージン
        
        // ランダムなX座標とZ座標を計算
        float randX = Random.Range(-playArea.x / 2f + margin, playArea.x / 2f - margin);
        float randZ = Random.Range(-playArea.y / 2f + margin, playArea.y / 2f - margin);
        
        // 地面の高さを取得してY座標を決定
        float spawnY = GetGroundHeight(randX, randZ, 0.1f);
        Vector3 spawnPos = new Vector3(randX, spawnY, randZ);

        // 新しいゲームオブジェクトを作成し、位置を設定
        GameObject go = new GameObject("AttackBuffZone");
        go.transform.position = spawnPos;

        // トリガーとなるスフィアコライダーを追加
        SphereCollider col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2f;

        // メッシュレンダラーを追加し、半透明の赤色マテリアルを設定
        MeshRenderer rend = go.AddComponent<MeshRenderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"));
        rend.material.color = new Color(1f, 0.2f, 0.2f, 0.15f);

        // ゾーンの視覚的な壁を表示するスクリプトを追加
        ZoneVisualWall wall = go.AddComponent<ZoneVisualWall>();
        wall.radius = 2f;
        wall.height = 3f;

        // バフ処理を行う本体スクリプトを追加
        go.AddComponent<AttackBuffZone>();
        
        // 回転エフェクトとして HealZoneVisual を流用
        go.AddComponent<HealZoneVisual>();

        // 次の生成直前（少し余裕を持たせて1秒前）にこのゾーンを破棄する
        Destroy(go, spawnInterval - 1f);
    }

    /// <summary>
    /// 指定した座標の地面の高さ（Y座標）を取得します。
    /// 上空からRayを飛ばして、地面と判定されるオブジェクトの中で最も高い位置を返します。
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
            // プレイヤー、敵、弾、ブロックなどの動的/障害物オブジェクトは無視する
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Enemy") || hit.collider.name.Contains("Bullet") || hit.collider.name.Contains("Block")) continue;

            // 最も高い位置を更新する
            if (hit.point.y > highestY)
            {
                highestY = hit.point.y;
                hitGround = true;
            }
        }
        
        // 地面が見つかれば少し浮かせた高さを返し、見つからなければデフォルト値を返す
        return hitGround ? highestY + 0.1f : defaultY;
    }
}
