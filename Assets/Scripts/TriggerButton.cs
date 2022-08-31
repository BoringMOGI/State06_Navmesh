using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IInteraction
{
    string Name { get; }
    KeyCode Key { get; }
    Vector3 Position { get; }

    void OnInteract();
}

public class TriggerButton : MonoBehaviour, IInteraction
{
    [SerializeField] string triggerName;
    [SerializeField] KeyCode shortcutKey = KeyCode.G;
    [SerializeField] UnityEvent onAction;

    string IInteraction.Name => triggerName;
    KeyCode IInteraction.Key => shortcutKey;
    Vector3 IInteraction.Position => transform.position;

    [ContextMenu("¿€µø")]
    void IInteraction.OnInteract()
    {
        onAction?.Invoke();

    }
}
