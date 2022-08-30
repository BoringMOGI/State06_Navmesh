using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeAttack : Attackable
{
    [SerializeField] Transform muzzle;      // �ѱ�(=����ü �߻� ����)
    [SerializeField] Projectile prefab;     // ����ü ������.
    [SerializeField] float projectileSpeed; // ����ü �̵� �ӵ�. 

    public override void Attack(Damageable target)
    {
        Projectile bullet = Instantiate(prefab, muzzle.position, muzzle.rotation);
        bullet.Shoot(status, target, projectileSpeed);
    }
}
