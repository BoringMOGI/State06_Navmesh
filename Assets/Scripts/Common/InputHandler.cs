using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputHandler : MonoBehaviour
{
    protected void AddEvent(KeyCode key, System.Action onEvent)
    {
        InputManager.Instance.AddEvent(key, gameObject, onEvent);
    }
    protected void RemoveEvent(KeyCode key)
    {
        InputManager.Instance.RemoveEvent(key);
    }

    protected void AddInherentOwner()
    {
        InputManager.Instance.AddInherentOwner(gameObject);
    }
    protected void RemoveInherentOwner()
    {
        InputManager.Instance.RemoveInherentOnwer();
    }

}
