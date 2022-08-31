using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [SerializeField] Transform destinationPivot;        // ������ ����.
    [SerializeField] float moveSpeed;                   // �̵� �ӵ�.

    Vector3 originPos;      // ����.
    Vector3 destination;    // ��ǥ��.
    bool isOrigin;          // ������ �־���ϴ°�?

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
