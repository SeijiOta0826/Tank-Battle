using UnityEngine;

/// <summary>
/// Game シーン開始時に、弾の共通爆発 Prefab を登録します（任意）。
///
/// 【アタッチ先】Game シーンの GameSceneRoot（GameSceneController と同じオブジェクトでOK）
/// 【Inspector】Explosion Prefab に ExplosionVFX 付き Prefab を割り当て（無くても自動生成で動作）
/// </summary>
public class CombatVFXSetup : MonoBehaviour
{
    [Header("全弾で使う爆発 Prefab（ExplosionVFX 付き推奨）")]
    [SerializeField]
    GameObject explosionPrefab;

    [Header("未設定のとき実行時にパーティクル爆発を自動生成する")]
    [SerializeField]
    bool useRuntimeExplosionIfEmpty = true;

    /// <summary>
    /// シーン開始時に呼ばれ、設定されたPrefabをグローバルな爆発エフェクトとして登録します。
    /// </summary>
    void Awake()
    {
        if (explosionPrefab != null)
        {
            Bullet.GlobalExplosionPrefab = explosionPrefab;
            Debug.Log("[CombatVFXSetup] 共通爆発 Prefab を登録しました。");
            return;
        }

        if (useRuntimeExplosionIfEmpty)
        {
            Bullet.GlobalExplosionPrefab = null;
            Debug.Log("[CombatVFXSetup] 爆発は ExplosionVFX の実行時生成を使用します。");
        }
    }

    /// <summary>
    /// オブジェクト破棄時に呼ばれ、登録したPrefabをクリアします。
    /// </summary>
    void OnDestroy()
    {
        if (Bullet.GlobalExplosionPrefab == explosionPrefab)
        {
            Bullet.GlobalExplosionPrefab = null;
        }
    }
}
