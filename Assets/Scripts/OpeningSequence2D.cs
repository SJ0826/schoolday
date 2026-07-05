using System.Collections;
using UnityEngine;

/// <summary>
/// 2D 오프닝: 책상에 엎드려 자다 친구가 깨운다.
/// 시작 시 이동을 잠그고 대사를 재생한 뒤 조작을 푼다. (연출·페이드는 이후 보강)
/// </summary>
public class OpeningSequence2D : MonoBehaviour
{
    [TextArea]
    public string[] lines =
    {
        "…(책상에 엎드려 자고 있다)",
        "짝꿍  —  야, 닌 오자마자 자냐?",
        "짝꿍  —  담임 뜨기 전에 정신 차려라.",
        "( WASD 이동 · E로 말 걸기/조사 · 자리에 앉으면 조례가 시작된다 )",
    };

    IEnumerator Start()
    {
        var pc = FindFirstObjectByType<PlayerController2D>();
        var dlg = FindFirstObjectByType<DialogueUI>();

        if (pc != null) pc.controlEnabled = false;
        yield return new WaitForSeconds(1f);

        if (dlg != null) yield return dlg.PlayLines(lines);

        if (pc != null) pc.controlEnabled = true;
    }
}
