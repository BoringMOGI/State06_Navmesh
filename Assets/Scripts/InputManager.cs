using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class InputManager : Singleton<InputManager>
{
    struct Event
    {
        public GameObject owner;
        public Action onEvent;
    }

    // ���� �ش� ������Ʈ�� ���ʰ� ��ϵǾ��ִٸ�
    // ȣ��Ǵ� �̺�Ʈ�� �ش� ������ �̺�Ʈ�� �����Ѵ�.
    GameObject InherentOwner;

    Dictionary<KeyCode, Stack<Event>> keyEvents;
    bool isLockContorl;

    protected new void Awake()
    {
        base.Awake();

        // ��� Ű�迭�� �������� �����.
        keyEvents = new Dictionary<KeyCode, Stack<Event>>();
        foreach(KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (keyEvents.ContainsKey(key))
                continue;

            keyEvents.Add(key, new Stack<Event>());
        }
    }
    private void Update()
    {
        if (isLockContorl)
            return;

        // ��� Ű�� �Է��� �Ǿ��� ��.
        if(Input.anyKeyDown)
        {
            // Ű�迭�� ���鼭 � Ű���� Ȯ��.
            foreach(KeyCode key in keyEvents.Keys)
            {
                // �ش� Ű�� ������ �ƴ϶��..
                if (!Input.GetKeyDown(key))
                    continue;

                // Ű �̺�Ʈ ���� ������ ������ 0�̻��̶��
                if (keyEvents[key].Count > 0)
                    OnInputEvent(keyEvents[key].Peek());

                break;
            }
        }
    }

    public void Lock(bool isLock)
    {
        isLockContorl = isLock;
    }

    // ȣ���ų �Է� �̺�Ʈ!!
    private void OnInputEvent(Event e)
    {
        if(InherentOwner != null)
        {
            if (e.owner == InherentOwner)
                e.onEvent?.Invoke();
        }
        else
        {
            e.onEvent?.Invoke();
        }
    }

    // ������ ���ʿ� ��Ͻ�Ű�� �ش� ������ �̺�Ʈ�� �Ҹ��� �Ѵ�.
    public void AddInherentOwner(GameObject owner)
    {
        InherentOwner = owner;
    }
    public void RemoveInherentOnwer()
    {
        InherentOwner = null;
    }

    public void AddEvent(KeyCode key, GameObject owner, Action onEvent)
    {
        keyEvents[key].Push(new Event() { owner = owner, onEvent = onEvent } );
    }
    public void RemoveEvent(KeyCode key)
    {
        keyEvents[key].Pop();
    }
}
