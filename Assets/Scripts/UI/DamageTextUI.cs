using UnityEngine;

/// <summary>
/// ダメージや回復量を示すポップアップテキストを画面に表示するためのUIクラス。
/// </summary>
public class DamageTextUI : MonoBehaviour
{
    /// <summary>
    /// ダメージまたは回復のテキストを画面上に生成します。
    /// </summary>
    public static void Spawn(Vector3 worldPosition, int amount, bool isHeal = false)
    {
        GameObject go = new GameObject(isHeal ? "HealText" : "DamageText");
        go.transform.position = worldPosition + Vector3.up * 2f;
        DamageTextUI dmg = go.AddComponent<DamageTextUI>();
        dmg.Init(amount, isHeal);
    }

    /// <summary>
    /// カスタムテキスト（アイテム取得など）を画面上に生成します。
    /// </summary>
    public static void SpawnText(Vector3 worldPosition, string text, Color color)
    {
        GameObject go = new GameObject("CustomText");
        go.transform.position = worldPosition + Vector3.up * 2.5f; // 少し高めに表示
        DamageTextUI dmg = go.AddComponent<DamageTextUI>();
        dmg.InitText(text, color);
    }

    int amount;
    bool isHealText;
    string customText;
    bool useCustomText = false;
    float lifeTimer = 1f;
    Vector3 worldPos;
    GUIStyle style;

    /// <summary>
    /// テキストの初期化を行います。色や表示時間を設定します。
    /// </summary>
    void Init(int dmgAmount, bool isHeal)
    {
        amount = dmgAmount;
        isHealText = isHeal;
        worldPos = transform.position;

        style = new GUIStyle();
        style.fontSize = 32;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = isHeal ? Color.green : Color.red;
        style.alignment = TextAnchor.MiddleCenter;

        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// カスタムテキストの初期化を行います。色や表示時間を設定します。
    /// </summary>
    void InitText(string text, Color color)
    {
        customText = text;
        useCustomText = true;
        worldPos = transform.position;

        style = new GUIStyle();
        style.fontSize = 60;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = color;
        style.alignment = TextAnchor.MiddleCenter;

        lifeTimer = 1.5f; // アイテム用は少し長めに表示
        Destroy(gameObject, 1.5f);
    }

    /// <summary>
    /// 毎フレーム上方向に移動させ、寿命タイマーを減らします。
    /// </summary>
    void Update()
    {
        worldPos += Vector3.up * Time.deltaTime * 2f; // Float upwards
        lifeTimer -= Time.deltaTime;
    }

    /// <summary>
    /// 画面上にテキストを描画します。カメラの前方にいる場合のみ表示されます。
    /// </summary>
    void OnGUI()
    {
        if (Camera.main == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // カメラの前方に位置する場合のみ描画する
        if (screenPos.z > 0)
        {
            Color c = style.normal.textColor;
            c.a = Mathf.Clamp01(lifeTimer);
            style.normal.textColor = c;

            // GUI座標系はY軸が下向きなので反転させる
            float y = Screen.height - screenPos.y;

            string displayString;
            if (useCustomText)
            {
                displayString = customText;
            }
            else
            {
                string prefix = isHealText ? "+" : "-";
                displayString = prefix + amount.ToString();
            }

            // Outline effect for better visibility
            Color outlineColor = Color.black;
            outlineColor.a = c.a;
            style.normal.textColor = outlineColor;
            GUI.Label(new Rect(screenPos.x - 51, y - 50, 100, 100), displayString, style);
            GUI.Label(new Rect(screenPos.x - 49, y - 50, 100, 100), displayString, style);
            GUI.Label(new Rect(screenPos.x - 50, y - 51, 100, 100), displayString, style);
            GUI.Label(new Rect(screenPos.x - 50, y - 49, 100, 100), displayString, style);
            
            style.normal.textColor = c;
            GUI.Label(new Rect(screenPos.x - 50, y - 50, 100, 100), displayString, style);
        }
    }
}
