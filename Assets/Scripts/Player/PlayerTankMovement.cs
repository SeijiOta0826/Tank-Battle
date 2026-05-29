using System.Collections;
using UnityEngine;

/// <summary>
/// カメラの向きに合わせた WASD 移動。
/// 車体は進行方向へ向き、マズル（ノズル）は PlayerTurretAim が別制御します。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerTankMovement : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 12f;

    [SerializeField]
    bool useInertia;

    [SerializeField]
    float bodyRotationSpeed = 720f;

    Rigidbody rb;
    TankHealth health;
    Vector3 moveInput;

    float baseMoveSpeed;
    float speedMultiplier = 1f;
    Coroutine speedBoostCoroutine;

    /// <summary>
    /// コンポーネントの初期化と、物理演算の制約（回転の固定など）を設定します。
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<TankHealth>();
        baseMoveSpeed = moveSpeed;

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    /// <summary>
    /// 毎フレームの入力を取得します。プレイヤーが死亡している場合は入力をリセットします。
    /// </summary>
    void Update()
    {
        if (health != null && health.IsDead)
        {
            moveInput = Vector3.zero;
            return;
        }

        moveInput = CameraPlanarInput.ReadWasdCameraRelative();
    }

    /// <summary>
    /// 物理演算の更新タイミングで、戦車の移動と回転の処理を行います。
    /// </summary>
    void FixedUpdate()
    {
        if (!useInertia && moveInput.sqrMagnitude < 0.01f)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 velocity = moveInput * (baseMoveSpeed * speedMultiplier);
        velocity.y = rb.velocity.y;

        rb.velocity = velocity;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput, Vector3.up);
            Quaternion newRotation = Quaternion.RotateTowards(
                rb.rotation,
                targetRotation,
                bodyRotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }
    }

    /// <summary>
    /// 移動速度を一定時間上昇させるバフを適用します。
    /// </summary>
    /// <param name="multiplier">速度の倍率</param>
    /// <param name="durationSeconds">効果時間（秒）</param>
    public void ApplyMoveSpeedBoost(float multiplier, float durationSeconds)
    {
        if (speedBoostCoroutine != null)
        {
            StopCoroutine(speedBoostCoroutine);
        }

        speedBoostCoroutine = StartCoroutine(MoveSpeedBoostRoutine(multiplier, durationSeconds));
    }

    /// <summary>
    /// 一定時間後に移動速度のバフを解除するコルーチンです。
    /// </summary>
    IEnumerator MoveSpeedBoostRoutine(float multiplier, float durationSeconds)
    {
        speedMultiplier = multiplier;
        yield return new WaitForSeconds(durationSeconds);
        speedMultiplier = 1f;
        speedBoostCoroutine = null;
    }
}
