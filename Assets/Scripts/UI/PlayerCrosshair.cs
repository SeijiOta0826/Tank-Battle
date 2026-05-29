using UnityEngine;

/// <summary>
/// 画面中央にプレイヤー用の照準（クロスヘア）を描画するクラスです。
/// </summary>
public class PlayerCrosshair : MonoBehaviour
{
    GUIStyle style;

    void Start()
    {
        // 照準用のGUIスタイルを初期化
        style = new GUIStyle();
        style.fontSize = 40;
        style.normal.textColor = new Color(1f, 0.2f, 0.2f, 0.8f); // Reddish semi-transparent (半透明の赤)
        style.alignment = TextAnchor.MiddleCenter;
    }

    void OnGUI()
    {
        // Draw + in the exact center of the screen
        // 画面の正確な中心に「+」マークを描画
        GUI.Label(new Rect(Screen.width / 2f - 25f, Screen.height / 2f - 25f, 50f, 50f), "+", style);
    }
}
