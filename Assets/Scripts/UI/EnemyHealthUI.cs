using UnityEngine;

/// <summary>
/// 敵の頭上にHPバーを表示するUIスクリプトです。
/// </summary>
public class EnemyHealthUI : MonoBehaviour
{
    TankHealth health;
    GUIStyle bgStyle;
    GUIStyle fillStyle;
    Texture2D bgTex;
    Texture2D fillTex;

    /// <summary>
    /// 初期化処理。HPバーの背景色と塗りつぶし色のテクスチャを生成します。
    /// </summary>
    void Start()
    {
        health = GetComponent<TankHealth>();

        bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
        bgTex.Apply();

        fillTex = new Texture2D(1, 1);
        fillTex.SetPixel(0, 0, new Color(1f, 0.2f, 0.2f, 1f));
        fillTex.Apply();

        bgStyle = new GUIStyle();
        bgStyle.normal.background = bgTex;

        fillStyle = new GUIStyle();
        fillStyle.normal.background = fillTex;
    }

    /// <summary>
    /// HPバーを画面に描画します。敵がカメラの前方にいる場合のみ表示されます。
    /// </summary>
    void OnGUI()
    {
        if (health == null || health.IsDead || Camera.main == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);

        if (screenPos.z > 0)
        {
            float width = 60f;
            float height = 8f;
            float x = screenPos.x - width / 2f;
            float y = Screen.height - screenPos.y;

            // 背景を描画
            GUI.Box(new Rect(x, y, width, height), "", bgStyle);

            // 現在のHPに応じたバーの塗りつぶしを描画
            float hpRatio = (float)health.CurrentHp / Mathf.Max(1, health.MaxHp);
            hpRatio = Mathf.Clamp01(hpRatio);
            GUI.Box(new Rect(x, y, width * hpRatio, height), "", fillStyle);
        }
    }
}
