using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 실물 씬(학교 에셋 등)에 1인칭 플레이어 + 기본 UI를 넣는 도구.
/// - [SchoolDay ▸ 선택한 바닥 위에 플레이어 놓기]: 고른 바닥 오브젝트의 표면 중앙 위로 플레이어를 정확히 배치(권장)
/// - [SchoolDay ▸ 현재 씬에 플레이어 넣기]: 씬뷰가 보는 위치에 대충 스폰(이후 손으로 이동)
/// </summary>
public static class PlayerSetup
{
    [MenuItem("SchoolDay/선택한 바닥 위에 플레이어 놓기")]
    static void PlaceOnSelected()
    {
        var sel = Selection.activeGameObject;
        var rend = sel != null ? sel.GetComponentInChildren<Renderer>() : null;
        if (rend == null)
        {
            Debug.LogWarning("[SchoolDay] 먼저 바닥(floor 등, 눈에 보이는) 오브젝트를 하나 선택하세요.");
            return;
        }

        var b = rend.bounds;                                   // 월드 기준 경계
        Vector3 spot = new Vector3(b.center.x, b.max.y + 0.2f, b.center.z);  // 바닥 표면 중앙 위

        var player = Object.FindFirstObjectByType<FirstPersonController>();
        if (player == null)
        {
            SpawnPlayerAt(spot);
        }
        else
        {
            player.transform.position = spot;
            player.transform.rotation = Quaternion.identity;
            Selection.activeGameObject = player.gameObject;
        }
        Debug.Log($"[SchoolDay] 플레이어를 '{sel.name}' 바닥 위({spot})에 놓았습니다. ▶ Play 하세요.");
    }

    [MenuItem("SchoolDay/현재 씬에 플레이어 넣기")]
    static void SpawnAtView()
    {
        if (Object.FindFirstObjectByType<FirstPersonController>() != null)
        {
            Debug.Log("[SchoolDay] 이미 플레이어가 있습니다. '선택한 바닥 위에 플레이어 놓기'로 위치를 잡으세요.");
            return;
        }
        Vector3 pos = new Vector3(0f, 1f, 0f);
        var sv = SceneView.lastActiveSceneView;
        if (sv != null)
        {
            var t = sv.camera.transform;
            pos = t.position + t.forward * 3f;
            pos.y = 1f;
        }
        SpawnPlayerAt(pos);
    }

    static void SpawnPlayerAt(Vector3 pos)
    {
        // 기존 카메라는 꺼서 플레이어 카메라가 화면을 잡게 한다
        foreach (var c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            c.gameObject.SetActive(false);

        var player = new GameObject("Player");
        player.transform.position = pos;
        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.7f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0.85f, 0f);

        var camGo = new GameObject("Camera");
        camGo.transform.SetParent(player.transform);
        camGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        var cam = camGo.AddComponent<Camera>();
        camGo.AddComponent<AudioListener>();
        camGo.tag = "MainCamera";
        var camData = cam.GetUniversalAdditionalCameraData();
        if (camData != null) camData.renderPostProcessing = true;

        player.AddComponent<FirstPersonController>();
        player.AddComponent<PlayerInteractor>();

        if (Object.FindFirstObjectByType<HUD>() == null)
            new GameObject("HUD").AddComponent<HUD>();
        if (Object.FindFirstObjectByType<DialogueUI>() == null)
            new GameObject("Dialogue").AddComponent<DialogueUI>();

        Selection.activeGameObject = player;
    }
}
