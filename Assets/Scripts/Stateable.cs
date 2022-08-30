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

    [SerializeField] string name;      // �̸�.
    [SerializeField] int level;
    [SerializeField] Status basic;     // �⺻ �������ͽ�.
    [SerializeField] Status grow;      // ���� �������ͽ�.

    public string Name => name;
    public float Level => level;
    public float hp { get; private set; }
    public bool IsAlive => hp > 0f;

    // �������ͽ�.
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
            return basic.defence + (grow.defence * level);
        }
    }
    public float resistance
    {
        get
        {
            return basic.resistance + (grow.resistance * level);
        }
    }
    public float moveSpeed
    {
        get
        {
            return basic.moveSpeed + (grow.moveSpeed * level);
        }
    }
    public float attackRange
    {
        get
        {
            return basic.attackRange + (grow.attackRange * level);
        }
    }


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

        // �ִ� ü�� ������ ���� HP ����.
        hp += (maxHp - beforeMaxHp);
    }
    public void Increase(float amount)
    {
        hp = Mathf.Clamp(hp + amount, 0f, maxHp);
    }
    public void Decrease(float amount)
    {
        hp = Mathf.Clamp(hp - amount, 0f, maxHp);
    }


}