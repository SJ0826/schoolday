using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2단계 오프닝: 책상에 엎드려 자다 친구가 깨운다.
/// 엎드림(카메라 다운) → 친구 대사 → 기상(카메라 업) → 자유 이동.
/// 대사는 Space/Enter/좌클릭으로 진행한다.
/// </summary>
public class OpeningSequence : MonoBehaviour
{
    [TextArea]
    public string[] lines =
    {
        "친구  —  야, 닌 오자마자 자냐?",
        "친구  —  담임 뜨기 전에 정신 차려라.",
        "( WASD 이동 · 마우스 시점 · Shift 달리기 )"
    };

    public float sleepHold = 1.5f;   // 자고 있는 시간
    public float wakeDuration = 1.2f; // 기상 연출 시간

    // 카메라 로컬 포즈 — 엎드림 / 정상(눈높이)
    readonly Vector3 sleepPos = new Vector3(0.10f, -0.05f, 0.15f);
    readonly Vector3 sleepEuler = new Vector3(12f, 0f, 82f);
    readonly Vector3 wakePos = new Vector3(0f, 0.7f, 0f);

    FirstPersonController player;
    Transform camT;
    DialogueUI dialogue;

    IEnumerator Start()
    {
        player = FindFirstObjectByType<FirstPersonController>();
        if (player == null) { Debug.LogWarning("[Opening] Player 없음"); yield break; }

        var cam = player.GetComponentInChildren<Camera>();
        if (cam == null) { Debug.LogWarning("[Opening] Camera 없음"); yield break; }
        camT = cam.transform;

        dialogue = FindFirstObjectByType<DialogueUI>();
        if (dialogue == null)
            dialogue = new GameObject("Dialogue").AddComponent<DialogueUI>();

        // 잠금 + 엎드린 자세
        player.controlEnabled = false;
        camT.localPosition = sleepPos;
        camT.localRotation = Quaternion.Euler(sleepEuler);

        yield return new WaitForSeconds(sleepHold);

        // 친구가 깨우는 첫 대사
        yield return Line(lines.Length > 0 ? lines[0] : "일어나.");

        // 기상 연출
        yield return WakeUp();

        // 나머지 대사
        for (int i = 1; i < lines.Length; i++)
            yield return Line(lines[i]);

        dialogue.Hide();
        player.controlEnabled = true;   // 이후 컨트롤러가 카메라 회전을 이어받는다
    }

    IEnumerator Line(string text)
    {
        dialogue.Show(text);
        yield return null;              // 표시된 프레임의 입력은 무시
        while (!Advance()) yield return null;
    }

    static bool Advance()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        return (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
            || (mouse != null && mouse.leftButton.wasPressedThisFrame);
    }

    IEnumerator WakeUp()
    {
        Vector3 p0 = camT.localPosition;
        Quaternion r0 = camT.localRotation;
        float t = 0f;
        while (t < wakeDuration)
        {
            t += Time.deltaTime;
            float e = Mathf.SmoothStep(0f, 1f, t / wakeDuration);
            camT.localPosition = Vector3.Lerp(p0, wakePos, e);
            camT.localRotation = Quaternion.Slerp(r0, Quaternion.identity, e);
            yield return null;
        }
        camT.localPosition = wakePos;
        camT.localRotation = Quaternion.identity;
    }
}
