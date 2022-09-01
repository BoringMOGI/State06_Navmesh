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
    [SerializeField] ParticleSystem fxPoint;        // Ŭ�� ���� ����Ʈ.
    [SerializeField] ParticleSystem fxQuestion;

    NavMeshAgent agent;
    Camera cam;
    IInteraction interactor;        // ��ȣ�ۿ���.

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

        // �˻��� �ݶ��̴� �迭���� �������̽��� ������ ��ü ��
        // ���� �Ÿ��� ª�� ����� ã�´�.
        var handle = colliders.
            Select(c => c.GetComponent<IInteraction>()).
            Where(c => c != null).
            OrderBy(c => Vector3.Distance(c.Position, transform.position));

        // �˻� ��� ���ͷ��Ͱ� ���� ��� �����Ѵ�.
        if (handle.Count() <= 0)
        {
            InteractUI.Instance.CloseUI();
            return;
        }

        // �Ÿ� ���� ���� 0��°�� ���� ����� ��ȣ�ۿ��ڴ�.
        IInteraction interactor = handle.ToArray()[0];

        // UI���
        InteractUI.Instance.UpdateUI(interactor);

        // ��ȣ�ۿ��ڰ� ���ϴ� Ű�� �����ٸ� �۵��� ��Ų��.
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
            // ���콺 ��ġ�� ��ũ���� ���̷� �����.
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // ���� �����̽��� ���콺�� ��ġ�κ��� ī�޶��� ���� �������� ���̸� �߻��Ѵ�.
            // ���̿� �浹�� ������ agent�� ������ ��������.
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
