using System.Collections;
using UnityEngine;

/// <summary>
/// キャプチャーゾーン（青ゾーン）内で定期的にアイテムを生成するスポナー。
/// </summary>
public class CaptureZoneItemSpawner : MonoBehaviour
{
    [Header("アイテムがリポップする間隔（秒）")]
    public float spawnInterval = 10f;

    [Header("アイテムを生成する半径（ゾーンの半径より少し小さめ）")]
    public float spawnRadius = 10f;

    // 今回追加する4つのアイテムの種類
    PowerUpType[] spawnTypes = new PowerUpType[]
    {
        PowerUpType.MaxHpUp,
        PowerUpType.AttackUp,
        PowerUpType.Heal,
        PowerUpType.BlastRadiusUp
    };

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // 最初の生成までは少し待つ
        yield return new WaitForSeconds(3f);

        while (true)
        {
            SpawnRandomItem();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRandomItem()
    {
        // ランダムな種類を選択
        PowerUpType selectedType = spawnTypes[Random.Range(0, spawnTypes.Length)];

        // ゾーン内のランダムな位置（円形）を計算
        Vector2 randCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randCircle.x, 0f, randCircle.y);

        // 地面の高さを取得してY座標を調整（めり込み防止）
        spawnPos.y = GetGroundHeight(spawnPos.x, spawnPos.z, transform.position.y) + 1f;

        // アイテムオブジェクトを生成
        GameObject itemObj = new GameObject("ZoneItem_" + selectedType.ToString());
        itemObj.transform.position = spawnPos;

        // コライダーを追加（Trigger）
        SphereCollider col = itemObj.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1.5f;

        // レンダラーを追加（PowerUpスクリプトが親のレンダラーを取得するため）
        itemObj.AddComponent<MeshRenderer>();

        // PowerUp スクリプトを追加して設定
        PowerUp powerUp = itemObj.AddComponent<PowerUp>();
        powerUp.Configure(selectedType);

        // くるくる回すアニメーションはPowerUp内で自動で行われる
    }

    float GetGroundHeight(float x, float z, float defaultY)
    {
        RaycastHit[] hits = Physics.RaycastAll(new Vector3(x, 100f, z), Vector3.down, 200f);
        float highestY = -9999f;
        bool hitGround = false;
        
        foreach (var hit in hits)
        {
            if (hit.collider.isTrigger) continue;
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Enemy") || hit.collider.name.Contains("Bullet")) continue;

            if (hit.point.y > highestY)
            {
                highestY = hit.point.y;
                hitGround = true;
            }
        }
        
        return hitGround ? highestY + 0.1f : defaultY;
    }
}
