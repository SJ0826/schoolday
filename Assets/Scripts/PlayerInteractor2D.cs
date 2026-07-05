using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D 탑다운 근접 상호작용. 플레이어 주변 range 안의 가장 가까운 Interactable을 잡고
/// [E]로 상호작용한다. 연출 중(controlEnabled=false)에는 잠근다.
/// </summary>
[RequireComponent(typeof(PlayerController2D))]
public class PlayerInteractor2D : MonoBehaviour
{
    public float range = 1.3f;

    PlayerController2D pc;
    HUD hud;
    Interactable current;

    void Start()
    {
        pc = GetComponent<PlayerController2D>();
        hud = FindFirstObjectByType<HUD>();
        if (hud == null) hud = new GameObject("HUD").AddComponent<HUD>();
    }

    void Update()
    {
        if (pc != null && !pc.controlEnabled) { SetCurrent(null); return; }

        Interactable best = null;
        float bestDist = float.MaxValue;
        foreach (var col in Physics2D.OverlapCircleAll(transform.position, range))
        {
            var it = col.GetComponentInParent<Interactable>();
            if (it == null) continue;
            float d = Vector2.Distance(transform.position, col.transform.position);
            if (d < bestDist) { bestDist = d; best = it; }
        }
        SetCurrent(best);

        if (current != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            current.Interact();
    }

    void SetCurrent(Interactable it)
    {
        current = it;
        if (hud != null) hud.SetPrompt(it != null ? it.Prompt : "");
    }
}
