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

        // 데미지를 받고 죽었다면 죽는 이벤트 실행.
        if(!status.OnDamage(attacker.power))
        {
            OnDead();
        }
    }
    private void OnDead()
    {
        Debug.Log("죽었다...");
    }
}
