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

    // 만약 해당 오브젝트에 오너가 등록되어있다면
    // 호출되는 이벤트는 해당 오너의 이벤트로 제한한다.
    GameObject InherentOwner;

    Dictionary<KeyCode, Stack<Event>> keyEvents;
    bool isLockContorl;

    protected new void Awake()
    {
        base.Awake();

        // 모든 키배열을 사전으로 만든다.
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

        // 어떠한 키라도 입력이 되었을 때.
        if(Input.anyKeyDown)
        {
            // 키배열을 돌면서 어떤 키인지 확인.
            foreach(KeyCode key in keyEvents.Keys)
            {
                // 해당 키를 누른게 아니라면..
                if (!Input.GetKeyDown(key))
                    continue;

                // 키 이벤트 내부 스택의 개수가 0이상이라면
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

    // 호출시킬 입력 이벤트!!
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

    // 고유한 오너에 등록시키면 해당 오너의 이벤트만 불리게 한다.
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
