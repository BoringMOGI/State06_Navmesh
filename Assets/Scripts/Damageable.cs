using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Stateable))]
public class Damageable : MonoBehaviour
{
    private Stateable status;

    private void Start()
    {
        status = GetComponent<Stateable>();
    }


    public void OnDamageable(Stateable attacker)
    {
        if (!status.IsAlive)
            return;

        // �������� �ް� �׾��ٸ� �״� �̺�Ʈ ����.
        if(!status.OnDamage(attacker.power))
        {
            OnDead();
        }
    }
    private void OnDead()
    {
        Debug.Log("�׾���...");
    }
}
