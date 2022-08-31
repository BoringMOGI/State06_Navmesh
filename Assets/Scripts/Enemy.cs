using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Stateable))]
public class Enemy : MonoBehaviour
{
    private enum STATE
    {
        Stay,       // ��� : ���ڸ����� ��ٸ�.
        Patrol,     // ���� : �ֺ��� ���ƴٴѴ�.
        Chase,      // ���� : ����� ���ݹ����� �������� ���󰣴�.
        Attack,     // ���� : ����� �����Ѵ�.
    }

    [SerializeField] Damageable target;      // �÷��̾�(=Ÿ��)
    [SerializeField] float stayTime;        // ��� �ð�.

    [Header("Range")]
    [SerializeField] float patrolRange;     // ���� ����.
    [SerializeField] float detectionRange;  // Ž�� ����.
    [SerializeField] float attackRange;     // ���� ����.

    private NavMeshAgent agent;
    private Stateable status;
    private Attackable attackable;
    private Animator anim;

    private STATE state;            // ���� ����.
    private float timer;            // ��� �ð� Ÿ�̸�.
    private float nextAttackTime;   // ���� ���� ���� �ð�.

    private Vector3 birthPoint;     // ź�� ����(=���� ��ġ)
    private Vector3 patrolPoint;    // ���� ����.

    private int groundLayerMask;    // ���� ���̾� ����ũ.
    private int playerLayerMask;    // �÷��̾� ���̾� ����ũ.

    private bool isSetPatrolPoint;  // ���� ������ �غ� �Ǿ��°�?
    private bool isInDetectRange;   // Ž�� ������ �÷��̾ ���Դ°�?
    private bool isInAttackRange;   // ���� ������ �÷��̾ ���Դ°�?

    void Start()
    {
        state = STATE.Stay;
        timer = 0f;

        birthPoint = transform.position;

        agent = GetComponent<NavMeshAgent>();
        status = GetComponent<Stateable>();
        attackable = GetComponent<Attackable>();    // �ٰŸ� or ���Ÿ�.
        anim = GetComponent<Animator>();

        // �������ͽ��� �ִ� ���� �����Ѵ�.
        attackRange = status.attackRange;
        agent.speed = status.moveSpeed;

        Debug.Log(status.moveSpeed);

        // ��Ʈ �÷����̱� ������ ����Ʈ �������� ����Ѵ�.
        playerLayerMask = 1 << LayerMask.NameToLayer("Player");
        groundLayerMask = 1 << LayerMask.NameToLayer("Ground");
    }

    void Update()
    {
        // Ž��, ���� ������ �÷��̾ ���Դ��� üũ.
        isInDetectRange = Physics.CheckSphere(transform.position, detectionRange, playerLayerMask);
        isInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayerMask);

        // ����. (Ž�� �������� ���Դµ� ���� ������ ������ �ʾҴ�.
        if(isInDetectRange && !isInAttackRange)
        {
            OnChase();
        }
        // ����. (Ž�� �������� ���Ӱ� ���� �������� ���Դ�.)
        else if(isInDetectRange && isInAttackRange)
        {
            OnAttack();
        }
        // ����.
        else if(isSetPatrolPoint)
        {
            OnPatrol();
        }
        else
        {
            OnStay();
        }

        anim.SetBool("isMove", state == STATE.Patrol || state == STATE.Chase);
    }

    private void OnStay()
    {
        state = STATE.Stay;

        timer += Time.deltaTime;
        if(timer >= stayTime)
        {
            // stayTime��ŭ ��⸦ �Ϸ��ߴ�. Ž���� �����Ѵ�.
            timer = 0f;

            // ���� ���� ���� �ȿ��� ������ ���.
            Vector2 insideUnit = Random.insideUnitCircle;
            Vector3 point = birthPoint + (new Vector3(insideUnit.x, 0f, insideUnit.y) * patrolRange);
            point += Vector3.up * 10f;

            RaycastHit hit;
            if(Physics.Raycast(point, Vector3.down, out hit, float.MaxValue, groundLayerMask))
            {
                patrolPoint = hit.point;        // ���� ����Ʈ ����.
                isSetPatrolPoint = true;        // ���� ������ �����ߴ�.
            }
        }
    }
    private void OnPatrol()
    {
        state = STATE.Patrol;

        agent.SetDestination(patrolPoint);                          // ������ ����.
        if (agent.hasPath && agent.remainingDistance <= 0.1f)       // �������� �ְ�, �����Ÿ��� 0.1�����϶�.
        {
            // �����ߴ�.
            isSetPatrolPoint = false;
        }

    }   
    private void OnChase()
    {
        state = STATE.Chase;
        agent.SetDestination(target.Position);
    }
    private void OnAttack()
    {
        state = STATE.Attack;
        agent.SetDestination(transform.position);

        if(nextAttackTime <= Time.time)
        {
            nextAttackTime = Time.time + status.attackRate;
            anim.SetTrigger("onAttack");
            attackable.Attack(target);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.white;
        GUIStyle style = new GUIStyle() { fontSize = 20};
        Handles.Label(transform.position, state.ToString());
    }

    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(birthPoint, Vector3.up, patrolRange);

        // ���ݰ� Ž�� ����.        
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, Vector3.up, detectionRange);

        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, attackRange);

        Gizmos.DrawSphere(patrolPoint, 0.1f);
    }
#endif

}
