using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 수집과 교시 필수템 진행을 관리한다.
/// 씬에 놓인 PickupItem 중 isRequired 인 것을 세어 총 개수로 삼는다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    readonly List<PickupItem> collected = new();
    int requiredTotal;
    int requiredGot;
    HUD hud;
    DialogueUI dialogue;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        requiredTotal = 0;
        foreach (var it in FindObjectsByType<PickupItem>(FindObjectsSortMode.None))
            if (it.isRequired) requiredTotal++;

        hud = FindFirstObjectByType<HUD>();
        dialogue = FindFirstObjectByType<DialogueUI>();
        if (hud != null) hud.SetCounter(requiredGot, requiredTotal);
    }

    public void Collect(PickupItem item)
    {
        collected.Add(item);
        if (item.isRequired) requiredGot++;

        if (hud != null)
        {
            hud.ShowToast($"'{item.itemName}' 획득");
            hud.SetCounter(requiredGot, requiredTotal);
        }

        if (!string.IsNullOrEmpty(item.pickupLine))
            StartCoroutine(SayLine(item.pickupLine));
    }

    IEnumerator SayLine(string line)
    {
        var d = dialogue != null ? dialogue : FindFirstObjectByType<DialogueUI>();
        if (d == null) yield break;
        yield return new WaitForSeconds(0.6f);
        d.Show(line);
        yield return new WaitForSeconds(2.5f);
        d.Hide();
    }
}
