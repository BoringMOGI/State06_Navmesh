using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class PRoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] string nickName;
    [SerializeField] string roomName;
    [SerializeField] string gameVersion;

    void Start()
    {
        PhotonNetwork.NickName = nickName;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { }, TypedLobby.Default);
        }
    }
    public override void OnCreatedRoom()
    {
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate("Room/RoomPlayer", Vector3.zero, Quaternion.identity);
    }

    [ContextMenu("Next")]
    public void Next()
    {
        PhotonNetwork.LoadLevel("Test2");
    }
}
