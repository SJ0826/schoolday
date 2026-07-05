using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 카메라 정면으로 레이캐스트해 조준한 Interactable 을 감지하고 [E]로 상호작용한다.
/// 오프닝·대사 연출 중(controlEnabled=false)에는 동작하지 않는다.
/// </summary>
[RequireComponent(typeof(FirstPersonController))]
public class PlayerInteractor : MonoBehaviour
{
    public float range = 3f;

    Camera cam;
    FirstPersonController fpc;
    HUD hud;
    Interactable current;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        fpc = GetComponent<FirstPersonController>();
        hud = FindFirstObjectByType<HUD>();
        if (hud == null) hud = new GameObject("HUD").AddComponent<HUD>();
    }

    void Update()
    {
        // 연출 중엔 상호작용 잠금
        if (fpc != null && !fpc.controlEnabled) { SetCurrent(null); return; }
        if (cam == null) return;

        Interactable found = null;
        var ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out var hit, range))
            found = hit.collider.GetComponentInParent<Interactable>();

        SetCurrent(found);

        if (current != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            current.Interact();
    }

    void SetCurrent(Interactable it)
    {
        current = it;
        if (hud != null) hud.SetPrompt(it != null ? it.Prompt : "");
    }
}
