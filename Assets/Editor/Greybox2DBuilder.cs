using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 2D 사이드뷰(옆에서 보는) 교실 생성기.
/// 메뉴 [SchoolDay ▸ 2D 교실 만들기] → Assets/Scenes/Classroom2D.unity 생성.
/// 바닥·뒷벽·칠판·책상(옆모습)·옆모습 학생 캐릭터를 코드 도트로 배치. 좌우로 걷는다.
/// </summary>
public static class Greybox2DBuilder
{
    const float W = 22f;         // 교실 가로 길이(스크롤)
    const float FLOOR_Y = -3f;   // 바닥 표면 y (캐릭터 발 높이)

    [MenuItem("SchoolDay/2D 교실 만들기")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var floorSp = FloorSprite();
        var wallSp  = WallSprite();
        var deskSp  = DeskSprite();
        var boardSp = BoardSprite();

        var root = new GameObject("Classroom2D");

        // 뒷벽 (배경) + 바닥
        var wall = MakeSR("BackWall", root.transform, wallSp, new Vector2(0f, 1f), -3);
        wall.drawMode = SpriteDrawMode.Tiled; wall.size = new Vector2(W, 8f);
        var floor = MakeSR("Floor", root.transform, floorSp, new Vector2(0f, FLOOR_Y - 1.5f), -2);
        floor.drawMode = SpriteDrawMode.Tiled; floor.size = new Vector2(W, 3f);

        // 칠판 (뒷벽 중앙) — 조사 대상
        var board = MakeSR("Chalkboard", root.transform, boardSp, new Vector2(0f, 0.6f), -1);
        board.transform.localScale = new Vector3(5f, 3f, 1f);
        AddTrigger(board.gameObject, boardSp.bounds.size);
        var ex = board.gameObject.AddComponent<ExamineObject>();
        ex.label = "칠판"; ex.line = "[ 오늘의 시간표 ]  조례 · 1교시 국어…";

        // 책상들 (바닥 위 옆모습, 장식)
        var desks = new GameObject("Desks"); desks.transform.SetParent(root.transform);
        foreach (float dx in new[] { -8f, -5f, -2f, 1f, 4f, 7f })
            MakeSR($"Desk_{dx}", desks.transform, deskSp, new Vector2(dx, FLOOR_Y + 0.25f), 1);

        // 좌우 끝 벽(범위 제한, 안 보이는 콜라이더)
        EdgeWall(root.transform, new Vector2(-W / 2f, 0f));
        EdgeWall(root.transform, new Vector2( W / 2f, 0f));

        // 플레이어 (옆모습, 남색 교복)
        var player = new GameObject("Player");
        player.transform.position = new Vector3(-6f, FLOOR_Y + 0.5f, 0f);
        var psr = player.AddComponent<SpriteRenderer>();
        psr.sprite = StudentSprite("player", new Color32(60, 96, 150, 255));
        psr.sortingOrder = 3;
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.freezeRotation = true;
        var pcol = player.AddComponent<CapsuleCollider2D>(); pcol.size = new Vector2(0.5f, 1f);
        player.AddComponent<PlayerController2D>();     // sideView 기본 true
        player.AddComponent<PlayerInteractor2D>();

        // 매니저
        var dlg = new GameObject("Dialogue"); dlg.transform.SetParent(root.transform); dlg.AddComponent<DialogueUI>();
        var hudGo = new GameObject("HUD"); hudGo.transform.SetParent(root.transform); hudGo.AddComponent<HUD>().SetCrosshair(false);
        var opening = new GameObject("OpeningSequence"); opening.transform.SetParent(root.transform); opening.AddComponent<OpeningSequence2D>();

        // 친구 NPC
        Npc(root.transform, "짝꿍", new Color32(170, 80, 80, 255), new Vector2(-4f, FLOOR_Y + 0.5f),
            new[] { "왔냐? 침 흘리고 자더라.", "조례 곧 한대.", "오늘 급식 뭐냐?" });
        Npc(root.transform, "반장", new Color32(90, 140, 90, 255), new Vector2(6f, FLOOR_Y + 0.5f),
            new[] { "자리 앉는 게 좋을걸.", "숙제 했어?" });

        // 자기 자리(앉기) + 담임 + 조례
        var sit = new GameObject("SitPoint");
        sit.transform.SetParent(root.transform);
        sit.transform.position = new Vector3(-6f, FLOOR_Y + 0.5f, 0f);
        AddTrigger(sit, new Vector2(1.2f, 1.5f));
        sit.AddComponent<SitPoint2D>();

        var teacher = new GameObject("Teacher");
        teacher.transform.SetParent(root.transform);
        teacher.transform.position = new Vector3(9f, FLOOR_Y + 0.55f, 0f);
        teacher.transform.localScale = Vector3.one * 1.12f;
        var tsr = teacher.AddComponent<SpriteRenderer>();
        tsr.sprite = StudentSprite("teacher", new Color32(88, 90, 104, 255));
        tsr.sortingOrder = 2;
        teacher.SetActive(false);

        var hr = new GameObject("HomeroomSequence");
        hr.transform.SetParent(root.transform);
        var homeroom = hr.AddComponent<HomeroomSequence2D>();
        homeroom.teacher = teacher.transform;
        homeroom.podium = new Vector2(1f, FLOOR_Y + 0.55f);

        // 카메라 (직교 + 가로 팔로우, 세로 고정)
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 4.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.06f, 0.09f);
        camGo.transform.position = new Vector3(0f, -0.8f, -10f);
        camGo.tag = "MainCamera";
        camGo.AddComponent<AudioListener>();
        var follow = camGo.AddComponent<CameraFollow2D>();
        follow.target = player.transform; follow.lockY = true; follow.fixedY = -0.8f;

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Classroom2D.unity");
        Debug.Log("[SchoolDay] 2D 사이드뷰 교실 생성 완료 → Assets/Scenes/Classroom2D.unity");
    }

    // ---- 헬퍼 ----

    static SpriteRenderer MakeSR(string name, Transform parent, Sprite sp, Vector2 pos, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp; sr.sortingOrder = order;
        return sr;
    }

    static void AddTrigger(GameObject go, Vector2 size)
    {
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true; col.size = size;
    }

    static void EdgeWall(Transform parent, Vector2 pos)
    {
        var go = new GameObject("EdgeWall");
        go.transform.SetParent(parent);
        go.transform.position = pos;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 10f);
    }

    static void Npc(Transform parent, string npcName, Color32 uniform, Vector2 pos, string[] lines)
    {
        var go = new GameObject($"NPC_{npcName}");
        go.transform.SetParent(parent);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = StudentSprite($"student_{npcName}", uniform);
        sr.sortingOrder = 2;
        AddTrigger(go, new Vector2(0.6f, 1f));
        var npc = go.AddComponent<TalkableNPC>();
        npc.npcName = npcName; npc.lines = lines;
    }

    // ---- 도트 스프라이트 (옆모습) ----

    static Sprite StudentSprite(string name, Color32 uniform)
    {
        var pal = new Dictionary<char, Color32>
        {
            { 'h', new Color32(58, 40, 28, 255) },
            { 'f', new Color32(240, 200, 165, 255) },
            { 'b', uniform },
            { 's', new Color32(45, 40, 38, 255) },
        };
        string[] rows =
        {
            "  hhhhh   ",
            " hhhhhhh  ",
            " hhhhfff  ",
            " hhhhfff  ",
            "  hhffff  ",
            "   ffff   ",
            "  bbbbbb  ",
            " bbbbbbbb ",
            " bbbbbbbb ",
            "  bbbbbb  ",
            "  bb bb   ",
            "  bb bb   ",
            "  ss ss   ",
        };
        return PixelArt.Make(name, rows, pal, 13);
    }

    static Sprite DeskSprite()
    {
        var pal = new Dictionary<char, Color32>
        {
            { 't', new Color32(178, 130, 88, 255) },
            { 'e', new Color32(120, 84, 52, 255) },
            { 'l', new Color32(96, 66, 40, 255) },
        };
        string[] rows =
        {
            "tttttttttttttt",
            "tttttttttttttt",
            "eeeeeeeeeeeeee",
            "ll..........ll",
            "ll..........ll",
            "ll..........ll",
            "ll..........ll",
        };
        return PixelArt.Make("desk_side", rows, pal, 16);
    }

    static Sprite FloorSprite()
    {
        var a = new Color32(198, 158, 112, 255); // 마루
        var b = new Color32(168, 128, 84, 255);  // 판선
        var pal = new Dictionary<char, Color32> { { 'a', a }, { 'b', b } };
        var rows = new string[16];
        for (int y = 0; y < 16; y++)
            rows[y] = (y == 15) ? new string('b', 16)           // 표면 라인
                    : (y % 5 == 0) ? BuildRow(16, x => x % 4 == 0 ? 'b' : 'a')
                    : new string('a', 16);
        return PixelArt.Make("floor_side", rows, pal, 16);
    }

    static Sprite WallSprite()
    {
        var c = new Color32(216, 208, 190, 255);
        var d = new Color32(198, 190, 172, 255);
        var pal = new Dictionary<char, Color32> { { 'c', c }, { 'd', d } };
        var rows = new string[16];
        for (int y = 0; y < 16; y++)
            rows[y] = new string((y % 8 == 0) ? 'd' : 'c', 16);
        return PixelArt.Make("wall_side", rows, pal, 16);
    }

    static Sprite BoardSprite()
    {
        var g = new Color32(34, 64, 48, 255);
        var f = new Color32(150, 110, 70, 255);
        var w = new Color32(210, 214, 200, 255);
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

    static string BuildRow(int w, System.Func<int, char> f)
    {
        var arr = new char[w];
        for (int x = 0; x < w; x++) arr[x] = f(x);
        return new string(arr);
    }
}
