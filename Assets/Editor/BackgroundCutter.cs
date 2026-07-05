using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 흰 배경 캐릭터 이미지에서 "바깥쪽 흰색"만 지워 투명하게 만든다(모서리부터 flood-fill).
/// 캐릭터 내부의 흰색(셔츠·양말)은 외곽선에 막혀 남는다. 그 뒤 Sprite로 임포트한다.
/// </summary>
public static class BackgroundCutter
{
    static bool IsWhite(Color32 c) => c.r > 240 && c.g > 240 && c.b > 240;

    public static Sprite CutAndImport(string path, int ppu, bool bottomPivot)
    {
        // 1) 읽기 가능하게
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) { Debug.LogWarning($"[SchoolDay] 이미지 없음: {path}"); return null; }
        ti.textureType = TextureImporterType.Default;
        ti.isReadable = true;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.maxTextureSize = 4096;
        ti.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        int w = tex.width, h = tex.height;
        var px = tex.GetPixels32();
        var visited = new bool[w * h];
        var st = new Stack<int>();

        void Push(int j) { if (!visited[j] && IsWhite(px[j])) { visited[j] = true; st.Push(j); } }

        for (int x = 0; x < w; x++) { Push(x); Push((h - 1) * w + x); }        // 위/아래 모서리
        for (int y = 0; y < h; y++) { Push(y * w); Push(y * w + w - 1); }      // 좌/우 모서리

        while (st.Count > 0)
        {
            int i = st.Pop();
            px[i].a = 0;                                    // 배경 → 투명
            int x = i % w, y = i / w;
            if (x > 0) Push(i - 1);
            if (x < w - 1) Push(i + 1);
            if (y > 0) Push(i - w);
            if (y < h - 1) Push(i + w);
        }

        // 2) 저장
        var outTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        outTex.SetPixels32(px);
        outTex.Apply();
        File.WriteAllBytes(path, outTex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        // 3) Sprite 로 임포트
        var ti2 = (TextureImporter)AssetImporter.GetAtPath(path);
        ti2.textureType = TextureImporterType.Sprite;
        ti2.spriteImportMode = SpriteImportMode.Single;
        ti2.spritePixelsPerUnit = ppu;
        ti2.filterMode = FilterMode.Bilinear;
        ti2.alphaIsTransparency = true;
        ti2.isReadable = false;
        if (bottomPivot)
        {
            var s = new TextureImporterSettings();
            ti2.ReadTextureSettings(s);
            s.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            ti2.SetTextureSettings(s);
        }
        ti2.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
