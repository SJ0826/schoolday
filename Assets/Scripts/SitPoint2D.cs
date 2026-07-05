using UnityEngine;

/// <summary>자기 자리. [E]로 앉으면 조례가 시작된다(한 번만).</summary>
public class SitPoint2D : Interactable
{
    public string label = "자리에 앉기";
    bool used;

    public override string Prompt => used ? "" : $"[E] {label}";

    public override void Interact()
    {
        if (used) return;
        used = true;
        var hr = FindFirstObjectByType<HomeroomSequence2D>();
        if (hr != null) hr.Begin();
    }
}
