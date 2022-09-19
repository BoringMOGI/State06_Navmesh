using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkRoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject userPrefab;
    [SerializeField] Transform userParent;

    int userCount = 0;

    private void Start()
    {
        UpdateUserList();
    }

    [ContextMenu("���� ����")]
    public void EnterUser()
    {
        userCount++;
        UpdateUserList();
    }
    [ContextMenu("���� ����")]
    public void LeaveUser()
    {
        userCount--;
        UpdateUserList();
    }
    private void UpdateUserList()
    {
        // ���� ���� ��� UI ����.
        foreach (Transform child in userParent)
            Destroy(child.gameObject);

        // (�����) userCount����ŭ ������ ����.
        for (int i = 0; i < userCount; i++)
            Instantiate(userPrefab, userParent);
    }

}
