using UnityEngine;

/// <summary>
/// 重力付きの弾丸。地面や戦車に当たると爆発し、範囲ダメージを与えます。
/// 弾丸の物理的挙動、ダメージ処理、および爆発の生成を担当します。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public static GameObject GlobalExplosionPrefab { get; set; }

    [Header("プレイヤー弾（飛距離を長く）")]
    [SerializeField]
    float playerLaunchSpeed = 28f;

    [SerializeField]
    float playerLifeTime = 10f;

    [Header("敵弾")]
    [SerializeField]
    float enemyLaunchSpeed = 22f;

    [SerializeField]
    float enemyLifeTime = 8f;

    [SerializeField]
    int damage = 1;

    [Header("着弾時の爆発ダメージ範囲")]
    [SerializeField]
    float playerExplosionRadius = 4f;

    [SerializeField]
    float enemyExplosionRadius = 2.2f;

    [Header("弾の質量（小さいほど戦車を押しにくい）")]
    [SerializeField]
    float bulletMass = 0.02f;

    [Header("着弾時に戦車の速度を残す割合（0に近いほど押されにくい）")]
    [SerializeField]
    float tankVelocityKeepRatio = 0.08f;

    [SerializeField]
    GameObject explosionPrefab;

    int currentDamage;
    float currentExplosionRadius;

    Team ownerTeam;
    Rigidbody rb;
    bool initialized;
    bool hasExploded;

    // コンポーネント初期化時に質量を設定
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = bulletMass;
    }

    /// <summary>
    /// 物理挙動のテンプレートを設定します。
    /// 重力を有効にし、衝突判定モードを連続的（ContinuousDynamic）にしてすり抜けを防ぎます。
    /// </summary>
    public void ConfigurePhysicsTemplate()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.mass = bulletMass;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    /// <summary>
    /// 弾丸を発射するための初期化処理を行います。
    /// 発射元チームに応じた速度、寿命、および向きを設定します。
    /// </summary>
    /// <param name="team">発射元のチーム</param>
    /// <param name="direction">発射方向</param>
    public void Initialize(Team team, Vector3 direction)
    {
        ownerTeam = team;
        hasExploded = false;
        initialized = true;

        // チームに応じて速度と寿命を決定
        float speed = team == Team.Player ? playerLaunchSpeed : enemyLaunchSpeed;
        float life = team == Team.Player ? playerLifeTime : enemyLifeTime;

        currentDamage = damage;
        currentExplosionRadius = team == Team.Player ? playerExplosionRadius : enemyExplosionRadius;

        // 物理挙動の初期設定
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = bulletMass;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // 方向ベクトルと速度から速度ベクトルを設定
        rb.velocity = direction.normalized * speed;

        // 進行方向に弾丸の向きを合わせる
        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        // 寿命が尽きたら自動的に破棄されるように設定
        Destroy(gameObject, life);
    }

    /// <summary>
    /// アイテムなどで強化された追加ダメージや追加爆風範囲を適用します。
    /// </summary>
    public void ApplyModifiers(int extraDamage, float extraRadius)
    {
        currentDamage += extraDamage;
        currentExplosionRadius += extraRadius;
    }

    /// <summary>
    /// 他のオブジェクトと衝突した際に呼び出されます。
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        // 未初期化、または既に爆発済みの場合は処理しない
        if (!initialized || hasExploded)
        {
            return;
        }

        // 味方の戦車に当たった場合はすり抜け（無視）する
        TankHealth directHit = collision.collider.GetComponent<TankHealth>();
        if (directHit != null && directHit.Team == ownerTeam)
        {
            return;
        }

        // 戦車への物理的な押し出しを弱める（弾丸の衝撃で戦車が吹き飛ばないようにする）
        if (collision.rigidbody != null && directHit != null)
        {
            DampenTankPush(collision.rigidbody);
        }

        // 衝突位置を取得し、その場で爆発処理を行う
        ContactPoint contact = collision.GetContact(0);
        ExplodeAt(contact.point);
    }

    /// <summary>
    /// 戦車が弾丸の物理的な衝撃で過剰に押し出されないよう、速度を減衰させます。
    /// </summary>
    void DampenTankPush(Rigidbody tankRigidbody)
    {
        Vector3 velocity = tankRigidbody.velocity;
        // X軸とZ軸の速度に減衰率を掛けて抑える
        tankRigidbody.velocity = new Vector3(
            velocity.x * tankVelocityKeepRatio,
            velocity.y,
            velocity.z * tankVelocityKeepRatio);
    }

    /// <summary>
    /// 指定された座標で爆発を発生させます。
    /// </summary>
    void ExplodeAt(Vector3 position)
    {
        if (hasExploded)
        {
            return;
        }

        hasExploded = true;
        SpawnExplosion(position);       // 爆発エフェクトの生成
        ApplyExplosionDamage(position); // 爆発による範囲ダメージの適用
        Destroy(gameObject);            // 弾丸自体の破棄
    }

    /// <summary>
    /// 爆発の範囲内にいる対象にダメージを与えます。
    /// </summary>
    void ApplyExplosionDamage(Vector3 center)
    {
        bool hitEnemy = false;
        
        // 爆発範囲内のすべてのコライダーを取得
        Collider[] hits = Physics.OverlapSphere(center, currentExplosionRadius);
        foreach (Collider hit in hits)
        {
            TankHealth health = hit.GetComponent<TankHealth>();
            
            // 無効な対象、味方、または既に死亡している対象は除外
            if (health == null || health.Team == ownerTeam || health.IsDead)
            {
                continue;
            }

            // ダメージを適用
            health.TakeDamage(currentDamage);

            // プレイヤーの攻撃が敵に当たったかどうかを記録
            if (ownerTeam == Team.Player && health.Team == Team.Enemy)
            {
                hitEnemy = true;
            }
        }

        // 敵にヒットした場合はヒットストップ演出を発生させる
        if (hitEnemy)
        {
            HitStopEffect.Trigger(0.12f);
        }
    }

    /// <summary>
    /// 爆発エフェクトを生成します。
    /// </summary>
    void SpawnExplosion(Vector3 position)
    {
        // インスペクターで専用のプレハブが設定されている場合はそれを優先して生成
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, position, Quaternion.identity);
            return;
        }

        // それ以外の場合は汎用の ExplosionVFX を使用して生成
        ExplosionVFX.SpawnAt(position);
    }
}
