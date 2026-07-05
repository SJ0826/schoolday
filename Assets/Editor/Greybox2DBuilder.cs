using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 2D 사이드뷰 교실 생성기 (일러스트 배경 기반).
/// 메뉴 [SchoolDay ▸ 2D 교실 만들기] → Assets/Scenes/Classroom2D.unity 생성.
/// classroom_bg.png 를 배경으로 깔고, 그 위에 플레이어·NPC(임시 도트)·상호작용·조례를 배치.
/// 캐릭터 일러스트가 오면 임시 도트만 교체하면 된다.
/// </summary>
public static class Greybox2DBuilder
{
    const string BG_PATH = "Assets/Sprites/classroom_bg.png";
    const float FLOOR_Y = -2.9f;   // 배경상 바닥(캐릭터 발 높이) — Play 후 미세조정
    const float LEFT = -8f, RIGHT = 8f;

    [MenuItem("SchoolDay/2D 교실 만들기")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var bg = EnsureBackground();

        var root = new GameObject("Classroom2D");

        // 배경 (일러스트 한 장)
        if (bg != null) MakeSR("Background", root.transform, bg, Vector2.zero, -10);

        // 바닥/좌우 끝 (안 보이는 콜라이더)
        EdgeWall(root.transform, new Vector2(LEFT - 0.5f, 0f));
        EdgeWall(root.transform, new Vector2(RIGHT + 0.5f, 0f));

        // 플레이어 (임시 도트 — 곧 일러스트로 교체)
        var player = new GameObject("Player");
        var psr = player.AddComponent<SpriteRenderer>();
        psr.sortingOrder = 5;
        var girl = EnsureCharacter("girl", 560);
        if (girl != null)
        {
            psr.sprite = girl;                                       // 일러스트(바닥 피벗)
            player.transform.position = new Vector3(LEFT + 1.5f, FLOOR_Y, 0f);
        }
        else
        {
            psr.sprite = StudentSprite("player", new Color32(60, 96, 150, 255));
            player.transform.position = new Vector3(LEFT + 1.5f, FLOOR_Y + 0.75f, 0f);
            player.transform.localScale = Vector3.one * 1.6f;
        }
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.freezeRotation = true;
        var pcol = player.AddComponent<CapsuleCollider2D>();
        pcol.size = new Vector2(0.7f, 2.4f); pcol.offset = new Vector2(0f, 1.2f);
        player.AddComponent<PlayerController2D>();
        player.AddComponent<PlayerInteractor2D>();

        // 매니저
        var dlg = new GameObject("Dialogue"); dlg.transform.SetParent(root.transform); dlg.AddComponent<DialogueUI>();
        var hudGo = new GameObject("HUD"); hudGo.transform.SetParent(root.transform); hudGo.AddComponent<HUD>().SetCrosshair(false);
        var opening = new GameObject("OpeningSequence"); opening.transform.SetParent(root.transform); opening.AddComponent<OpeningSequence2D>();

        // 친구 NPC (임시 도트)
        Npc(root.transform, "짝꿍", new Color32(170, 80, 80, 255), new Vector2(-3.5f, FLOOR_Y + 0.75f),
            new[] { "왔냐? 침 흘리고 자더라.", "조례 곧 한대.", "오늘 급식 뭐냐?" });
        Npc(root.transform, "반장", new Color32(90, 140, 90, 255), new Vector2(1.5f, FLOOR_Y + 0.75f),
            new[] { "자리 앉는 게 좋을걸.", "숙제 했어?" });

        // 칠판 조사 (배경의 칠판 위치에 보이지 않는 트리거)
        var boardZone = new GameObject("ChalkboardZone");
        boardZone.transform.SetParent(root.transform);
        boardZone.transform.position = new Vector3(4.8f, FLOOR_Y + 1f, 0f);
        AddTrigger(boardZone, new Vector2(3f, 5f));   // 바닥~칠판 세로로 넉넉히(발 기준 감지)
        var ex = boardZone.AddComponent<ExamineObject>();
        ex.label = "칠판"; ex.line = "칠판엔 오늘 시간표가 적혀 있다. 조례 · 1교시 국어…";

        // 자기 자리(앉기) + 담임 + 조례
        var sit = new GameObject("SitPoint");
        sit.transform.SetParent(root.transform);
        sit.transform.position = new Vector3(LEFT + 1.5f, FLOOR_Y + 0.6f, 0f);
        AddTrigger(sit, new Vector2(1.4f, 2.8f));
        sit.AddComponent<SitPoint2D>();

        var teacher = new GameObject("Teacher");
        teacher.transform.SetParent(root.transform);
        teacher.transform.position = new Vector3(RIGHT - 1f, FLOOR_Y + 0.85f, 0f);
        teacher.transform.localScale = Vector3.one * 1.75f;
        var tsr = teacher.AddComponent<SpriteRenderer>();
        tsr.sprite = StudentSprite("teacher", new Color32(88, 90, 104, 255));
        tsr.sortingOrder = 5;
        teacher.SetActive(false);

        var hr = new GameObject("HomeroomSequence");
        hr.transform.SetParent(root.transform);
        var homeroom = hr.AddComponent<HomeroomSequence2D>();
        homeroom.teacher = teacher.transform;
        homeroom.podium = new Vector2(4.8f, FLOOR_Y + 0.85f);

        // 카메라 (배경 높이에 맞춤, 가로 스크롤)
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 4.435f; // 배경 세로(887px/100/2)
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.07f);
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.tag = "MainCamera";
        camGo.AddComponent<AudioListener>();
        var follow = camGo.AddComponent<CameraFollow2D>();
        follow.target = player.transform; follow.lockY = true; follow.fixedY = 0f;
        follow.clampToWorld = true; follow.worldLeft = -8.87f; follow.worldRight = 8.87f; // 배경 폭 1774px/100/2

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Classroom2D.unity");
        Debug.Log("[SchoolDay] 일러스트 배경 교실 생성 완료 → Assets/Scenes/Classroom2D.unity");
    }

    // ---- 헬퍼 ----

    static Sprite EnsureBackground()
    {
        var ti = AssetImporter.GetAtPath(BG_PATH) as TextureImporter;
        if (ti == null) { Debug.LogWarning($"[SchoolDay] 배경 없음: {BG_PATH}"); return null; }
        if (ti.textureType != TextureImporterType.Sprite || ti.spritePixelsPerUnit != 100 || ti.spriteImportMode != SpriteImportMode.Single)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100;      // 1774x887 → 약 17.7 x 8.9 유닛
            ti.filterMode = FilterMode.Bilinear; // 일러스트라 부드럽게
            ti.maxTextureSize = 2048;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(BG_PATH);
    }

    // 흰 배경 캐릭터 일러스트를 투명 처리 + Sprite(바닥 피벗)로. 이미 처리됐으면 그대로 로드.
    static Sprite EnsureCharacter(string name, int ppu)
    {
        string path = $"Assets/Sprites/{name}.png";
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return null;
        if (ti.textureType == TextureImporterType.Sprite && ti.spritePixelsPerUnit == ppu)
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        return BackgroundCutter.CutAndImport(path, ppu, true);
    }

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
        go.AddComponent<BoxCollider2D>().size = new Vector2(1f, 12f);
    }

    static void Npc(Transform parent, string npcName, Color32 uniform, Vector2 pos, string[] lines)
    {
        var go = new GameObject($"NPC_{npcName}");
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 1.6f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = StudentSprite($"student_{npcName}", uniform);
        sr.sortingOrder = 5;
        AddTrigger(go, new Vector2(0.9f, 2.8f));
        var npc = go.AddComponent<TalkableNPC>();
        npc.npcName = npcName; npc.lines = lines;
    }

    // 임시 옆모습 학생 도트 (일러스트 오면 교체)
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
}
