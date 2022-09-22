using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

using User = Photon.Realtime.Player;

public class RoomUserUI : MonoBehaviour, IPunObservable
{
    [SerializeField] TMP_Text indexText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] GameObject ready;

    PhotonView pv;
    User user;
    bool isReady;

    public int ActorNumber => user.ActorNumber;
    public string NickName => user.NickName;
    public bool IsMine => pv.IsMine;

    public void Setup(User user)
    {
        pv = GetComponent<PhotonView>();

        indexText.text = user.ActorNumber.ToString("00");
        ready.SetActive(false);
        this.user = user;

        if (user.IsMasterClient)
            nameText.text = $"{user.NickName}(방장)";
        else
            nameText.text = user.NickName;
    }
    public void UpdateInfo()
    {
        // 업데이트를 시키면 자동으로 OnPhotonSerializeView가 호출되어서 
        // 나의 클론들이 값을 동기화한다.
    }
    public void Ready(bool isReady)
    {
        this.isReady = isReady;
    }
  
    // 일정 시간마다 값을 동기화 시킨다.
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 송신.
        if (stream.IsWriting)
        {
            stream.SendNext(indexText.text);
            stream.SendNext(nameText.text);
            stream.SendNext(ready.activeSelf);
            stream.SendNext(isReady);
            stream.SendNext(user);
        }
        // 수신.
        if(stream.IsReading)
        {
            indexText.text = (string)stream.ReceiveNext();
            nameText.text = (string)stream.ReceiveNext();
            ready.SetActive((bool)stream.ReceiveNext());
            isReady = (bool)stream.ReceiveNext();
            user = (User)stream.ReceiveNext();
        }
    }
}
