using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 화면 하단 자막. 런타임에 Canvas/Text 를 만들어 쓴다(그레이박스 단계용).
/// 나중에 TextMeshPro + 한글 폰트 에셋으로 교체 예정.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    Text label;
    GameObject panel;

    void Awake()
    {
        var canvasGo = new GameObject("DialogueCanvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGo.AddComponent<GraphicRaycaster>();

        panel = new GameObject("Panel");
        panel.transform.SetParent(canvasGo.transform, false);
        var img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.72f);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.1f, 0.07f);
        prt.anchorMax = new Vector2(0.9f, 0.24f);
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(panel.transform, false);
        label = textGo.AddComponent<Text>();
        label.font = LoadFont();
        label.fontSize = 30;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(40f, 12f);
        trt.offsetMax = new Vector2(-40f, -12f);

        Hide();
    }

    // 한글이 보이도록 OS 폰트를 동적으로 사용(에디터/맥 기준). 실패 시 내장 폰트.
    static Font LoadFont()
    {
        var f = Font.CreateDynamicFontFromOSFont(
            new[] { "Apple SD Gothic Neo", "AppleGothic", "Malgun Gothic", "Arial Unicode MS", "Arial" }, 30);
        return f != null ? f : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    public void Show(string line)
    {
        panel.SetActive(true);
        label.text = line;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    /// <summary>대사를 한 줄씩 보여주고 Space/Enter/좌클릭으로 넘긴다. 끝나면 숨긴다.</summary>
    public IEnumerator PlayLines(string[] lines)
    {
        if (lines == null) yield break;
        foreach (var l in lines)
        {
            Show(l);
            yield return null;                 // 표시된 프레임의 입력은 무시
            while (!Advanced()) yield return null;
        }
        Hide();
    }

    static bool Advanced()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        return (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
            || (mouse != null && mouse.leftButton.wasPressedThisFrame);
    }
}
