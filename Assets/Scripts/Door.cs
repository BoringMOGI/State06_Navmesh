using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class Door : MonoBehaviour
{
    [SerializeField] Animation anim;      
    [SerializeField] string openAnimName;
    [SerializeField] string closeAnimName;

    NavMeshObstacle obstacle;   // ��ֹ� �ݶ��̴�.    
    bool isMoving;              // ���� �����ų� ������ ���ΰ�?
    bool isOpen;                // �����ִ°�?

    private void Start()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        isMoving = false;
        isOpen = false;
    }
    public void OnSwitchDoor()
    {
        if (isMoving)
            return;

        isMoving = true;        

        if (isOpen)
            OnClose();
        else
            OnOpen();

        isOpen = !isOpen;   // ���� ���¸� �����Ѵ�.
    }

    private void OnOpen()
    {
        // ���� �������� �ִϸ��̼��� ����Ѵ�.
        anim.Play(openAnimName);              

        // �ڷ�ƾ���� �ִϸ��̼��� ���� üũ�Ѵ�.
        // ���� �� �������� ��ֹ��� ����.
        StartCoroutine(EndAnimation(() => {
            obstacle.enabled = false;
            isMoving = false;
        }));     
    }
    private void OnClose()
    {
        obstacle.enabled = true;            // ��� ��ֹ� �ݶ��̴��� �Ҵ�.
        anim.Play(closeAnimName);           // ���� �ö���� �ִϸ��̼��� ����Ѵ�.

        // �ڷ�ƾ���� �ִϸ��̼��� ������ üũ�Ѵ�.
        StartCoroutine(EndAnimation(() => {
            isMoving = false;
        }));     
    }
    IEnumerator EndAnimation(System.Action onAction)
    {
        while (anim.isPlaying)      // �ִϸ��̼��� ������̸�..
            yield return null;      // ��ٸ���.

        onAction?.Invoke();
    }

}
