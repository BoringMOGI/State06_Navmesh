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
        Stay,       // 대기 : 제자리에서 기다림.
        Patrol,     // 정찰 : 주변을 돌아다닌다.
        Chase,      // 추적 : 대상이 공격범위에 들어갈때까지 따라간다.
        Attack,     // 공격 : 대상을 공격한다.
    }

    [SerializeField] Transform target;      // 플레이어(=타겟)
    [SerializeField] float stayTime;        // 대기 시간.

    [Header("Range")]
    [SerializeField] float patrolRange;     // 정찰 범위.
    [SerializeField] float detectionRange;  // 탐지 범위.
    [SerializeField] float attackRange;     // 공격 범위.

    private NavMeshAgent agent;

    private STATE state;    // 현재 상태.
    private float timer;    // 대기 시간 타이머.

    private Vector3 birthPoint;     // 탄생 지점(=원점 위치)
    private Vector3 patrolPoint;    // 정찰 지점.

    private int groundLayerMask;    // 지면 레이어 마스크.
    private int playerLayerMask;    // 플레이어 레이어 마스크.

    private bool isInDetectRange;   // 탐지 범위에 플레이어가 들어왔는가?
    private bool isInAttackRange;   // 공격 범위에 플레이어가 들어왔는가?

    void Start()
    {
        state = STATE.Stay;
        timer = 0f;

        birthPoint = transform.position;
        agent = GetComponent<NavMeshAgent>();

        // 비트 플레그이기 때문에 쉬프트 연산으로 계산한다.
        playerLayerMask = 1 << LayerMask.NameToLayer("Player");
        groundLayerMask = 1 << LayerMask.NameToLayer("Ground");
    }

    void Update()
    {
        // 탐지, 공격 범위에 플레이어가 들어왔는지 체크.
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
        // 만약 대기중에 플레이어가 탐지 범위에 들어왔다면
        // 상태를 추적단계로 변경한다.
        if(isInDetectRange)
        {
            state = STATE.Chase;
            return;
        }

        timer += Time.deltaTime;
        if(timer >= stayTime)
        {
            // stayTime만큼 대기를 완료했다. 탐색을 진행한다.
            timer = 0f;

            // 랜덤 원의 범위 안에서 목적지 계산.
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
        // 만약 정찰중에 플레이어가 탐지 범위에 들어왔다면
        // 상태를 추적단계로 변경한다.
        if (isInDetectRange)
        {
            state = STATE.Chase;
            return;
        }

        agent.SetDestination(patrolPoint);                          // 목적지 설정.
        if (agent.hasPath && agent.remainingDistance <= 0.1f)       // 목적지가 있고, 남은거리가 0.1이하일때.
        {
            // 도착했다.
            state = STATE.Stay;
        }

    }   
    private void OnChase()
    {
        // 추적 중 공격 범위에 들어온다면 공격 상태로 변경.
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

        // 공격과 탐지 범위.        
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, Vector3.up, detectionRange);

        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, attackRange);

        Gizmos.DrawSphere(patrolPoint, 0.1f);
    }
#endif

}
