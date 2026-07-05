using UnityEngine;

/// <summary>주울 수 있는 아이템. 필수템이면 교시 카운터에 반영된다.</summary>
public class PickupItem : Interactable
{
    public string itemName = "아이템";
    public bool isRequired = false;            // 교시 필수 아이템 여부
    [TextArea] public string pickupLine = "";  // 주울 때 나오는 독백(선택)

    public override string Prompt => $"[E] {itemName}";

    public override void Interact()
    {
        if (GameManager.Instance != null) GameManager.Instance.Collect(this);
        gameObject.SetActive(false);
    }
}
