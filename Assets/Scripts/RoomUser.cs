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

    [ContextMenu("버튼")]
    public void Button()
    {
        if(pv.IsMine)
            pv.RPC(nameof(ChangeNumber), RpcTarget.All, number);
    }
    
    [PunRPC]
    public void ChangeNumber(int num)
    {
        Debug.Log("이름 변경 : " + num);
        gameObject.name = num.ToString();
    }


}
