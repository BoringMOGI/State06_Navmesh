using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stateable : MonoBehaviour
{
    [System.Serializable]
    public struct Status
    {
        public float hp;
        public float power;
        public float attackRate;
        public float defence;
        public float resistance;
        public float moveSpeed;
        public float attackRange;
    }

    [SerializeField] string name;      // 이름.
    [SerializeField] int level;
    [SerializeField] Status basic;     // 기본 스테이터스.
    [SerializeField] Status grow;      // 성장 스테이터스.

    public float Level => level;
    public float hp { get; private set; }
    public float maxHp
    {
        get
        {
            return basic.hp + (grow.hp * level);
        }
    }
    public float power
    {
        get
        {
            return basic.power + (grow.power * level);
        }
    }
    public float attackRate
    {
        get
        {
            return basic.attackRate + (grow.attackRate * level);
        }
    }
    public float defence
    {
        get
        {
            return basic.attackRate + (grow.attackRate * level);
        }
    }
    public float resistance
    {
        get
        {
            return basic.attackRate + (grow.attackRate * level);
        }
    }
    public float moveSpeed
    {
        get
        {
            return basic.attackRate + (grow.attackRate * level);
        }
    }
    public float attackRange
    {
        get
        {
            return basic.attackRate + (grow.attackRate * level);
        }
    }

    public bool IsAlive => hp > 0f;

    private void Start()
    {
        hp = maxHp;
    }

    public void LevelUp()
    {
        if (level >= 18)
            return;

        float beforeMaxHp = maxHp;
        level += 1;

        // 최대 체력 증가에 따른 HP 증가.
        hp += (maxHp - beforeMaxHp);
    }
    public bool OnDamage(float amount)
    {
        hp = Mathf.Clamp(hp - amount, 0f, maxHp);
        return IsAlive;
    }


}
