using UnityEngine;

/// <summary>그레이박스 단계용 한글 폰트 로더. 나중에 TMP + 폰트 에셋으로 교체.</summary>
public static class FontUtil
{
    public static Font Load(int size = 30)
    {
        var f = Font.CreateDynamicFontFromOSFont(
            new[] { "Apple SD Gothic Neo", "AppleGothic", "Malgun Gothic", "Arial Unicode MS", "Arial" }, size);
        return f != null ? f : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}
