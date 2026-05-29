using UnityEngine;

/// <summary>
/// プレイヤー球体を上から追いかけるカメラ。
/// GameSceneBootstrap が自動で Main Camera に付けます。
/// </summary>
public class SimpleFollowCamera : MonoBehaviour
{
    [SerializeField]
    Transform target;

    [SerializeField]
    Vector3 offset = new Vector3(0f, 18f, -12f);

    [SerializeField]
    float followSmooth = 6f;

    /// <summary>
    /// カメラが追いかけるターゲットを設定します。
    /// </summary>
    /// <param name="newTarget">新しい追従対象（プレイヤーなど）</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// 毎フレームの終わりに呼ばれる更新処理。
    /// ターゲットが存在する場合、滑らかに追従して位置と回転を更新します。
    /// </summary>
    void LateUpdate()
    {
        // ターゲットが設定されていない場合は何もしない
        if (target == null)
        {
            return;
        }

        // ターゲットの位置にオフセットを加えた目標位置を計算
        Vector3 desired = target.position + offset;
        
        // 滑らかに移動するための補間割合を計算（フレームレートに依存しない指数減衰）
        float t = 1f - Mathf.Exp(-followSmooth * Time.deltaTime);
        
        // 現在位置から目標位置へ線形補間で移動
        transform.position = Vector3.Lerp(transform.position, desired, t);
        
        // カメラの角度を固定（見下ろす角度）
        transform.rotation = Quaternion.Euler(52f, 0f, 0f);
    }
}
