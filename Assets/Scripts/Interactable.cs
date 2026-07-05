using UnityEngine;

/// <summary>조준해서 [E]로 상호작용할 수 있는 대상의 공통 인터페이스.</summary>
public abstract class Interactable : MonoBehaviour
{
    /// <summary>조준 시 화면에 뜨는 안내(예: "[E] 사원증").</summary>
    public abstract string Prompt { get; }

    /// <summary>[E]를 눌렀을 때 실행.</summary>
    public abstract void Interact();
}
