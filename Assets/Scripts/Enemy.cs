using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    private enum STATE
    {
        Stay,       // ��� : ���ڸ����� ��ٸ�.
        Patrol,     // ���� : �ֺ��� ���ƴٴѴ�.
        Chase,      // ���� : ����� ���ݹ����� �������� ���󰣴�.
        Attack,     // ���� : ����� �����Ѵ�.
    }

    [SerializeField] Transform target;      // �÷��̾�(=Ÿ��)
    [SerializeField] float stayTime;        // ��� �ð�.

    [Header("Range")]
    [SerializeField] float patrolRange;     // ���� ����.
    [SerializeField] float detectionRange;  // Ž�� ����.
    [SerializeField] float attackRange;     // ���� ����.

    private NavMeshAgent agent;

    private STATE state;    // ���� ����.
    private float timer;    // ��� �ð� Ÿ�̸�.

    private Vector3 birthPoint;     // ź�� ����(=���� ��ġ)
    private Vector3 patrolPoint;    // ���� ����.

    private int groundLayerMask;    // ���� ���̾� ����ũ.
    private int playerLayerMask;    // �÷��̾� ���̾� ����ũ.

    private bool isInDetectRange;   // Ž�� ������ �÷��̾ ���Դ°�?
    private bool isInAttackRange;   // ���� ������ �÷��̾ ���Դ°�?

    void Start()
    {
        state = STATE.Stay;
        timer = 0f;

        birthPoint = transform.position;
        agent = GetComponent<NavMeshAgent>();

        // ��Ʈ �÷����̱� ������ ����Ʈ �������� ����Ѵ�.
        playerLayerMask = 1 << LayerMask.NameToLayer("Player");
        groundLayerMask = 1 << LayerMask.NameToLayer("Ground");
    }

    void Update()
    {
        // Ž��, ���� ������ �÷��̾ ���Դ��� üũ.
        isInDetectRange = Physics.CheckSphere(transform.position, detectionRange);
        isInAttackRange = Physics.CheckSphere(transform.position, attackRange);

        switch(state)
        {
            case STATE.Stay:
                OnStay();
                break;

            case STATE.Patrol:
                OnPatrol();
                break;

            case STATE.Chase:
                OnChase();
                break;

            case STATE.Attack:
                OnAttack();
                break;
        }
    }

    private void OnStay()
    {
        // ���� ����߿� �÷��̾ Ž�� ������ ���Դٸ�
        // ���¸� �����ܰ�� �����Ѵ�.
        if(isInDetectRange)
        {
            state = STATE.Chase;
            return;
        }

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
                patrolPoint = hit.point;
                state = STATE.Patrol;
            }
        }
    }
    private void OnPatrol()
    {
        // ���� �����߿� �÷��̾ Ž�� ������ ���Դٸ�
        // ���¸� �����ܰ�� �����Ѵ�.
        if (isInDetectRange)
        {
            state = STATE.Chase;
            return;
        }

        agent.SetDestination(patrolPoint);                          // ������ ����.
        if (agent.hasPath && agent.remainingDistance <= 0.1f)       // �������� �ְ�, �����Ÿ��� 0.1�����϶�.
        {
            // �����ߴ�.
            state = STATE.Stay;
        }

    }   
    private void OnChase()
    {
        // ���� �� ���� ������ ���´ٸ� ���� ���·� ����.
        if (isInAttackRange)
        {
            state = STATE.Attack;
            return;
        }

        agent.SetDestination(target.position);
    }
    private void OnAttack()
    {
        
    }

#if UNITY_EDITOR
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
