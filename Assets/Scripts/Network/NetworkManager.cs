using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField nameField;
    [SerializeField] TMP_InputField roomField;
    [SerializeField] Toggle remember;
    [SerializeField] SwitchButton connectButton;

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

        connectButton.Switch(false, "서버 연결 중");
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

        PhotonNetwork.NickName = nameField.text;    // 네트워크상 나의 닉네임을 지정.
        connectButton.Switch(false, "검색중");       // 버튼 비활성화.
        SaveOrLoad(true);                           // 데이터 세이브.

        StartCoroutine(Connecting());               // 룸 연결 시도.
    }
    IEnumerator Connecting()
    {
        // Room씬을 비동기식으로 로드 (Mode:더하기)
        AsyncOperation op = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Additive);
        while (!op.isDone)
            yield return null;

        // 씬 로드가 완료되면 Setup을 시킨다. 
        NetworkRoomManager room = NetworkRoomManager.Instance;
        room.Setup(roomField.text, (isSuccess) => {

            // 룸 입장 성공 여부에 따라 연결 버튼을 바꾼다.
            connectButton.Switch(!isSuccess, isSuccess ? "성공" : "방 입장");

            // 룸 입장 성공 여부에 따라 씬을 언로드한다.
            SceneManager.UnloadSceneAsync(isSuccess ? "Login" : "Room");
        });
    }

    // Network evnets...
    public override void OnConnectedToMaster()
    {
        // 마스터 서버에 접속.
        Debug.Log("마스터 서버 접속 성공");
        connectButton.Switch(true, "방 입장");
    }


}
