using System.Collections;
using UnityEngine;

/// <summary>
/// 조준 [E]로 '살펴보는' 대상. 줍지 않고 독백만 띄운다(위화감 연출용).
/// once=true 면 한 번만, false 면 볼 때마다 반복.
/// </summary>
public class ExamineObject : Interactable
{
    public string label = "살펴보기";
    [TextArea] public string line = "";
    public bool once = false;
    public float showDuration = 3f;

    bool used;
    DialogueUI dialogue;
    Coroutine co;

    public override string Prompt => (once && used) ? "" : $"[E] {label}";

    public override void Interact()
    {
        if (once && used) return;
        used = true;

        if (dialogue == null) dialogue = FindFirstObjectByType<DialogueUI>();
        if (dialogue == null) return;

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Show());
    }

    IEnumerator Show()
    {
        dialogue.Show(line);
        yield return new WaitForSeconds(showDuration);
        dialogue.Hide();
    }
}
