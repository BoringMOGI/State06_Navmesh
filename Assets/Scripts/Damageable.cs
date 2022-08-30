using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Stateable))]
public class Damageable : MonoBehaviour
{
    [SerializeField] Transform damagePivot;

    public Vector3 Position => damagePivot.position;
    private Stateable status;

    private void Start()
    {
        status = GetComponent<Stateable>();
    }

    public void OnDamaged(Stateable attacker)
    {
        if (!status.IsAlive)
            return;

        float finalDamage = Mathf.Clamp(attacker.power - (status.defence * 0.5f), 1, 9999);
        status.Decrease(finalDamage);

        Debug.Log("데미지를 받았다 : " + finalDamage);

        if (!status.IsAlive)
            OnDead();
    }
    private void OnDead()
    {
        Debug.Log("죽었다...");
    }
}
