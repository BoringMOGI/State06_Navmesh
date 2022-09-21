using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomUser : MonoBehaviour
{
    [SerializeField] PhotonView pv;
    [SerializeField] int number;

    private void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    [ContextMenu("��ư")]
    public void Button()
    {
        if(pv.IsMine)
            pv.RPC(nameof(ChangeNumber), RpcTarget.All, number);
    }
    
    [PunRPC]
    public void ChangeNumber(int num)
    {
        Debug.Log("�̸� ���� : " + num);
        gameObject.name = num.ToString();
    }


}
