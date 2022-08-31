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

    NavMeshObstacle obstacle;   // 장애물 콜라이더.    
    bool isMoving;              // 문이 열리거나 닫히는 중인가?
    bool isOpen;                // 열려있는가?

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

        isOpen = !isOpen;   // 문의 상태를 변견한다.
    }

    private void OnOpen()
    {
        // 문이 내려가는 애니메이션을 재생한다.
        anim.Play(openAnimName);              

        // 코루틴으로 애니메이션의 끝을 체크한다.
        // 문이 다 내려가면 장애물을 끈다.
        StartCoroutine(EndAnimation(() => {
            obstacle.enabled = false;
            isMoving = false;
        }));     
    }
    private void OnClose()
    {
        obstacle.enabled = true;            // 즉시 장애물 콜라이더를 켠다.
        anim.Play(closeAnimName);           // 문이 올라오는 애니메이션을 재생한다.

        // 코루틴으로 애니메이션이 끝남을 체크한다.
        StartCoroutine(EndAnimation(() => {
            isMoving = false;
        }));     
    }
    IEnumerator EndAnimation(System.Action onAction)
    {
        while (anim.isPlaying)      // 애니메이션이 재생중이면..
            yield return null;      // 기다린다.

        onAction?.Invoke();
    }

}
