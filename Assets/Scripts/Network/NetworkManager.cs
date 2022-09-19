using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField nameField;
    [SerializeField] TMP_InputField roomField;
    [SerializeField] Toggle remember;

    private string roomName = "Local";
    private string gameVersion = "1.0";

    private void Awake()
    {
        // 마스터 클라이언트의 요청으로 새로운 씬이 로드될 때
        // 같은 방의 모든 클라이언트가 자동으로 레벨을 동기화한다.
        // MasterClinet는 PhotonNetwork.LoadLevel("")로 호출할 수 있다.
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
        SaveOrLoad(false);                          // 데이터 로드.
        PhotonNetwork.GameVersion = gameVersion;    // 게임 버전 (버전 간 간섭이 없다)
        PhotonNetwork.ConnectUsingSettings();       // 마스터 서버 접속 시도.
    }

    private void SaveOrLoad(bool isSave)
    {
        // 데이터 세이브.
        if(isSave)
        {
            if(remember.isOn)
            {
                PlayerPrefs.SetInt("remember", 1);
                PlayerPrefs.SetString("userName", nameField.text);
                PlayerPrefs.SetString("roomName", roomField.text);
            }
            else
            {
                PlayerPrefs.DeleteKey("remember");
                PlayerPrefs.DeleteKey("userName");
                PlayerPrefs.DeleteKey("roomName");
            }
        }
        // 데이터 로드.
        else
        {
            // 시작시 유저의 입력 정보를 저장했는지 체크.
            if (PlayerPrefs.HasKey("remember"))
            {
                remember.isOn = true;
                nameField.text = PlayerPrefs.GetString("userName");
                roomField.text = PlayerPrefs.GetString("roomName");
            }
            else
            {
                remember.isOn = false;
                nameField.text = string.Empty;
                roomField.text = string.Empty;
            }
        }
    }

    public void Connect()
    {
        if(!PhotonNetwork.IsConnected)
        {
            Debug.Log("포톤 네트워크와 연결되어있지 않습니다.");
            return;
        }

        if(string.IsNullOrEmpty(nameField.text))
        {
            Debug.Log("이름이 입력되지 않았습니다.");
            return;
        }

        SaveOrLoad(true);       // 데이터 세이브.

        // null과 ""는 다르다.
        string roomName = roomField.text.Equals(string.Empty) ? null : roomField.text;

        // 만약 방 이름을 null로 하면 랜덤한 방을 만들라는 의미다.
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 10 });

        // CreateRoom               : 방 생성
        // JoinRoom                 : 방 입장
        // JoinOrCreateRoom         : 방 입장 시도, 실패 시 방 생성.
        // JoinRandomOrCreateRoom   : 랜덤 방 입장 시도, 실패 시 방 생성.
    }


    // Network evnets...
    public override void OnConnectedToMaster()
    {
        // 마스터 서버에 접속,해제.
        if (PhotonNetwork.IsConnected)
            Debug.Log("마스터 서버 접속 성공");
    }

    // 방 생성 (Create)
    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성 완료!!");
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log($"방 생성 실패 [CODE:{returnCode}] : {message}");
        Debug.Log($"{roomName}에 입장 시도");
        PhotonNetwork.JoinRoom(roomName);
    }

    // 방 입장 (Join)
    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 성공");
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"방 입장 실패 [CODE:{returnCode}] : {message}");
        PhotonNetwork.Disconnect();
    }
}
