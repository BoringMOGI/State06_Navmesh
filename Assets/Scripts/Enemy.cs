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
        Stay,       // 대기 : 제자리에서 기다림.
        Patrol,     // 정찰 : 주변을 돌아다닌다.
        Chase,      // 추적 : 대상이 공격범위에 들어갈때까지 따라간다.
        Attack,     // 공격 : 대상을 공격한다.
    }

    [SerializeField] Damageable target;      // 플레이어(=타겟)
    [SerializeField] float stayTime;        // 대기 시간.

    [Header("Range")]
    [SerializeField] float patrolRange;     // 정찰 범위.
    [SerializeField] float detectionRange;  // 탐지 범위.
    [SerializeField] float attackRange;     // 공격 범위.

    private NavMeshAgent agent;
    private Stateable status;
    private Attackable attackable;
    private Animator anim;

    private STATE state;            // 현재 상태.
    private float timer;            // 대기 시간 타이머.
    private float nextAttackTime;   // 다음 공격 가능 시간.

    private Vector3 birthPoint;     // 탄생 지점(=원점 위치)
    private Vector3 patrolPoint;    // 정찰 지점.

    private int groundLayerMask;    // 지면 레이어 마스크.
    private int playerLayerMask;    // 플레이어 레이어 마스크.

    private bool isSetPatrolPoint;  // 정찰 지점이 준비가 되었는가?
    private bool isInDetectRange;   // 탐지 범위에 플레이어가 들어왔는가?
    private bool isInAttackRange;   // 공격 범위에 플레이어가 들어왔는가?

    void Start()
    {
        state = STATE.Stay;
        timer = 0f;

        birthPoint = transform.position;

        agent = GetComponent<NavMeshAgent>();
        status = GetComponent<Stateable>();
        attackable = GetComponent<Attackable>();    // 근거리 or 원거리.
        anim = GetComponent<Animator>();

        // 스테이터스에 있는 값을 대입한다.
        attackRange = status.attackRange;
        agent.speed = status.moveSpeed;

        Debug.Log(status.moveSpeed);

        // 비트 플레그이기 때문에 쉬프트 연산으로 계산한다.
        playerLayerMask = 1 << LayerMask.NameToLayer("Player");
        groundLayerMask = 1 << LayerMask.NameToLayer("Ground");
    }

    void Update()
    {
        // 탐지, 공격 범위에 플레이어가 들어왔는지 체크.
        isInDetectRange = Physics.CheckSphere(transform.position, detectionRange, playerLayerMask);
        isInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayerMask);

        // 추적. (탐지 범위에는 들어왔는데 공격 범위에 들어오지 않았다.
        if(isInDetectRange && !isInAttackRange)
        {
            OnChase();
        }
        // 공격. (탐지 범위에도 들어왓고 공격 범위에도 들어왔다.)
        else if(isInDetectRange && isInAttackRange)
        {
            OnAttack();
        }
        // 추적.
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
            // stayTime만큼 대기를 완료했다. 탐색을 진행한다.
            timer = 0f;

            // 랜덤 원의 범위 안에서 목적지 계산.
            Vector2 insideUnit = Random.insideUnitCircle;
            Vector3 point = birthPoint + (new Vector3(insideUnit.x, 0f, insideUnit.y) * patrolRange);
            point += Vector3.up * 10f;

            RaycastHit hit;
            if(Physics.Raycast(point, Vector3.down, out hit, float.MaxValue, groundLayerMask))
            {
                patrolPoint = hit.point;        // 정찰 포인트 대입.
                isSetPatrolPoint = true;        // 정찰 지점을 지정했다.
            }
        }
    }
    private void OnPatrol()
    {
        state = STATE.Patrol;

        agent.SetDestination(patrolPoint);                          // 목적지 설정.
        if (agent.hasPath && agent.remainingDistance <= 0.1f)       // 목적지가 있고, 남은거리가 0.1이하일때.
        {
            // 도착했다.
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

        // 공격과 탐지 범위.        
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, Vector3.up, detectionRange);

        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, attackRange);

        Gizmos.DrawSphere(patrolPoint, 0.1f);
    }
#endif

}
