using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class Player : MonoBehaviour
{
    [SerializeField] LayerMask groundMask;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] ParticleSystem fxPoint;        // 클릭 지점 이펙트.
    [SerializeField] ParticleSystem fxQuestion;

    NavMeshAgent agent;
    Camera cam;
    IInteraction interactor;        // 상호작용자.

    void Start()
    {
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        UserInput();
        OnUpdateInteract();

        Vector3[] cornoers = agent.path.corners;
        lineRenderer.positionCount = cornoers.Length;
        lineRenderer.SetPositions(cornoers);
    }

    private void OnUpdateInteract()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 3f);
        if (colliders.Length <= 0)
        {
            InteractUI.Instance.CloseUI();
            return;
        }

        // 검색된 콜라이더 배열에서 인터페이스를 구현한 객체 중
        // 가장 거리가 짧은 대상을 찾는다.
        var handle = colliders.
            Select(c => c.GetComponent<IInteraction>()).
            Where(c => c != null).
            OrderBy(c => Vector3.Distance(c.Position, transform.position));

        // 검색 결과 인터렉터가 없을 경우 리턴한다.
        if (handle.Count() <= 0)
        {
            InteractUI.Instance.CloseUI();
            return;
        }

        // 거리 정렬 기준 0번째가 가장 가까운 상호작용자다.
        IInteraction interactor = handle.ToArray()[0];

        // UI출력
        InteractUI.Instance.UpdateUI(interactor);

        // 상호작용자가 원하는 키를 눌렀다면 작동을 시킨다.
        if(Input.GetKeyDown(interactor.Key))
        {
            interactor.OnInteract();
        }
    }
    private void UserInput()
    {
        bool isDownMouse = Input.GetMouseButtonDown(1);
        bool isDownQuestion = Input.GetKeyDown(KeyCode.F2);

        if (isDownMouse || isDownQuestion)
        {
            // 마우스 위치를 스크린상 레이로 만든다.
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // 월드 스페이스상 마우스의 위치로부터 카메라의 정면 방향으로 레이를 발사한다.
            // 레이에 충돌된 지점이 agent가 가야할 목적지다.
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, groundMask))
            {
                Vector3 destination = hit.point;

                if (isDownMouse)
                {
                    Instantiate(fxPoint, destination, Quaternion.identity);
                    agent.SetDestination(destination);
                }
                else if(isDownQuestion)
                {
                    Instantiate(fxQuestion, destination, Quaternion.identity);
                }
            }
        }
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle() { fontSize = 30 };
        GUI.Label(new Rect(0, 0, 500, 50), agent.path.status.ToString(), style);
    }
}
