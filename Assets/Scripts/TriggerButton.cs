using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IInteraction
{
    string Name { get; }
    KeyCode Key { get; }
    Vector3 Position { get; }    // ������� ������Ʈ ��ġ.
    Vector3 UiPostion { get; }   // UI�� ��µ� ��ġ.

    void OnInteract();           // ������ �϶�!
}

public class TriggerButton : MonoBehaviour, IInteraction
{
    [System.Serializable]
    private struct NameField
    {
        public string uiName;       // ui ��� �̸�.
        public string animName;     // animation clip �̸�.
    }

    [Header("Variable")]
    [SerializeField] Transform uiPivot;     // UI�� ��µ� ��ġ.
    [SerializeField] Animation anim;        // <Ư��>��ư�� �ִϸ��̼�.
    [SerializeField] NameField onName;      // on���� �̸�.
    [SerializeField] NameField offName;     // off���� �̸�.
    [SerializeField] bool isOn;             // ��ư�� ���� �����ΰ�?

    [Header("Interact")]
    [SerializeField] KeyCode shortcutKey = KeyCode.G;
    [SerializeField] UnityEvent<bool, System.Action> onAction;

    private bool isUse;     // ��� ���̴�.

    #region �������̽� ����.

    string IInteraction.Name => isOn ? (onName.uiName) : (offName.uiName);
    KeyCode IInteraction.Key => shortcutKey;
    Vector3 IInteraction.UiPostion => uiPivot.position;
    Vector3 IInteraction.Position => transform.position;

    #endregion

    void IInteraction.OnInteract()
    {
        if (isUse)
        {
            Debug.Log("�̹� �۵� ���Դϴ�.");
            return;
        }

        isUse = true;        
        
        // �ִϸ��̼� ����.
        // ���� ���� ���¿��� �����ٸ� -> �ִϸ��̼��� ����
        if (anim != null)
            anim.Play(isOn ? offName.animName : onName.animName);

        // ��ϵ� �̺�Ʈ���� ��� ������ ������ ���¸� �����Ѵ�.
        // ���� ������ ���� �ݴ��� ���� �ش�.
        onAction?.Invoke(!isOn, () => { 

            isUse = false;
            isOn = !isOn;
        });     
    }
}
