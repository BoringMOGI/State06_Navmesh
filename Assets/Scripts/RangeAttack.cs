using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeAttack : Attackable
{
    [SerializeField] Transform muzzle;      // 총구(=투사체 발사 지점)
    [SerializeField] Projectile prefab;     // 투사체 프리팹.
    [SerializeField] float projectileSpeed; // 투사체 이동 속도. 

    public override void Attack(Damageable target)
    {
        Projectile bullet = Instantiate(prefab, muzzle.position, muzzle.rotation);
        bullet.Shoot(status, target, projectileSpeed);
    }
}
