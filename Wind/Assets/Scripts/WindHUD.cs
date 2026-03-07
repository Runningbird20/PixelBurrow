using UnityEngine;

public class WindHUD : MonoBehaviour
{
    [TextArea]
    public string title = "You Are the Wind";

    [TextArea]
    public string subtitle = "Drift, carry seeds, and bring life back to the land.";

    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;

    private void OnGUI()
    {
        if (titleStyle == null)
        {
            BuildStyles();
        }

        GUILayout.BeginArea(new Rect(18f, 18f, 500f, 180f));
        GUILayout.Label(title, titleStyle);
        GUILayout.Label(subtitle, bodyStyle);
        GUILayout.Label("Move: WASD / Arrow keys / Mouse", bodyStyle);
        GUILayout.Label("Pick up: fly through seeds/petals/clouds", bodyStyle);
        GUILayout.Label("Drop: Space", bodyStyle);
        GUILayout.EndArea();
    }

    private void BuildStyles()
    {
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 26,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.93f, 0.98f, 1f, 0.95f) }
        };

        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            normal = { textColor = new Color(0.85f, 0.94f, 0.92f, 0.92f) }
        };
    }
}
