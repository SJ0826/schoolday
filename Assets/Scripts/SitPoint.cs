using UnityEngine;

/// <summary>
/// 자기 자리 책상. [E]로 앉으면 조례가 시작된다(한 번만).
/// seatPosition 은 씬 빌더가 플레이어를 앉힐 위치로 설정한다.
/// </summary>
public class SitPoint : Interactable
{
    public string label = "자리에 앉기";
    public Vector3 seatPosition;

    bool used;

    public override string Prompt => used ? "" : $"[E] {label}";

    public override void Interact()
    {
        if (used) return;
        used = true;

        var hr = FindFirstObjectByType<HomeroomSequence>();
        if (hr != null) hr.Begin(seatPosition);
    }
}
