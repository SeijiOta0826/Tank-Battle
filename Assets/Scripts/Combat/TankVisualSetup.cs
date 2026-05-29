using UnityEngine;

/// <summary>
/// 球体戦車にノズル（砲口）の見た目と FirePoint を追加します。
/// シーンに既に置いてある Player / Enemy に Awake で自動適用されます。
/// </summary>
public class TankVisualSetup : MonoBehaviour
{
    [Header("ノズルが無いとき自動で作る")]
    [SerializeField]
    bool createNozzleIfMissing = true;

    /// <summary>ノズル階層を作り、FirePoint の Transform を返す</summary>
    /// <param name="tankRoot">戦車のルートTransform</param>
    /// <param name="nozzleColor">ノズルの色</param>
    /// <param name="muzzleFixedToBody">true のときマズルは車体の前方固定（プレイヤー向け）</param>
    public static Transform EnsureNozzleHierarchy(
        Transform tankRoot,
        Color? nozzleColor = null,
        bool muzzleFixedToBody = false)
    {
        // 既にノズルのピボットが存在するか確認
        Transform existingPivot = tankRoot.Find("NozzlePivot");
        if (existingPivot != null)
        {
            // 車体固定フラグが立っている場合は角度を調整
            if (muzzleFixedToBody)
            {
                existingPivot.localRotation = Quaternion.Euler(-15f, 0f, 0f);
            }

            // FirePointが存在すればそれを返す
            Transform existingFire = existingPivot.Find("FirePoint");
            if (existingFire != null)
            {
                return existingFire;
            }
        }

        // 以前の単体 FirePoint（球の子）があれば削除してノズル構造に統一
        Transform legacyFirePoint = tankRoot.Find("FirePoint");
        if (legacyFirePoint != null && legacyFirePoint.parent == tankRoot)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(legacyFirePoint.gameObject);
            }
            else
            {
                Object.DestroyImmediate(legacyFirePoint.gameObject);
            }
        }

        // ノズルのピボット（回転の軸）を作成
        GameObject pivotObject = new GameObject("NozzlePivot");
        pivotObject.transform.SetParent(tankRoot);
        pivotObject.transform.localPosition = new Vector3(0f, 0.15f, 0.35f);
        pivotObject.transform.localRotation = muzzleFixedToBody
            ? Quaternion.Euler(-15f, 0f, 0f)
            : Quaternion.Euler(-18f, 0f, 0f);

        // ノズルの見た目（Cube）を作成
        GameObject nozzleObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nozzleObject.name = "Nozzle";
        nozzleObject.transform.SetParent(pivotObject.transform);
        nozzleObject.transform.localPosition = new Vector3(0f, 0f, 0.8f);
        nozzleObject.transform.localScale = new Vector3(0.28f, 0.22f, 1.5f);

        // ノズル自体の当たり判定は不要なため削除
        Collider nozzleCollider = nozzleObject.GetComponent<Collider>();
        if (nozzleCollider != null)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(nozzleCollider);
            }
            else
            {
                Object.DestroyImmediate(nozzleCollider);
            }
        }

        // ノズルの色を設定
        Renderer nozzleRenderer = nozzleObject.GetComponent<Renderer>();
        if (nozzleRenderer != null)
        {
            Color color = nozzleColor ?? new Color(0.25f, 0.25f, 0.28f);
            nozzleRenderer.material.color = color;
        }

        // 弾の発射位置となるオブジェクトを作成
        GameObject firePointObject = new GameObject("FirePoint");
        firePointObject.transform.SetParent(pivotObject.transform);
        firePointObject.transform.localPosition = new Vector3(0f, 0f, 1.6f);
        firePointObject.transform.localRotation = Quaternion.identity;

        return firePointObject.transform;
    }

    void Awake()
    {
        if (!createNozzleIfMissing)
        {
            return;
        }

        EnsureNozzleHierarchy(transform);
    }
}
