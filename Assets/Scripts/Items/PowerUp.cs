using UnityEngine;

/// <summary>
/// ステージに置くパワーアップアイテム。
/// プレイヤー戦車が触れると効果を適用し、自身は消えます。
///
/// 【Prefab 構成の目安】
/// - Sphere や Cube（見た目用）
/// - Sphere Collider: Is Trigger オン
/// - この PowerUp スクリプト
/// - （任意）Rigidbody: Is Kinematic オン（Trigger 検知を安定させたい場合）
/// </summary>
public class PowerUp : MonoBehaviour
{
    [Header("パワーアップの種類")]
    [SerializeField]
    PowerUpType powerUpType = PowerUpType.Heal;

    [Header("Heal 用：回復する HP 量")]
    [SerializeField]
    int healAmount = 1;

    [Header("MoveSpeedBoost / FireRateBoost 用：倍率（1.5 = 50% アップ）")]
    [SerializeField]
    float boostMultiplier = 1.5f;

    [Header("MoveSpeedBoost / FireRateBoost 用：効果時間（秒）")]
    [SerializeField]
    float boostDuration = 10f;

    [Header("取得後にアイテムを消す")]
    [SerializeField]
    bool destroyOnPickup = true;

    [Header("くるくる回転させる（見た目）")]
    [SerializeField]
    float rotateSpeed = 90f;

    /// <summary>
    /// GameSceneBootstrap などからアイテムの種類を指定して生成・初期化します。
    /// </summary>
    /// <param name="type">パワーアップの種類</param>
    public void Configure(PowerUpType type)
    {
        powerUpType = type;
        ApplyVisualByType();
    }

    /// <summary>
    /// ゲーム開始時に見た目（色など）を種類に合わせて適用します。
    /// </summary>
    void Start()
    {
        ApplyVisualByType();
    }

    /// <summary>
    /// 毎フレーム呼ばれ、アイテムをY軸（上方向）を中心に回転させて目立たせます。
    /// </summary>
    void Update()
    {
        // 空間のY軸を基準に一定速度で回転
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// 他のコライダー（プレイヤー等）が触れた瞬間に呼ばれます。
    /// </summary>
    /// <param name="other">触れたオブジェクトのコライダー</param>
    void OnTriggerEnter(Collider other)
    {
        // 戦車のHPコンポーネントを取得
        TankHealth health = other.GetComponent<TankHealth>();
        
        // 取得できない、または既に死亡している場合は無視（敵も取得可能）
        if (health == null || health.IsDead)
        {
            return;
        }

        // アイテムの効果を適用
        ApplyEffect(health, other.gameObject);

        // 取得後、自身を削除する設定なら削除
        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// パワーアップの種類に応じた効果をプレイヤーに適用します。
    /// </summary>
    /// <param name="health">対象となるプレイヤーのHPコンポーネント</param>
    /// <param name="playerObject">対象となるプレイヤーのゲームオブジェクト</param>
    void ApplyEffect(TankHealth health, GameObject playerObject)
    {
        switch (powerUpType)
        {
            // HP回復
            case PowerUpType.Heal:
                health.Heal(healAmount);
                Debug.Log($"[PowerUp] HP を {healAmount} 回復しました。");
                DamageTextUI.SpawnText(transform.position, "HEAL!", new Color(0.2f, 1f, 0.4f));
                break;

            // 移動速度アップ
            case PowerUpType.MoveSpeedBoost:
                PlayerTankMovement movement = playerObject.GetComponent<PlayerTankMovement>();
                if (movement != null)
                {
                    movement.ApplyMoveSpeedBoost(boostMultiplier, boostDuration);
                    Debug.Log($"[PowerUp] 移動速度 x{boostMultiplier} を {boostDuration} 秒間適用。");
                    DamageTextUI.SpawnText(transform.position, "SPEED UP!", new Color(0.3f, 0.7f, 1f));
                }
                break;

            // 攻撃速度（発射レート）アップ
            case PowerUpType.FireRateBoost:
                TankAutoShooter shooter = playerObject.GetComponent<TankAutoShooter>();
                if (shooter != null)
                {
                    shooter.ApplyFireRateBoost(boostMultiplier, boostDuration);
                    Debug.Log($"[PowerUp] 発射レート x{boostMultiplier} を {boostDuration} 秒間適用。");
                    DamageTextUI.SpawnText(transform.position, "FIRE RATE UP!", new Color(1f, 0.85f, 0.2f));
                }
                break;

            // 最大HP上昇
            case PowerUpType.MaxHpUp:
                health.IncreaseMaxHp(1);
                Debug.Log($"[PowerUp] {health.name} の最大HPが +1 されました。");
                DamageTextUI.SpawnText(transform.position, "MAX HP UP!", new Color(1f, 0.2f, 0.3f));
                break;

            // 攻撃力アップ
            case PowerUpType.AttackUp:
                TankAutoShooter shooterAtk = playerObject.GetComponent<TankAutoShooter>();
                if (shooterAtk != null)
                {
                    shooterAtk.extraDamage += 1;
                    Debug.Log($"[PowerUp] {health.name} の攻撃力が +1 されました。");
                    DamageTextUI.SpawnText(transform.position, "ATK UP!", new Color(1f, 0.4f, 0.1f));
                }
                break;

            // 爆風範囲アップ
            case PowerUpType.BlastRadiusUp:
                TankAutoShooter shooterRad = playerObject.GetComponent<TankAutoShooter>();
                if (shooterRad != null)
                {
                    shooterRad.extraBlastRadius += 1.5f;
                    Debug.Log($"[PowerUp] {health.name} の爆風範囲が +1.5 されました。");
                    DamageTextUI.SpawnText(transform.position, "BLAST UP!", new Color(1f, 0.9f, 0.2f));
                }
                break;
        }

        // 取得した際のエフェクト（光など）を発生させる
        FlashPickupEffect();
    }

    /// <summary>
    /// 取得した時に視覚的なエフェクト（爆発エフェクトの流用）を発生させます。
    /// </summary>
    void FlashPickupEffect()
    {
        // アイテムの位置にエフェクトを生成（サイズはやや小さめ）
        ExplosionVFX.SpawnAt(transform.position, 0.6f);
    }

    /// <summary>
    /// パワーアップの種類に応じて、アイテムの見た目（3Dモデルと色）を自動生成します。
    /// </summary>
    void ApplyVisualByType()
    {
        // 既存のレンダラーがあれば非表示にする（親のコライダーはそのまま使う）
        Renderer parentRenderer = GetComponent<Renderer>();
        if (parentRenderer != null)
        {
            parentRenderer.enabled = false;
        }

        // 古い見た目オブジェクトがあれば削除
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 新しい見た目を作成
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        
        switch (powerUpType)
        {
            case PowerUpType.Heal: // 緑色の十字架風
                CreateCross(visual.transform, new Color(0.2f, 1f, 0.4f));
                break;

            case PowerUpType.MoveSpeedBoost: // 水色の球体
                CreatePrimitive(visual.transform, PrimitiveType.Sphere, new Color(0.3f, 0.7f, 1f), Vector3.one * 0.8f);
                break;

            case PowerUpType.FireRateBoost: // 黄色の円柱
                CreatePrimitive(visual.transform, PrimitiveType.Cylinder, new Color(1f, 0.85f, 0.2f), new Vector3(0.6f, 0.4f, 0.6f));
                break;

            case PowerUpType.MaxHpUp: // 赤い大きめのカプセル（ハートの代わり）
                CreatePrimitive(visual.transform, PrimitiveType.Capsule, new Color(1f, 0.2f, 0.3f), new Vector3(0.8f, 0.6f, 0.8f));
                break;

            case PowerUpType.AttackUp: // オレンジと赤の剣（ピラミッド風）
                CreateSword(visual.transform, new Color(1f, 0.4f, 0.1f));
                break;

            case PowerUpType.BlastRadiusUp: // 黄色いトゲトゲ（球体と複数のカプセル）
                CreateSpikeBall(visual.transform, new Color(1f, 0.9f, 0.2f));
                break;
        }
    }

    // --- 以下、見た目生成用のヘルパーメソッド群 ---

    void CreatePrimitive(Transform parent, PrimitiveType type, Color color, Vector3 scale)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        Destroy(obj.GetComponent<Collider>());
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().material.color = color;
    }

    void CreateCross(Transform parent, Color color)
    {
        // 縦
        GameObject v = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(v.GetComponent<Collider>());
        v.transform.SetParent(parent);
        v.transform.localPosition = Vector3.zero;
        v.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
        v.GetComponent<Renderer>().material.color = color;

        // 横
        GameObject h = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(h.GetComponent<Collider>());
        h.transform.SetParent(parent);
        h.transform.localPosition = Vector3.zero;
        h.transform.localScale = new Vector3(1f, 0.3f, 0.3f);
        h.GetComponent<Renderer>().material.color = color;
    }

    void CreateSword(Transform parent, Color color)
    {
        // 刃
        GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(blade.GetComponent<Collider>());
        blade.transform.SetParent(parent);
        blade.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        blade.transform.localScale = new Vector3(0.2f, 0.8f, 0.2f);
        blade.GetComponent<Renderer>().material.color = color;

        // 柄
        GameObject hilt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(hilt.GetComponent<Collider>());
        hilt.transform.SetParent(parent);
        hilt.transform.localPosition = new Vector3(0f, -0.2f, 0f);
        hilt.transform.localScale = new Vector3(0.6f, 0.2f, 0.2f);
        hilt.GetComponent<Renderer>().material.color = Color.gray;
    }

    void CreateSpikeBall(Transform parent, Color color)
    {
        CreatePrimitive(parent, PrimitiveType.Sphere, color, Vector3.one * 0.6f);

        for (int i = 0; i < 3; i++)
        {
            GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(spike.GetComponent<Collider>());
            spike.transform.SetParent(parent);
            spike.transform.localPosition = Vector3.zero;
            spike.transform.localScale = new Vector3(0.2f, 0.6f, 0.2f);
            
            // XYZ軸それぞれに回転
            Vector3 euler = Vector3.zero;
            euler[i] = 90f;
            if (i == 0) euler = new Vector3(45f, 45f, 0f);
            if (i == 1) euler = new Vector3(-45f, 45f, 0f);
            if (i == 2) euler = new Vector3(0f, 45f, 45f);

            spike.transform.localRotation = Quaternion.Euler(euler);
            spike.GetComponent<Renderer>().material.color = color;
        }
    }
}
