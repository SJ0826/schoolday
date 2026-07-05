using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 0단계: 그레이박스 교실 생성기.
/// 메뉴 [SchoolDay ▸ 그레이박스 교실 만들기] 를 누르면
/// Assets/Scenes/Classroom.unity 를 새로 만들고, 회색 상자(프리미티브)로
/// 교실 + 조명 + 1인칭 플레이어를 배치한다. 에셋은 나중에 교체한다.
/// 다시 눌러도 되도록 씬을 통째로 새로 생성한다.
/// </summary>
public static class GreyboxClassroomBuilder
{
    const float W = 8f;   // 교실 폭 (x)
    const float L = 9f;   // 교실 길이 (z), +z 쪽이 칠판(앞)
    const float H = 3f;   // 천장 높이 (y)

    [MenuItem("SchoolDay/그레이박스 교실 만들기")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var floorMat  = Mat("Greybox_Floor",   new Color(0.62f, 0.62f, 0.64f));
        var wallMat   = Mat("Greybox_Wall",    new Color(0.78f, 0.78f, 0.80f));
        var ceilMat   = Mat("Greybox_Ceiling", new Color(0.85f, 0.85f, 0.88f));
        var deskMat   = Mat("Greybox_Desk",    new Color(0.45f, 0.40f, 0.35f));
        var boardMat  = Mat("Greybox_Board",   new Color(0.15f, 0.28f, 0.20f));
        var doorMat   = Mat("Greybox_Door",    new Color(0.35f, 0.30f, 0.28f));
        var glassMat  = Mat("Greybox_Glass",   new Color(0.40f, 0.55f, 0.65f));
        var lockerMat = Mat("Greybox_Locker",  new Color(0.40f, 0.45f, 0.52f));

        var root = new GameObject("GreyboxClassroom");

        // 바닥 (Plane: scale 1 == 10m)
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(root.transform);
        floor.transform.localScale = new Vector3(W / 10f, 1f, L / 10f);
        SetMat(floor, floorMat);

        // 천장
        Box("Ceiling", new Vector3(0, H, 0), new Vector3(W, 0.15f, L), ceilMat, root.transform);

        // 벽 4개
        Box("Wall_Back",  new Vector3(0, H / 2f, -L / 2f), new Vector3(W, H, 0.2f), wallMat, root.transform);
        Box("Wall_Front", new Vector3(0, H / 2f,  L / 2f), new Vector3(W, H, 0.2f), wallMat, root.transform);
        Box("Wall_Left",  new Vector3(-W / 2f, H / 2f, 0), new Vector3(0.2f, H, L), wallMat, root.transform);
        Box("Wall_Right", new Vector3( W / 2f, H / 2f, 0), new Vector3(0.2f, H, L), wallMat, root.transform);

        // 칠판 (앞벽)
        Box("Chalkboard", new Vector3(0, 1.6f, L / 2f - 0.11f), new Vector3(4f, 1.2f, 0.05f), boardMat, root.transform);

        // 교탁 (앞쪽)
        Box("TeacherDesk", new Vector3(-2f, 0.4f, L / 2f - 1.5f), new Vector3(1.2f, 0.8f, 0.6f), deskMat, root.transform);

        // 문 (좌벽 뒤쪽)
        Box("Door", new Vector3(-W / 2f + 0.12f, 1.0f, -L / 2f + 1.3f), new Vector3(0.1f, 2.0f, 1.0f), doorMat, root.transform);

        // 창문 (우벽, 그레이박스 표시)
        Box("Window", new Vector3(W / 2f - 0.12f, 1.6f, 0f), new Vector3(0.08f, 1.2f, 4f), glassMat, root.transform);

        // 학생 책상 + 의자 그리드 (4열 x 4행)
        var desks = new GameObject("StudentDesks");
        desks.transform.SetParent(root.transform);
        const int cols = 4, rows = 4;
        const float gx = 1.6f, gz = 1.25f;
        float x0 = -(cols - 1) * gx / 2f;
        float z0 = L / 2f - 3.0f;            // 앞줄부터 시작
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float px = x0 + c * gx;
                float pz = z0 - r * gz;
                Box($"Desk_{r}_{c}",  new Vector3(px, 0.38f, pz),        new Vector3(0.6f, 0.75f, 0.5f), deskMat, desks.transform);
                Box($"Chair_{r}_{c}", new Vector3(px, 0.25f, pz - 0.4f), new Vector3(0.45f, 0.5f, 0.45f), deskMat, desks.transform);
            }
        }

        // 사물함 (뒷벽을 따라)
        var lockers = new GameObject("Lockers");
        lockers.transform.SetParent(root.transform);
        const int nl = 8;
        const float lw = 0.45f;
        float lx0 = -(nl - 1) * (lw + 0.02f) / 2f;
        for (int i = 0; i < nl; i++)
            Box($"Locker_{i}", new Vector3(lx0 + i * (lw + 0.02f), 0.9f, -L / 2f + 0.35f), new Vector3(lw, 1.8f, 0.4f), lockerMat, lockers.transform);

        // 조명
        var lightGo = new GameObject("Directional Light");
        lightGo.transform.SetParent(root.transform);
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.0f;
        light.color = new Color(1f, 0.98f, 0.92f);
        light.shadows = LightShadows.Soft;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.35f, 0.35f, 0.40f);

        // 1인칭 플레이어 (교실 뒤쪽에서 칠판을 바라봄)
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 0.9f, -L / 2f + 1.5f);
        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.7f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0.85f, 0f);
        var camGo = new GameObject("Camera");
        camGo.transform.SetParent(player.transform);
        camGo.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        camGo.AddComponent<Camera>();
        camGo.AddComponent<AudioListener>();
        camGo.tag = "MainCamera";
        player.AddComponent<FirstPersonController>();

        // 저장
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Classroom.unity");
        AssetDatabase.SaveAssets();
        Debug.Log("[SchoolDay] 그레이박스 교실 생성 완료 → Assets/Scenes/Classroom.unity (Play 눌러 걸어다녀 보세요)");
    }

    static void Box(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        SetMat(go, mat);
    }

    static void SetMat(GameObject go, Material mat)
    {
        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    // URP Lit 머티리얼을 만들어(있으면 색만 갱신) Assets/Materials/Greybox 에 저장
    static Material Mat(string name, Color color)
    {
        const string dir = "Assets/Materials/Greybox";
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets/Materials", "Greybox");

        string path = $"{dir}/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.color = color;
            existing.SetColor("_BaseColor", color);
            return existing;
        }

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader) { color = color };
        mat.SetColor("_BaseColor", color);   // URP Lit 은 _BaseColor 를 쓴다
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}
