using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 目標地点（キャプチャーゾーン）。
/// 一定時間ごとにランダムに移動し、中に入っているチームの制圧ゲージを進行させます。
/// </summary>
public class CaptureZone : MonoBehaviour
{
    [Header("制圧スピード（1秒間に増減するポイント）")]
    [SerializeField]
    float captureSpeed = 10f;

    [Header("ゾーンが移動する間隔（秒）")]
    [SerializeField]
    float moveInterval = 14f;

    [Header("移動範囲")]
    [SerializeField]
    Vector2 moveAreaSize = new Vector2(24f, 24f);

    float moveTimer;
    List<TankHealth> tanksInside = new List<TankHealth>();
    Vector3 centerPosition = Vector3.zero;
    float soundTickTimer = 0f;

    public static float DebugTotalTimeInZone { get; private set; } = 0f;

    /// <summary>
    /// 初期化処理。中心位置を記憶し、最初の位置への移動を行います。
    /// </summary>
    void Start()
    {
        centerPosition = transform.position;
        moveTimer = moveInterval;
        Relocate();
    }

    /// <summary>
    /// 毎フレーム呼ばれる更新処理。
    /// タイマーを減らし、0になったらゾーンを再配置します。
    /// その後、制圧の進捗を更新します。
    /// </summary>
    void Update()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            Relocate();
            moveTimer = moveInterval;
        }
        UpdateCaptureProgress();
    }

    /// <summary>
    /// ゾーン内にいる戦車を確認し、制圧ゲージ（UI）を進捗させます。
    /// </summary>
    void UpdateCaptureProgress()
    {
        if (CaptureProgressBarUI.Instance == null) return;

        // すでに勝敗がついている場合は処理しない
        if (CaptureProgressBarUI.Instance.captureProgress >= 100f || CaptureProgressBarUI.Instance.captureProgress <= -100f)
        {
            return;
        }

        // OverlapSphereを使って正確に範囲内の戦車を取得する
        tanksInside.Clear();
        float currentRadius = 4f;
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc != null) currentRadius = sc.radius;

        // ゾーンの半径内にあるコライダーをすべて取得
        Collider[] cols = Physics.OverlapSphere(transform.position, currentRadius);
        foreach (Collider col in cols)
        {
            TankHealth t = col.GetComponentInParent<TankHealth>();
            // 生きている戦車であればリストに追加（重複防止）
            if (t != null && !t.IsDead && !tanksInside.Contains(t))
            {
                tanksInside.Add(t);
            }
        }

        int playerTanks = 0;
        int enemyTanks = 0;

        // ゾーン内にいる戦車の数をチームごとにカウント
        foreach (var tank in tanksInside)
        {
            if (tank.Team == Team.Player) playerTanks++;
            else if (tank.Team == Team.Enemy) enemyTanks++;
        }

        // プレイヤー側はプラス、敵側はマイナス方向にゲージを動かす
        float delta = 0f;
        delta += playerTanks * captureSpeed * Time.deltaTime;
        delta -= enemyTanks * captureSpeed * Time.deltaTime;

        // 誰かがゾーンにいればデバッグ用の合計滞在時間をカウント
        if (playerTanks > 0 || enemyTanks > 0)
        {
            DebugTotalTimeInZone += Time.deltaTime;
        }

        // プレイヤーがゾーン内にいるかどうかのフラグをUIに伝達
        CaptureProgressBarUI.Instance.isPlayerInZone = (playerTanks > 0);

        // ゲージの増減がある場合のみ処理
        if (delta != 0f)
        {
            CaptureProgressBarUI.Instance.captureProgress += delta;
            
            // 進行度を -100 (敵の完全制圧) ～ 100 (プレイヤーの完全制圧) の範囲に制限
            CaptureProgressBarUI.Instance.captureProgress = Mathf.Clamp(CaptureProgressBarUI.Instance.captureProgress, -100f, 100f);

            // 制圧中の効果音（カチカチ音）を一定間隔で鳴らす
            soundTickTimer -= Time.deltaTime;
            if (soundTickTimer <= 0f)
            {
                SimpleAudioManager.Instance?.PlayCaptureTick(transform.position);
                soundTickTimer = 0.2f; // Play tick every 0.2 seconds
            }

            CheckWinCondition();
        }
    }

    /// <summary>
    /// 制圧ゲージが100または-100に達したか確認し、到達していれば勝利/敗北処理を呼び出します。
    /// </summary>
    void CheckWinCondition()
    {
        float progress = CaptureProgressBarUI.Instance.captureProgress;
        
        // プレイヤー側が100%制圧した場合
        if (progress >= 100f)
        {
            GameSceneController scene = FindFirstObjectByType<GameSceneController>();
            if (scene != null) scene.OnZoneCaptured(Team.Player);
            ResetZone();
        }
        // 敵側が100%制圧した場合 (-100%)
        else if (progress <= -100f)
        {
            GameSceneController scene = FindFirstObjectByType<GameSceneController>();
            if (scene != null) scene.OnZoneCaptured(Team.Enemy);
            ResetZone();
        }
    }

    /// <summary>
    /// ゾーンの進行度と位置をリセットします。
    /// </summary>
    void ResetZone()
    {
        CaptureProgressBarUI.Instance.captureProgress = 0f;
        Relocate();
        moveTimer = moveInterval;
    }

    /// <summary>
    /// 指定された移動範囲内のランダムな位置にゾーンを移動させます。
    /// </summary>
    void Relocate()
    {
        float x = Random.Range(-moveAreaSize.x / 2f, moveAreaSize.x / 2f);
        float z = Random.Range(-moveAreaSize.y / 2f, moveAreaSize.y / 2f);
        transform.position = centerPosition + new Vector3(x, 0f, z);
    }
}
