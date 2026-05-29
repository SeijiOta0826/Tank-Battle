using UnityEngine;

/// <summary>
/// カメラの向きを XZ 平面に投影し、移動・照準入力の基準にします。
/// </summary>
public static class CameraPlanarInput
{
    /// <summary>
    /// メインカメラの向きを基に、XZ平面上の前方(forward)と右方向(right)のベクトルを計算します。
    /// </summary>
    public static void GetCameraAxes(out Vector3 forward, out Vector3 right)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            forward = Vector3.forward;
            right = Vector3.right;
            return;
        }

        forward = cam.transform.forward;
        forward.y = 0f;
        // 真上や真下を向いている場合のフェイルセーフ
        if (forward.sqrMagnitude < 0.01f)
        {
            forward = Vector3.forward;
        }
        else
        {
            forward.Normalize();
        }

        right = Vector3.Cross(Vector3.up, forward);
    }

    /// <summary>WASD をカメラ基準の XZ 方向ベクトルに変換</summary>
    public static Vector3 ReadWasdCameraRelative()
    {
        GetCameraAxes(out Vector3 forward, out Vector3 right);

        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            direction += forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            direction -= forward;
        }

        if (Input.GetKey(KeyCode.D))
        {
            direction += right;
        }

        if (Input.GetKey(KeyCode.A))
        {
            direction -= right;
        }

        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }

        return direction;
    }
}
