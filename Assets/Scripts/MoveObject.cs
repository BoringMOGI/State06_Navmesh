using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [SerializeField] Transform destinationPivot;        // 목적지 지점.
    [SerializeField] float moveSpeed;                   // 이동 속도.

    Vector3 originPos;      // 원점.
    Vector3 destination;    // 목표점.
    bool isOrigin;          // 원점에 있어야하는가?

    private void Start()
    {
        originPos = transform.position;
        destination = destinationPivot.position;
        isOrigin = true;
    }

    private void Update()
    {
        Vector3 point = isOrigin ? originPos : destination;
        transform.position = Vector3.MoveTowards(transform.position, point, moveSpeed * Time.deltaTime);
    }

    public void OnSwitch()
    {
        isOrigin = !isOrigin;
    }
}
