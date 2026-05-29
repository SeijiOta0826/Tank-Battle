using UnityEngine;

/// <summary>
/// 敵戦車の移動AI（プレイヤーを追いつつ、左右に揺れて弾を避けやすい動きをする）。
/// プレイヤーと同じく Rigidbody.MovePosition を使用します。
///
/// 【アタッチ先】Enemy 戦車のルートオブジェクト
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnemyTankAI : MonoBehaviour
{
    [Header("移動速度（通常時）")]
    [SerializeField]
    float moveSpeed = 6f;

    [Header("ダッシュ速度（ゾーンやアイテムへ向かう時）")]
    [SerializeField]
    float dashSpeed = 9f;

    [Header("プレイヤーとの理想距離（近づきすぎ・遠ざかりすぎを防ぐ）")]
    [SerializeField]
    float preferredDistance = 8f;

    [Header("左右に揺れる強さ（弾避けの「ぐるっと動く」感じ）")]
    [SerializeField]
    float strafeStrength = 1.2f;

    [Header("揺れの速さ")]
    [SerializeField]
    float strafeFrequency = 2f;

    [SerializeField]
    float rotationSpeed = 540f;

    Rigidbody rb;
    TankHealth health;
    Transform playerTransform;
    Transform captureZoneTransform;

    /// <summary>
    /// 外部からAIの移動速度を設定します。
    /// </summary>
    /// <param name="speed">設定する移動速度</param>
    public void Configure(float speed)
    {
        moveSpeed = speed;
    }

    /// <summary>
    /// 初期化処理。コンポーネントの取得とRigidbodyの物理挙動設定を行います。
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<TankHealth>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    /// <summary>
    /// 開始処理。プレイヤーやキャプチャーゾーンのオブジェクトを検索して参照を保持します。
    /// </summary>
    void Start()
    {
        // Tag「Player」が付いたオブジェクトを探す（プレイヤーに Tag を設定してください）
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        GameObject cz = GameObject.Find("CaptureZone");
        if (cz != null)
        {
            captureZoneTransform = cz.transform;
        }
    }

    /// <summary>
    /// 物理演算のタイミングで呼ばれ、移動と回転のメインロジック（キャプチャー進行、弾避けなど）を実行します。
    /// </summary>
    void FixedUpdate()
    {
        if (health != null && health.IsDead)
        {
            return;
        }

        // A. キャプチャーゾーン及びプレイヤーのステータス確認
        bool isInCaptureZone = false;
        Vector3 captureZonePos = Vector3.zero;
        float captureZoneRadius = 12f;

        if (captureZoneTransform != null)
        {
            captureZonePos = captureZoneTransform.position;
            Vector3 toZone = captureZonePos - transform.position;
            toZone.y = 0f;
            if (toZone.magnitude < captureZoneRadius)
            {
                isInCaptureZone = true;
            }
        }

        Vector3 approach = Vector3.zero;
        Vector3 forwardDir = transform.forward;

        if (isInCaptureZone)
        {
            // 1. すでにゾーン内にいる場合は、プレイヤーを目標として距離 preferredDistance を維持して戦う（中心へ直進しない）
            if (playerTransform != null)
            {
                Vector3 toPlayer = playerTransform.position - transform.position;
                toPlayer.y = 0f;
                float playerDist = toPlayer.magnitude;
                forwardDir = playerDist > 0.01f ? toPlayer.normalized : transform.forward;

                float error = playerDist - preferredDistance;
                approach = forwardDir * Mathf.Clamp(error * 0.35f, -1f, 1f);
            }
            else
            {
                // プレイヤーがいない場合はゾーンの中心方向を見る
                Vector3 toCenter = captureZonePos - transform.position;
                toCenter.y = 0f;
                forwardDir = toCenter.normalized;
            }

            // 【境界バリアロジック】ゾーンの境界（半径 - 1.5m）を越えそうになったら中心に引き戻す
            Vector3 toZoneCenter = captureZonePos - transform.position;
            toZoneCenter.y = 0f;
            float distFromCenter = toZoneCenter.magnitude;
            if (distFromCenter > (captureZoneRadius - 1.5f))
            {
                float pullStrength = (distFromCenter - (captureZoneRadius - 1.5f)) / 1.5f; // 0 ~ 1.0
                approach = Vector3.Lerp(approach, toZoneCenter.normalized, pullStrength * 1.5f);
            }
        }
        else
        {
            // 2. ゾーンの外にいる場合は、ゾーンの中心へ向かう
            if (captureZoneTransform != null)
            {
                Vector3 toZone = captureZonePos - transform.position;
                toZone.y = 0f;
                forwardDir = toZone.normalized;
                approach = forwardDir; // 全力でゾーンに向かう
            }
            else if (playerTransform != null)
            {
                // ゾーン自体が存在しない場合はプレイヤーに向かう
                Vector3 toPlayer = playerTransform.position - transform.position;
                toPlayer.y = 0f;
                float playerDist = toPlayer.magnitude;
                forwardDir = playerDist > 0.01f ? toPlayer.normalized : transform.forward;

                float error = playerDist - preferredDistance;
                approach = forwardDir * Mathf.Clamp(error * 0.35f, -1f, 1f);
            }
        }

        // B. 弾避けベクトルの計算
        Vector3 bulletAvoidVec = CalculateBulletAvoidanceVector();

        // C. アイテム取得ベクトル
        Vector3 itemVec = CalculateItemApproachVector();

        // D. HP2以下のときの回復ゾーン（HealZone）の引き寄せベクトル
        Vector3 healZoneVec = CalculateHealZoneVector();

        // ベクトルの合成
        Vector3 baseMoveDir = approach;
        bool isDashing = false;

        if (itemVec != Vector3.zero)
        {
            // アイテムへの引力を強くブレンド（積極的に取りに行く）
            baseMoveDir = Vector3.Lerp(baseMoveDir, itemVec, 0.7f);
            isDashing = true; // アイテムへ向かう時はダッシュ
        }
        else if (healZoneVec != Vector3.zero)
        {
            // 回復ゾーンへの引力を適度にブレンド
            baseMoveDir = Vector3.Lerp(baseMoveDir, healZoneVec, 0.35f);
        }
        
        // ゾーン外からゾーン中心に向かう場合もダッシュ
        if (!isInCaptureZone && captureZoneTransform != null)
        {
            isDashing = true;
        }

        // 左右のストレイフ（微細な揺れ）
        Vector3 right = Vector3.Cross(Vector3.up, forwardDir);
        float strafe = Mathf.Sin(Time.time * strafeFrequency) * strafeStrength;
        Vector3 strafeVec = right * strafe;

        // 最終移動ベクトルの決定
        Vector3 finalMoveVec = (baseMoveDir + strafeVec + bulletAvoidVec).normalized;

        float currentSpeed = isDashing ? dashSpeed : moveSpeed;
        Vector3 velocity = finalMoveVec * currentSpeed;
        velocity.y = rb.velocity.y;

        Vector3 newPosition = rb.position + velocity * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // 車体の回転処理
        if (finalMoveVec.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(finalMoveVec, Vector3.up);
            Quaternion newRotation = Quaternion.RotateTowards(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }
    }

    /// <summary>
    /// 周囲の弾を検出し、自分に向かって飛んでいる危険な弾から直交方向に避けるベクトルを計算します。
    /// </summary>
    Vector3 CalculateBulletAvoidanceVector()
    {
        Vector3 avoidVec = Vector3.zero;
        Collider[] cols = Physics.OverlapSphere(transform.position, 8f);
        float closestDist = float.MaxValue;
        Vector3 dangerBulletDir = Vector3.zero;
        Vector3 dangerBulletVel = Vector3.zero;

        foreach (Collider col in cols)
        {
            Bullet bullet = col.GetComponentInParent<Bullet>();
            if (bullet != null)
            {
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb != null)
                {
                    Vector3 bulletVel = bulletRb.velocity;
                    Vector3 toSelf = transform.position - col.transform.position;
                    float dist = toSelf.magnitude;

                    // 弾が自分に向かって飛んできているかチェック（相対速度と方向の内積）
                    float approachSpeed = Vector3.Dot(bulletVel, toSelf.normalized);
                    if (approachSpeed > 1.0f) 
                    {
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            dangerBulletDir = toSelf.normalized;
                            dangerBulletVel = bulletVel;
                        }
                    }
                }
            }
        }

        if (closestDist < float.MaxValue)
        {
            Vector3 bulletForward = dangerBulletVel.normalized;
            Vector3 rightAvoid = Vector3.Cross(Vector3.up, bulletForward);

            // 弾から自分への方向と直交ベクトルの内積で、避ける左右方向を決める
            float dot = Vector3.Dot(dangerBulletDir, rightAvoid);
            Vector3 chosenAvoidDir = dot >= 0f ? rightAvoid : -rightAvoid;

            // 距離が近いほど強く回避する（最大強さ 2.5f）
            float avoidForce = Mathf.Clamp01(1f - (closestDist / 8f)) * 2.5f;
            avoidVec = chosenAvoidDir * avoidForce;
        }

        return avoidVec;
    }

    /// <summary>
    /// 周囲のパワーアップアイテムを検出し、最も近いアイテムへ向かうベクトルを計算します。
    /// </summary>
    Vector3 CalculateItemApproachVector()
    {
        PowerUp[] items = FindObjectsByType<PowerUp>(FindObjectsSortMode.None);
        if (items.Length == 0) return Vector3.zero;

        PowerUp closestItem = null;
        float minDist = 20f; // 索敵範囲

        foreach (var item in items)
        {
            float dist = Vector3.Distance(transform.position, item.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestItem = item;
            }
        }

        if (closestItem != null)
        {
            Vector3 dir = closestItem.transform.position - transform.position;
            dir.y = 0f;
            return dir.normalized;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// HPが2以下の場合、シーン内の最も近い HealZone へ向かうベクトルを計算します。
    /// </summary>
    Vector3 CalculateHealZoneVector()
    {
        if (health == null || health.CurrentHp > 2)
        {
            return Vector3.zero;
        }

        HealZone[] healZones = FindObjectsByType<HealZone>(FindObjectsSortMode.None);
        if (healZones.Length == 0)
        {
            return Vector3.zero;
        }

        HealZone closestZone = null;
        float minDist = float.MaxValue;

        foreach (var zone in healZones)
        {
            float dist = Vector3.Distance(transform.position, zone.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestZone = zone;
            }
        }

        if (closestZone != null)
        {
            Vector3 dir = closestZone.transform.position - transform.position;
            dir.y = 0f;
            return dir.normalized;
        }

        return Vector3.zero;
    }
}
