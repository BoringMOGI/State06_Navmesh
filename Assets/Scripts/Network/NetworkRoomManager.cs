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

    [ContextMenu("유저 입장")]
    public void EnterUser()
    {
        userCount++;
        UpdateUserList();
    }
    [ContextMenu("유저 퇴장")]
    public void LeaveUser()
    {
        userCount--;
        UpdateUserList();
    }
    private void UpdateUserList()
    {
        // 기존 유저 목록 UI 삭제.
        foreach (Transform child in userParent)
            Destroy(child.gameObject);

        // (현재는) userCount수만큼 프리팹 생성.
        for (int i = 0; i < userCount; i++)
            Instantiate(userPrefab, userParent);
    }

}
