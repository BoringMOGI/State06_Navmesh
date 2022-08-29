using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Stateable))]
public abstract class Attackable : MonoBehaviour
{
    protected Stateable status;

    protected void Start()
    {
        status = GetComponent<Stateable>();
    }
    public abstract void Attack(Damageable target);
}
