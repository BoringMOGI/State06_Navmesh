using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IInteraction
{
    string Name { get; }
    KeyCode Key { get; }
    Vector3 Position { get; }    // 대상자의 오브젝트 위치.
    Vector3 UiPostion { get; }   // UI가 출력될 위치.

    void OnInteract();           // 동작을 하라!
}

public class TriggerButton : MonoBehaviour, IInteraction
{
    [System.Serializable]
    private struct NameField
    {
        public string uiName;       // ui 출력 이름.
        public string animName;     // animation clip 이름.
    }

    [Header("Variable")]
    [SerializeField] Transform uiPivot;     // UI가 출력될 위치.
    [SerializeField] Animation anim;        // <특정>버튼의 애니메이션.
    [SerializeField] NameField onName;      // on상태 이름.
    [SerializeField] NameField offName;     // off상태 이름.
    [SerializeField] bool isOn;             // 버튼이 눌린 상태인가?

    [Header("Interact")]
    [SerializeField] KeyCode shortcutKey = KeyCode.G;
    [SerializeField] UnityEvent<bool, System.Action> onAction;

    private bool isUse;     // 사용 중이다.

    #region 인터페이스 내용.

    string IInteraction.Name => isOn ? (onName.uiName) : (offName.uiName);
    KeyCode IInteraction.Key => shortcutKey;
    Vector3 IInteraction.UiPostion => uiPivot.position;
    Vector3 IInteraction.Position => transform.position;

    #endregion

    void IInteraction.OnInteract()
    {
        if (isUse)
        {
            Debug.Log("이미 작동 중입니다.");
            return;
        }

        isUse = true;        
        
        // 애니메이션 제어.
        // 동작 중인 상태에서 눌렀다면 -> 애니메이션은 비동작
        if (anim != null)
            anim.Play(isOn ? offName.animName : onName.animName);

        // 등록된 이벤트에서 어떠한 동작이 끝나면 상태를 변경한다.
        // 따라서 전달할 때는 반대의 값을 준다.
        onAction?.Invoke(!isOn, () => { 

            isUse = false;
            isOn = !isOn;
        });     
    }
}
