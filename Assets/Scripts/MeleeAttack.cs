using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : Attackable
{
    public override void Attack(Damageable target)
    {
        target.OnDamageable(status);
    }
}
