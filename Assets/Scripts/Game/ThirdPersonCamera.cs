using UnityEngine;

/// <summary>
/// プレイヤーを中心に、円状の軌道を回るオービットカメラ。
/// 十字キーで左右（ヨー）・上下（ピッチ）を回せます。
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Tooltip("カメラが追いかける対象（プレイヤーなど）")]
    [SerializeField]
    Transform target;

    [Header("プレイヤーからの距離")]
    [Tooltip("カメラからターゲットまでの距離")]
    [SerializeField]
    float orbitDistance = 12f;

    [Header("現在の回転角（度）")]
    [Tooltip("水平方向の回転角度（ヨー角）")]
    [SerializeField]
    float orbitYaw;

    [Tooltip("垂直方向の回転角度（ピッチ角）")]
    [SerializeField]
    float orbitPitch = 32f;

    [Header("十字キーで回す速さ")]
    [Tooltip("左右キー入力時の回転スピード")]
    [SerializeField]
    float yawSpeed = 140f;

    [Tooltip("上下キー入力時の回転スピード")]
    [SerializeField]
    float pitchSpeed = 100f;

    [Tooltip("見上げられる最大角度（下向きへの回転限界）")]
    [SerializeField]
    float minPitch = -80f;

    [Tooltip("見下ろせる最大角度（上向きへの回転限界）")]
    [SerializeField]
    float maxPitch = 75f;

    [Tooltip("カメラの移動にどれくらい遅れて追従するか（滑らかさ）")]
    [SerializeField]
    float positionSmooth = 14f;

    [Tooltip("ターゲットのどの高さを中心に見つめるか（Y軸のオフセット）")]
    [SerializeField]
    float lookAtHeight = 1.2f;

    // 視点が固定されている時の座標
    Vector3 frozenPosition;
    // 視点が固定されている時の回転
    Quaternion frozenRotation;
    // 視点固定モードが有効かどうか
    bool useFrozenView;

    /// <summary>
    /// ゲーム開始時に呼ばれる初期化処理
    /// </summary>
    void Awake()
    {
        // Inspectorで古い値（-40など）が保存されていても上書きして、確実に上を向けるようにする
        minPitch = -85f;
    }

    /// <summary>
    /// カメラが追いかけるターゲットを設定します。
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        SetTarget(newTarget, false);
    }

    /// <summary>
    /// 追従対象のターゲットを設定する。
    /// 入力: newTarget(ターゲットのTransform), smoothTransition(急激な角度リセットを防ぐフラグ), 出力: なし, 副作用: カメラ追従対象の変更
    /// </summary>
    public void SetTarget(Transform newTarget, bool smoothTransition)
    {
        target = newTarget;
        useFrozenView = false;

        // 瞬時の画角回転による画面酔いを防止するため、スムーズ遷移時は現在のヨー角を維持する
        if (target != null && !smoothTransition)
        {
            InitializeYawFromCurrentPosition();
        }
    }

    /// <summary>
    /// カメラと追従ターゲットとの距離（ orbitDistance ）を動的に変更する。
    /// 入力: distance(設定する距離), 出力: なし, 副作用: orbitDistance変数の書き換え
    /// </summary>
    public void SetOrbitDistance(float distance)
    {
        orbitDistance = distance;
    }

    /// <summary>
    /// 現在のカメラ位置と回転を固定し、追従を停止します。（ゲームオーバー時などに使用）
    /// </summary>
    public void FreezeAtCurrentView()
    {
        frozenPosition = transform.position;
        frozenRotation = transform.rotation;
        useFrozenView = true;
    }

    /// <summary>
    /// 現在のカメラ位置とターゲット位置から、初期の水平角度（ヨー角）を計算して設定します。
    /// </summary>
    void InitializeYawFromCurrentPosition()
    {
        // Y軸を無視した水平方向のオフセット（距離）を計算
        Vector3 flatOffset = transform.position - target.position;
        flatOffset.y = 0f;

        // 十分な距離がある場合のみ角度を計算（ゼロ除算などを防ぐ）
        if (flatOffset.sqrMagnitude > 0.01f)
        {
            // Atan2を使用してXとZから角度（ラジアン）を求め、度数法に変換
            orbitYaw = Mathf.Atan2(flatOffset.x, flatOffset.z) * Mathf.Rad2Deg;
        }
    }

    /// <summary>
    /// 毎フレーム呼ばれる更新処理。主に入力によるカメラの回転を処理します。
    /// </summary>
    void Update()
    {
        // 視点固定モード中、またはターゲットが存在しない場合は何もしない
        if (useFrozenView || target == null)
        {
            return;
        }

        // 左キーでカメラを左回り（ヨー角を減少）
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            orbitYaw -= yawSpeed * Time.deltaTime;
        }

        // 右キーでカメラを右回り（ヨー角を増加）
        if (Input.GetKey(KeyCode.RightArrow))
        {
            orbitYaw += yawSpeed * Time.deltaTime;
        }

        // 上キーでカメラを見下ろす（ピッチ角を増加）
        if (Input.GetKey(KeyCode.UpArrow))
        {
            orbitPitch += pitchSpeed * Time.deltaTime;
        }

        // 下キーでカメラを見上げる（ピッチ角を減少）
        if (Input.GetKey(KeyCode.DownArrow))
        {
            orbitPitch -= pitchSpeed * Time.deltaTime;
        }

        // ピッチ角が最小・最大角度の範囲内に収まるように制限する
        orbitPitch = Mathf.Clamp(orbitPitch, minPitch, maxPitch);
    }

    /// <summary>
    /// Updateの後に呼ばれる更新処理。カメラの実際の位置と回転を更新します。
    /// </summary>
    void LateUpdate()
    {
        // 視点固定モード中は固定された座標と回転を維持する
        if (useFrozenView)
        {
            transform.position = frozenPosition;
            transform.rotation = frozenRotation;
            return;
        }

        // ターゲットが削除されている場合は何もしない
        if (target == null)
        {
            return;
        }

        // ピッチ角とヨー角から、カメラの回転を表すクォータニオンを作成
        Quaternion orbitRotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
        
        // Z軸のマイナス方向（後ろ）に距離分だけ離したオフセットベクトルを作成し、カメラの回転を適用
        Vector3 offset = orbitRotation * new Vector3(0f, 0f, -orbitDistance);
        
        // ターゲットの位置にオフセットを足して、カメラの目標座標を計算
        Vector3 desiredPosition = target.position + offset;

        // 目標座標に向かって滑らかにカメラを移動させる（指数減衰を使用）
        float t = 1f - Mathf.Exp(-positionSmooth * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, t);

        // ターゲットより少し高い位置（lookAtHeight）を見つめるようにカメラを回転させる
        Vector3 lookPoint = target.position + Vector3.up * lookAtHeight;
        transform.LookAt(lookPoint);

        // ダメージ時などのカメラシェイク（画面揺れ）を加算する
        if (CameraShake.Instance != null && CameraShake.Instance.CurrentShakeOffset != Vector3.zero)
        {
            transform.position += transform.rotation * CameraShake.Instance.CurrentShakeOffset;
        }
    }
}
