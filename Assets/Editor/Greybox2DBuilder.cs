using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 2D 탑다운 그레이박스 교실 생성기.
/// 메뉴 [SchoolDay ▸ 2D 교실 만들기] → Assets/Scenes/Classroom2D.unity 생성.
/// 단색 사각형(스프라이트)으로 바닥·벽·책상 + 플레이어 + 따라가는 카메라. 도트 그림은 나중에 교체.
/// </summary>
public static class Greybox2DBuilder
{
    const float W = 12f, H = 8f;   // 교실 크기(유닛)

    [MenuItem("SchoolDay/2D 교실 만들기")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var white = WhiteSprite();

        var root = new GameObject("Classroom2D");

        Color floorC  = new Color(0.82f, 0.74f, 0.60f);
        Color wallC   = new Color(0.60f, 0.62f, 0.68f);
        Color deskC   = new Color(0.50f, 0.38f, 0.28f);
        Color playerC = new Color(0.30f, 0.60f, 0.90f);

        // 바닥 (콜라이더 없음)
        Tile("Floor", root.transform, white, floorC, Vector2.zero, new Vector2(W, H), 0);

        // 벽 (테두리, 콜라이더 있음)
        const float t = 0.4f;
        Solid("Wall_Top",    root.transform, white, wallC, new Vector2(0f,  H / 2f), new Vector2(W, t));
        Solid("Wall_Bottom", root.transform, white, wallC, new Vector2(0f, -H / 2f), new Vector2(W, t));
        Solid("Wall_Left",   root.transform, white, wallC, new Vector2(-W / 2f, 0f), new Vector2(t, H));
        Solid("Wall_Right",  root.transform, white, wallC, new Vector2( W / 2f, 0f), new Vector2(t, H));

        // 책상 그리드 (콜라이더 있는 장애물)
        var desks = new GameObject("Desks");
        desks.transform.SetParent(root.transform);
        const int cols = 4, rows = 3;
        const float gx = 2f, gy = 1.8f;
        float x0 = -(cols - 1) * gx / 2f, y0 = (rows - 1) * gy / 2f;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                Solid($"Desk_{r}_{c}", desks.transform, white, deskC,
                    new Vector2(x0 + c * gx, y0 - r * gy), new Vector2(1.1f, 0.7f));

        // 플레이어
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0f, -H / 2f + 1.2f, 0f);
        player.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        var psr = player.AddComponent<SpriteRenderer>();
        psr.sprite = white; psr.color = playerC; psr.sortingOrder = 10;
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.freezeRotation = true;
        player.AddComponent<BoxCollider2D>();
        player.AddComponent<PlayerController2D>();

        // 카메라 (직교 + 따라가기)
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.tag = "MainCamera";
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<CameraFollow2D>().target = player.transform;

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Classroom2D.unity");
        Debug.Log("[SchoolDay] 2D 교실 생성 완료 → Assets/Scenes/Classroom2D.unity (▶ Play로 WASD 이동)");
    }

    static void Tile(string name, Transform parent, Sprite sp, Color c, Vector2 pos, Vector2 size, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp; sr.color = c; sr.sortingOrder = order;
    }

    static void Solid(string name, Transform parent, Sprite sp, Color c, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp; sr.color = c; sr.sortingOrder = 1;
        go.AddComponent<BoxCollider2D>();
    }

    // 1x1 흰 스프라이트(도트용, Point 필터)를 만들어 재사용. color/scale로 색·크기 조절.
    static Sprite WhiteSprite()
    {
        const string path = "Assets/Sprites/white.png";
        if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
            AssetDatabase.CreateFolder("Assets", "Sprites");
        if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null)
        {
            var tex = new Texture2D(16, 16);
            var px = new Color32[16 * 16];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px); tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spritePixelsPerUnit = 16;      // 스프라이트 1개 = 1 유닛
            ti.filterMode = FilterMode.Point;  // 도트 느낌(선명)
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
