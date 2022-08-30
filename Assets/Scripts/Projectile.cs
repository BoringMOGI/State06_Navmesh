using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Stateable attacker;        // ������.
    private Damageable target;         // �ǰ���.
    private float moveSpeed;           // ����ü �̵� �ӵ�.

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

        // ������ �����ǰ� �� �������� ��ġ���� ���� ���.
        while(true)
        {
            Vector3 destination = target.Position;
            float movement = moveSpeed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, destination, movement);
            if (transform.position == target.Position)
                break;

            yield return null;
        }

        // ���濡�� �������� �������ͽ��� ����.
        target.OnDamaged(attacker);
    }
}
