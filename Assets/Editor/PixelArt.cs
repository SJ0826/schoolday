using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 문자열 격자로 도트 스프라이트를 만들어 Assets/Sprites 에 저장하는 에디터 유틸.
/// 각 문자 = 팔레트 색 한 칸. 공백/'.'은 투명. 위쪽 문자열이 위쪽 행.
/// </summary>
public static class PixelArt
{
    public static Sprite Make(string name, string[] rows, Dictionary<char, Color32> palette, int ppu = 16)
    {
        int h = rows.Length;
        int w = rows[0].Length;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };

        var clear = new Color32(0, 0, 0, 0);
        for (int y = 0; y < h; y++)
        {
            string row = rows[h - 1 - y]; // 위 문자열이 위쪽 행이 되도록 뒤집기
            for (int x = 0; x < w; x++)
            {
                char ch = x < row.Length ? row[x] : ' ';
                Color32 c = palette.TryGetValue(ch, out var col) ? col : clear;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();

        if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
            AssetDatabase.CreateFolder("Assets", "Sprites");
        string path = $"Assets/Sprites/{name}.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = ppu;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.wrapMode = TextureWrapMode.Clamp;
        // Tiled drawMode가 되도록 Full Rect 메시로
        var st = new TextureImporterSettings();
        ti.ReadTextureSettings(st);
        st.spriteMeshType = SpriteMeshType.FullRect;
        ti.SetTextureSettings(st);
        ti.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
