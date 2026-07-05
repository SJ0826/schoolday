using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 칠판에 붙는 월드 공간 텍스트(시간표 등). 교시가 바뀌면 content 를 갈아끼워
/// 기묘하게 변하는 연출에 쓴다. 그레이박스용 — 나중에 TMP로 교체.
/// </summary>
public class BlackboardText : MonoBehaviour
{
    [TextArea] public string content = "";
    public float width = 3.6f;    // 미터
    public float height = 1.1f;
    public int fontSize = 40;

    Text text;

    void Start()
    {
        // 방향/위치는 부모(스케일 1인 BoardText 오브젝트)가 담당한다.
        // 칠판 큐브에 직접 붙이면 비균등 스케일이 전파돼 글자가 왜곡·반전된다.
        var canvasGo = new GameObject("BoardCanvas");
        canvasGo.transform.SetParent(transform, false);
        canvasGo.transform.localPosition = Vector3.zero;
        canvasGo.transform.localRotation = Quaternion.identity;
        canvasGo.transform.localScale = Vector3.one * 0.01f;

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var crt = canvas.GetComponent<RectTransform>();
        crt.sizeDelta = new Vector2(width * 100f, height * 100f);

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(canvasGo.transform, false);
        text = textGo.AddComponent<Text>();
        text.font = FontUtil.Load(fontSize);
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.92f, 0.96f, 0.92f);
        text.resizeTextForBestFit = true;      // 칠판 안에 자동으로 맞춤
        text.resizeTextMaxSize = fontSize;
        text.resizeTextMinSize = 8;
        text.text = content;
        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(20f, 20f);
        trt.offsetMax = new Vector2(-20f, -20f);
    }

    /// <summary>교시 전환 등에서 칠판 내용을 바꾼다.</summary>
    public void SetContent(string s)
    {
        content = s;
        if (text != null) text.text = s;
    }
}
