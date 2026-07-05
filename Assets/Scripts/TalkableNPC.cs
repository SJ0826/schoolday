using System.Collections;
using UnityEngine;

/// <summary>
/// 말을 걸 수 있는 친구 NPC. [E]를 누를 때마다 대사를 순환한다(평범한 대답).
/// 오프닝에서는 전부 평범하게 — 후반에 어긋난 대사로 교체할 예정.
/// </summary>
public class TalkableNPC : Interactable
{
    public string npcName = "친구";
    [TextArea] public string[] lines = { "왔냐?" };
    public float showDuration = 2.5f;

    int idx;
    DialogueUI dialogue;
    Coroutine co;

    public override string Prompt => $"[E] {npcName}에게 말 걸기";

    public override void Interact()
    {
        if (lines == null || lines.Length == 0) return;
        if (dialogue == null) dialogue = FindFirstObjectByType<DialogueUI>();
        if (dialogue == null) return;

        string line = $"{npcName}  —  {lines[idx % lines.Length]}";
        idx++;

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Show(line));
    }

    IEnumerator Show(string line)
    {
        dialogue.Show(line);
        yield return new WaitForSeconds(showDuration);
        dialogue.Hide();
    }
}
