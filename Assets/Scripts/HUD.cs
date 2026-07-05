using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 조준선 + [E] 프롬프트 + 획득 토스트 + 필수템 카운터.
/// 런타임에 Canvas/Text 를 만들어 쓴다(그레이박스 단계용, 나중에 TMP로 교체).
/// </summary>
public class HUD : MonoBehaviour
{
    Text prompt, toast, counter, crosshair;
    Coroutine toastCo;

    /// <summary>2D 탑다운 등 조준선이 필요 없을 때 끈다.</summary>
    public void SetCrosshair(bool on)
    {
        if (crosshair != null) crosshair.gameObject.SetActive(on);
    }

    void Awake()
    {
        var canvasGo = new GameObject("HUDCanvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGo.AddComponent<GraphicRaycaster>();

        var t = canvasGo.transform;

        crosshair = NewText(t, "Crosshair", 40, TextAnchor.MiddleCenter);
        crosshair.text = "+";
        Anchor(crosshair, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-25f, -25f), new Vector2(25f, 25f));

        prompt = NewText(t, "Prompt", 28, TextAnchor.MiddleCenter);
        Anchor(prompt, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(-350f, -30f), new Vector2(350f, 30f));

        counter = NewText(t, "Counter", 26, TextAnchor.UpperRight);
        Anchor(counter, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-360f, -80f), new Vector2(-30f, -30f));

        toast = NewText(t, "Toast", 34, TextAnchor.MiddleCenter);
        Anchor(toast, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(-450f, -40f), new Vector2(450f, 40f));
    }

    Text NewText(Transform parent, string goName, int size, TextAnchor anchor)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = LoadFont();
        t.fontSize = size;
        t.alignment = anchor;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        var sh = go.AddComponent<Shadow>();
        sh.effectDistance = new Vector2(1.5f, -1.5f);
        return t;
    }

    static void Anchor(Graphic g, Vector2 min, Vector2 max, Vector2 offMin, Vector2 offMax)
    {
        var rt = g.rectTransform;
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
    }

    static Font LoadFont()
    {
        var f = Font.CreateDynamicFontFromOSFont(
            new[] { "Apple SD Gothic Neo", "AppleGothic", "Malgun Gothic", "Arial Unicode MS", "Arial" }, 30);
        return f != null ? f : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    public void SetPrompt(string s)
    {
        if (prompt != null) prompt.text = s;
    }

    public void SetCounter(int got, int total)
    {
        if (counter == null) return;
        counter.text = total > 0 ? $"필수 아이템 {got}/{total}{(got >= total ? "  ✓" : "")}" : "";
    }

    public void ShowToast(string s, float dur = 2f)
    {
        if (toast == null) return;
        if (toastCo != null) StopCoroutine(toastCo);
        toastCo = StartCoroutine(ToastCo(s, dur));
    }

    IEnumerator ToastCo(string s, float dur)
    {
        toast.text = s;
        yield return new WaitForSeconds(dur);
        toast.text = "";
    }
}
