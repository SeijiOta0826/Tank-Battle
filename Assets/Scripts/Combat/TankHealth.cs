using UnityEngine;

/// <summary>
/// 戦車の HP と死亡処理。
/// </summary>
public class TankHealth : MonoBehaviour
{
    [SerializeField]
    Team team = Team.Player;

    [SerializeField]
    int maxHp = 3;

    [SerializeField]
    bool hideOnDeath = true;

    [SerializeField]
    bool freezeBodyOnDeath = true;

    public Team Team => team;
    public bool IsDead { get; private set; }
    public int CurrentHp { get; private set; }
    public int MaxHp => maxHp;

    TankAutoShooter shooter;
    PlayerTankMovement playerMove;
    EnemyTankAI enemyMove;
    TankDamageFlash damageFlash;
    Rigidbody rb;

    Vector3 initialPosition;
    Quaternion initialRotation;

    public void Configure(Team ownerTeam, int hp)
    {
        team = ownerTeam;
        maxHp = hp;
        CurrentHp = maxHp;
    }

    // コンポーネントや初期値の取得
    void Awake()
    {
        CurrentHp = maxHp;
        shooter = GetComponent<TankAutoShooter>();
        playerMove = GetComponent<PlayerTankMovement>();
        enemyMove = GetComponent<EnemyTankAI>();
        damageFlash = GetComponent<TankDamageFlash>();
        rb = GetComponent<Rigidbody>();

        // リスポーン位置と回転を記憶
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void SetRespawnPoint(Vector3 pos, Quaternion rot)
    {
        initialPosition = pos;
        initialRotation = rot;
    }

    /// <summary>
    /// HPを回復します。
    /// </summary>
    /// <returns>回復に成功した場合は true、すでに死亡しているか最大HPなら false を返します。</returns>
    public bool Heal(int amount)
    {
        if (IsDead || amount <= 0 || CurrentHp >= maxHp)
        {
            return false;
        }

        CurrentHp = Mathf.Min(CurrentHp + amount, maxHp);

        // UI演出とフラッシュを呼び出す
        DamageTextUI.Spawn(transform.position, amount, true);
        damageFlash?.PlayHealFlash();

        return true;
    }

    /// <summary>
    /// 最大HPを増やし、現在HPも同量回復します。
    /// </summary>
    public void IncreaseMaxHp(int amount)
    {
        maxHp += amount;
        Heal(amount);
    }

    /// <summary>
    /// ダメージを受けます。HPが0以下になったら死亡処理を呼び出します。
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (IsDead)
        {
            return;
        }

        CurrentHp -= amount;

        // SE再生とダメージテキスト表示
        SimpleAudioManager.Instance?.PlayDamage(transform.position);
        DamageTextUI.Spawn(transform.position, amount);

        // ダメージ時のフラッシュ演出
        damageFlash?.PlayFlash();

        // プレイヤーの場合はカメラを揺らす
        if (team == Team.Player)
        {
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake();
            }
        }

        // HPが尽きたら死亡
        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    public void KillInstant()
    {
        if (IsDead)
        {
            return;
        }

        CurrentHp = 0;
        Die();
    }

    /// <summary>
    /// 死亡時の処理を実行します。移動や攻撃を停止し、リスポーンを開始します。
    /// </summary>
    void Die()
    {
        IsDead = true;
        SimpleAudioManager.Instance?.PlayExplosion(transform.position);

        // 死体をその場に固定する
        if (freezeBodyOnDeath && rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // 物理演算を無効化
        }

        // 各種コンポーネントを停止
        if (shooter != null)
        {
            shooter.enabled = false;
        }

        if (playerMove != null)
        {
            playerMove.enabled = false;
        }

        if (enemyMove != null)
        {
            enemyMove.enabled = false;
        }

        // 見た目や当たり判定を消す
        if (hideOnDeath)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }

            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }
        }

        // プレイヤーが死んだ場合は、カメラが死んだ場所を映し続けるようにする
        if (team == Team.Player)
        {
            ThirdPersonCamera camera = Camera.main != null
                ? Camera.main.GetComponent<ThirdPersonCamera>()
                : null;
            if (camera != null)
            {
                camera.FreezeAtCurrentView();
            }
        }

        StartCoroutine(RespawnRoutine());
    }

    /// <summary>
    /// 一定時間経過後に戦車を指定の初期位置へ安全に復活させる。
    /// 入力: なし, 出力: IEnumerator(コルーチン), 副作用: HP最大回復、物理演算の再有効化、テレポート、操作無効解除
    /// </summary>
    System.Collections.IEnumerator RespawnRoutine()
    {
        // ゲームテンポとプレイヤーへのデス猶予を与えるため3秒待機する
        yield return new WaitForSeconds(3f);

        // 地面すり抜けおよび落下衝突を防ぐため、初期位置より少し上空を復活座標とする
        Vector3 spawnPos = initialPosition + Vector3.up * 2f;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // 物理エンジンの補間移動による吹っ飛びや描画バグを防ぐため、テレポート時は物理挙動を一時停止する
            rb.isKinematic = true;
            rb.position = spawnPos;
            rb.rotation = initialRotation;
        }

        transform.position = spawnPos;
        transform.rotation = initialRotation;

        // 物理演算が次のFixedUpdate前に古い座標で衝突判定を行ってしまうのを防ぐため、位置変更を即座に通知する
        Physics.SyncTransforms();

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        CurrentHp = maxHp;
        IsDead = false;

        // コンポーネントの有効化
        if (shooter != null) shooter.enabled = true;
        if (playerMove != null) playerMove.enabled = true;
        if (enemyMove != null) enemyMove.enabled = true;

        // 見た目や当たり判定を戻す
        if (hideOnDeath)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = true;
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = true;
        }

        // プレイヤーの場合はカメラのターゲットを元に戻す
        if (team == Team.Player && Camera.main != null)
        {
            ThirdPersonCamera camera = Camera.main.GetComponent<ThirdPersonCamera>();
            if (camera != null)
            {
                camera.SetTarget(transform);
            }
        }
    }
}
