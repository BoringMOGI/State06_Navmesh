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
            nameText.text = $"{user.NickName}(����)";
        else
            nameText.text = user.NickName;
    }
    public void UpdateInfo()
    {
        // ������Ʈ�� ��Ű�� �ڵ����� OnPhotonSerializeView�� ȣ��Ǿ 
        // ���� Ŭ�е��� ���� ����ȭ�Ѵ�.
    }
    public void Ready(bool isReady)
    {
        this.isReady = isReady;
    }
  
    // ���� �ð����� ���� ����ȭ ��Ų��.
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // �۽�.
        if (stream.IsWriting)
        {
            stream.SendNext(indexText.text);
            stream.SendNext(nameText.text);
            stream.SendNext(ready.activeSelf);
            stream.SendNext(isReady);
            stream.SendNext(user);
        }
        // ����.
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
