using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 실물 씬(학교 에셋 등)에 1인칭 플레이어 + 기본 UI를 한 번에 넣는 도구.
/// 메뉴 [SchoolDay ▸ 현재 씬에 플레이어 넣기]. 스폰 위치는 지금 보고 있는 씬뷰 앞쪽.
/// 넣은 뒤 Scene에서 교실 바닥 위로 옮기고 Play 하면 걸어다닐 수 있다.
/// </summary>
public static class PlayerSetup
{
    [MenuItem("SchoolDay/현재 씬에 플레이어 넣기")]
    static void SpawnPlayer()
    {
        var existing = Object.FindFirstObjectByType<FirstPersonController>();
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("[SchoolDay] 이미 플레이어가 있습니다. 위치만 옮겨서 쓰세요.");
            return;
        }

        // 기존 카메라들은 꺼서 플레이어 카메라가 화면을 잡게 한다
        foreach (var c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            c.gameObject.SetActive(false);

        // 스폰 위치: 지금 씬뷰가 보고 있는 지점 앞쪽(없으면 원점 위)
        Vector3 pos = new Vector3(0f, 1f, 0f);
        var sv = SceneView.lastActiveSceneView;
        if (sv != null)
        {
            var t = sv.camera.transform;
            pos = t.position + t.forward * 3f;
            pos.y = 1f; // 대략 바닥 위 — 이후 손으로 조정
        }

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

        // 기본 UI 매니저
        if (Object.FindFirstObjectByType<HUD>() == null)
            new GameObject("HUD").AddComponent<HUD>();
        if (Object.FindFirstObjectByType<DialogueUI>() == null)
            new GameObject("Dialogue").AddComponent<DialogueUI>();

        Selection.activeGameObject = player;
        Debug.Log("[SchoolDay] 플레이어 생성 완료. Scene에서 교실 바닥 위로 옮긴 뒤 ▶ Play 하세요.");
    }
}
