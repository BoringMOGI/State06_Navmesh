using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Stateable attacker;        // 공격자.
    private Damageable target;         // 피격자.
    private float moveSpeed;           // 투사체 이동 속도.

    public void Shoot(Stateable attacker, Damageable target, float moveSpeed)
    {
        this.attacker = attacker;
        this.target = target;
        this.moveSpeed = moveSpeed;

        StartCoroutine(Movement());
    }

    IEnumerator Movement()
    {
        Transform transform = base.transform;

        // 상대방의 포지션과 내 포지션이 일치하지 않을 경우.
        while(true)
        {
            Vector3 destination = target.Position;
            float movement = moveSpeed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, destination, movement);
            if (transform.position == target.Position)
                break;

            yield return null;
        }

        // 상대방에게 공격자의 스테이터스를 전달.
        target.OnDamaged(attacker);
    }
}
