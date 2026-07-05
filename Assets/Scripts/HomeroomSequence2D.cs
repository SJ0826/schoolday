using System.Collections;
using UnityEngine;

/// <summary>
/// 2D 조례: 자리에 앉으면 담임이 들어와 교탁으로 이동 → 짧은 조례 + 의미심장한 멘트 → 1교시 시작.
/// SitPoint2D 가 Begin()을 호출한다.
/// </summary>
public class HomeroomSequence2D : MonoBehaviour
{
    public Transform teacher;                       // 담임(시작엔 비활성)
    public Vector2 podium = new Vector2(0f, 2.2f);  // 교탁(칠판 앞) 위치
    public float moveDuration = 2f;

    [TextArea]
    public string[] lines =
    {
        "담임  —  자, 다들 앉았지? 조례 시작하자.",
        "담임  —  오늘 특별한 공지는 없다. 1교시 국어 준비하고.",
        "담임  —  아, 그리고 하나만.",
        "담임  —  인생은 짧지 않고 길어. 당장 힘들다고 쉽게 포기하면, 어른이 됐을 때 더 힘들어진다.",
        "담임  —  물론… 도망치는 것도 때론 좋은 선택이야. 도망칠 수 있을 때는, 도망치도록.",
        "담임  —  자, 오늘도 잘해보자.",
    };

    bool started;

    public void Begin()
    {
        if (started) return;
        started = true;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        var pc = FindFirstObjectByType<PlayerController2D>();
        var dlg = FindFirstObjectByType<DialogueUI>();
        var hud = FindFirstObjectByType<HUD>();

        if (pc != null) pc.controlEnabled = false;
        yield return new WaitForSeconds(0.5f);

        if (teacher != null)
        {
            teacher.gameObject.SetActive(true);
            yield return Move(teacher, podium, moveDuration);
        }

        if (dlg != null) yield return dlg.PlayLines(lines);

        if (hud != null) hud.ShowToast("1교시  —  국어", 3f);
        if (pc != null) pc.controlEnabled = true;
    }

    static IEnumerator Move(Transform t, Vector2 dest, float dur)
    {
        Vector2 p0 = t.position;
        float e = 0f;
        while (e < dur)
        {
            e += Time.deltaTime;
            t.position = Vector2.Lerp(p0, dest, Mathf.SmoothStep(0f, 1f, e / dur));
            yield return null;
        }
        t.position = dest;
    }
}
