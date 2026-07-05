using System.Collections;
using UnityEngine;

/// <summary>
/// 조례 시퀀스: 자리에 앉으면 시작 → 담임 입장(교탁으로) → 짧은 조례 + 의미심장한 멘트 → 1교시 시작.
/// SitPoint 가 Begin()을 호출한다.
/// </summary>
public class HomeroomSequence : MonoBehaviour
{
    public Transform teacher;                                  // 담임(시작엔 비활성)
    public Vector3 teacherPodium = new Vector3(-2f, 0.9f, 5.5f); // 교탁 앞 위치

    [TextArea]
    public string[] lines =
    {
        "담임  —  자, 다들 앉았지? 조례 시작하자.",
        "담임  —  오늘 특별한 공지는 없다. 1교시 국어 준비하고.",
        "담임  —  아, 그리고 하나만.",
        "담임  —  자자, 인생은 짧지 않고 길어. 당장 어렵거나 힘들어도 쉽게 포기하면, 나중에 어른이 됐을 때 너무 힘들어.",
        "담임  —  물론… 도망치는 것도 때론 좋은 선택이란다. 도망칠 수 있을 때는, 도망치도록.",
        "담임  —  그럼, 오늘도 잘해보자.",
    };

    public float teacherMoveDuration = 2.5f;

    bool started;

    public void Begin(Vector3 seatPos)
    {
        if (started) return;
        started = true;
        StartCoroutine(Run(seatPos));
    }

    IEnumerator Run(Vector3 seatPos)
    {
        var player = FindFirstObjectByType<FirstPersonController>();
        var dialogue = FindFirstObjectByType<DialogueUI>();
        var hud = FindFirstObjectByType<HUD>();

        // 자리에 앉힌다(위치 고정 + 칠판 방향) + 입력 잠금
        if (player != null)
        {
            player.controlEnabled = false;
            player.transform.position = seatPos;
            player.transform.rotation = Quaternion.identity; // +z(칠판/담임) 방향
            if (hud != null) hud.SetPrompt("");
        }

        yield return new WaitForSeconds(0.8f);

        // 담임 입장 → 교탁으로
        if (teacher != null)
        {
            teacher.gameObject.SetActive(true);
            yield return MoveTeacher(teacher, teacherPodium, teacherMoveDuration);
        }

        // 조례 대사
        if (dialogue != null)
            yield return dialogue.PlayLines(lines);

        // 1교시 시작
        if (hud != null) hud.ShowToast("1교시  —  국어", 3f);

        if (player != null) player.controlEnabled = true;
    }

    static IEnumerator MoveTeacher(Transform t, Vector3 dest, float dur)
    {
        Vector3 p0 = t.position;
        Quaternion r0 = t.rotation;
        Quaternion r1 = Quaternion.Euler(0f, 180f, 0f); // 학생(-z) 쪽을 바라봄
        float e = 0f;
        while (e < dur)
        {
            e += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, e / dur);
            t.position = Vector3.Lerp(p0, dest, k);
            t.rotation = Quaternion.Slerp(r0, r1, k);
            yield return null;
        }
        t.position = dest;
        t.rotation = r1;
    }
}
