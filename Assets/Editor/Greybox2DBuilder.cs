using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 2D 탑다운 교실 생성기 (도트 스프라이트 버전).
/// 메뉴 [SchoolDay ▸ 2D 교실 만들기] → Assets/Scenes/Classroom2D.unity 생성.
/// 마루 바닥·벽·책상·학생 캐릭터를 코드 도트로 그려 배치한다.
/// </summary>
public static class Greybox2DBuilder
{
    const float W = 12f, H = 8f;   // 교실 크기(유닛)

    [MenuItem("SchoolDay/2D 교실 만들기")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var floorSp = FloorSprite();
        var wallSp  = WallSprite();
        var deskSp  = DeskSprite();
        var boardSp = BoardSprite();

        var root = new GameObject("Classroom2D");

        // 바닥 (마루 타일 반복)
        var floor = MakeSR("Floor", root.transform, floorSp, Vector2.zero, 0);
        floor.drawMode = SpriteDrawMode.Tiled;
        floor.size = new Vector2(W, H);

        // 벽 (테두리, 타일 + 콜라이더)
        const float t = 0.5f;
        WallSeg("Wall_Top",    root.transform, wallSp, new Vector2(0f,  H / 2f), new Vector2(W, t));
        WallSeg("Wall_Bottom", root.transform, wallSp, new Vector2(0f, -H / 2f), new Vector2(W, t));
        WallSeg("Wall_Left",   root.transform, wallSp, new Vector2(-W / 2f, 0f), new Vector2(t, H));
        WallSeg("Wall_Right",  root.transform, wallSp, new Vector2( W / 2f, 0f), new Vector2(t, H));

        // 칠판 (위쪽 벽 앞) — 조사 대상
        var board = MakeSR("Chalkboard", root.transform, boardSp, new Vector2(0f, H / 2f - 0.7f), 2);
        board.transform.localScale = new Vector3(4f, 1f, 1f);
        var boardCol = board.gameObject.AddComponent<BoxCollider2D>();
        boardCol.size = new Vector2(boardSp.bounds.size.x, boardSp.bounds.size.y);
        var ex = board.gameObject.AddComponent<ExamineObject>();
        ex.label = "칠판"; ex.line = "[ 오늘의 시간표 ]  조례 · 1교시 국어…";

        // 책상 그리드
        var desks = new GameObject("Desks");
        desks.transform.SetParent(root.transform);
        const int cols = 4, rows = 3;
        const float gx = 2f, gy = 1.8f;
        float x0 = -(cols - 1) * gx / 2f, y0 = (rows - 1) * gy / 2f;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                var d = MakeSR($"Desk_{r}_{c}", desks.transform, deskSp, new Vector2(x0 + c * gx, y0 - r * gy), 1);
                var dc = d.gameObject.AddComponent<BoxCollider2D>();
                dc.size = deskSp.bounds.size;
            }

        // 플레이어 (학생 도트, 남색 교복)
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0f, -H / 2f + 1.4f, 0f);
        var psr = player.AddComponent<SpriteRenderer>();
        psr.sprite = StudentSprite("player", new Color32(60, 96, 150, 255));
        psr.sortingOrder = 10;
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.freezeRotation = true;
        var pcol = player.AddComponent<CapsuleCollider2D>();
        pcol.size = new Vector2(0.5f, 0.8f);
        player.AddComponent<PlayerController2D>();
        player.AddComponent<PlayerInteractor2D>();

        // 매니저 (대사·HUD·오프닝)
        var dlg = new GameObject("Dialogue"); dlg.transform.SetParent(root.transform); dlg.AddComponent<DialogueUI>();
        var hudGo = new GameObject("HUD"); hudGo.transform.SetParent(root.transform); hudGo.AddComponent<HUD>().SetCrosshair(false);
        var opening = new GameObject("OpeningSequence"); opening.transform.SetParent(root.transform); opening.AddComponent<OpeningSequence2D>();

        // 친구 NPC (학생 도트, 교복색 다르게)
        Npc(root.transform, "짝꿍", new Color32(170, 80, 80, 255), new Vector2(-2f, -1f),
            new[] { "왔냐? 침 흘리고 자더라.", "조례 곧 한대.", "오늘 급식 뭐냐?" });
        Npc(root.transform, "반장", new Color32(90, 140, 90, 255), new Vector2(3f, 1f),
            new[] { "자리 앉는 게 좋을걸.", "숙제 했어?" });

        // 카메라 (직교 + 따라가기, 배경 남색)
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.06f, 0.09f);
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.tag = "MainCamera";
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<CameraFollow2D>().target = player.transform;

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Classroom2D.unity");
        Debug.Log("[SchoolDay] 2D 도트 교실 생성 완료 → Assets/Scenes/Classroom2D.unity");
    }

    // ---- 배치 헬퍼 ----

    static SpriteRenderer MakeSR(string name, Transform parent, Sprite sp, Vector2 pos, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp; sr.sortingOrder = order;
        return sr;
    }

    static void WallSeg(string name, Transform parent, Sprite sp, Vector2 pos, Vector2 size)
    {
        var sr = MakeSR(name, parent, sp, pos, 1);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
        var col = sr.gameObject.AddComponent<BoxCollider2D>();
        col.size = size;
    }

    static void Npc(Transform parent, string npcName, Color32 uniform, Vector2 pos, string[] lines)
    {
        var go = new GameObject($"NPC_{npcName}");
        go.transform.SetParent(parent);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = StudentSprite($"student_{npcName}", uniform);
        sr.sortingOrder = 5;
        var col = go.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.5f, 0.8f);
        var npc = go.AddComponent<TalkableNPC>();
        npc.npcName = npcName; npc.lines = lines;
    }

    // ---- 도트 스프라이트 ----

    // 위에서 본 학생: 머리카락+얼굴+교복+다리. 교복색만 바꿔 재사용.
    static Sprite StudentSprite(string name, Color32 uniform)
    {
        var pal = new Dictionary<char, Color32>
        {
            { 'h', new Color32(58, 40, 28, 255) },   // 머리카락
            { 'f', new Color32(240, 200, 165, 255) },// 피부
            { 'b', uniform },                         // 교복
            { 's', new Color32(45, 40, 38, 255) },   // 신발
        };
        string[] rows =
        {
            "  hhhhhh  ",
            " hhhhhhhh ",
            " hhhhhhhh ",
            " hhffffhh ",
            " hffffffh ",
            "  ffffff  ",
            " bbbbbbbb ",
            "bbbbbbbbbb",
            "bbbbbbbbbb",
            "bbbbbbbbbb",
            " bbbbbbbb ",
            " bb    bb ",
            " ss    ss ",
        };
        return PixelArt.Make(name, rows, pal, 13);
    }

    static Sprite DeskSprite()
    {
        var pal = new Dictionary<char, Color32>
        {
            { 't', new Color32(178, 130, 88, 255) }, // 상판
            { 'e', new Color32(120, 84, 52, 255) },  // 테두리
            { 'l', new Color32(96, 66, 40, 255) },   // 다리
        };
        string[] rows =
        {
            "eeeeeeeeeeeeeeee",
            "etttttttttttttte",
            "etttttttttttttte",
            "etttttttttttttte",
            "etttttttttttttte",
            "etttttttttttttte",
            "etttttttttttttte",
            "etttttttttttttte",
            "eeeeeeeeeeeeeeee",
            "ll............ll",
            "ll............ll",
            "ll............ll",
        };
        return PixelArt.Make("desk", rows, pal, 16);
    }

    static Sprite FloorSprite()
    {
        var a = new Color32(214, 182, 138, 255); // 마루
        var b = new Color32(196, 162, 116, 255); // 판 이음선
        var pal = new Dictionary<char, Color32> { { 'a', a }, { 'b', b } };
        var rows = new string[16];
        for (int y = 0; y < 16; y++)
        {
            if (y == 0) { rows[y] = new string('b', 16); continue; }       // 가로 이음
            var arr = new char[16];
            for (int x = 0; x < 16; x++) arr[x] = (x == 0 || x == 8) ? 'b' : 'a'; // 세로 판선
            rows[y] = new string(arr);
        }
        return PixelArt.Make("floor_wood", rows, pal, 16);
    }

    static Sprite WallSprite()
    {
        var c = new Color32(232, 224, 208, 255); // 벽
        var d = new Color32(206, 198, 180, 255); // 하단 음영
        var pal = new Dictionary<char, Color32> { { 'c', c }, { 'd', d } };
        var rows = new string[16];
        for (int y = 0; y < 16; y++)
            rows[y] = new string(y < 3 ? 'd' : 'c', 16);
        return PixelArt.Make("wall_tile", rows, pal, 16);
    }

    static Sprite BoardSprite()
    {
        var g = new Color32(34, 64, 48, 255);   // 칠판
        var f = new Color32(150, 110, 70, 255); // 나무틀
        var w = new Color32(210, 214, 200, 255);// 분필 자국
        var pal = new Dictionary<char, Color32> { { 'g', g }, { 'f', f }, { 'w', w } };
        string[] rows =
        {
            "ffffffffffffffffffffffffffffffff",
            "fggggggggggggggggggggggggggggggf",
            "fgggwwggggggwwwgggggggggwggggggf",
            "fggggggggggggggggggwwwgggggggggf",
            "fgggggwwwwgggggggggggggggggggggf",
            "fggggggggggggggggggggggggwwgggggf",
            "fggggggggggggggggggggggggggggggf",
            "ffffffffffffffffffffffffffffffff",
        };
        return PixelArt.Make("chalkboard", rows, pal, 32);
    }
}
