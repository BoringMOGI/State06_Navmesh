using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Player : MonoBehaviour
{
    [SerializeField] LayerMask groundMask;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] ParticleSystem fxPoint;        // 클릭 지점 이펙트.
    [SerializeField] ParticleSystem fxQuestion;

    NavMeshAgent agent;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        UserInput();

        Vector3[] cornoers = agent.path.corners;
        lineRenderer.positionCount = cornoers.Length;
        lineRenderer.SetPositions(cornoers);
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
