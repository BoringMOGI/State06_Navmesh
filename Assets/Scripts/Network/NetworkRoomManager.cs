using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;

using User = Photon.Realtime.Player;
using TMPro;

public class NetworkRoomManager : MonoBehaviourPunCallbacks
{
    public static NetworkRoomManager Instance { get; private set; }

    [SerializeField] RoomUserUI userPrefab;
    [SerializeField] Transform userParent;

    [SerializeField] TMP_Text roomNameText;     // 방 이름 텍스트.
    [SerializeField] TMP_Text roomInfoText;     // 방 세부 정보 텍스트.

    [SerializeField] RoomChatUI roomChat;


    string roomName;                    // 현재 접속중인 방의 이름.
    Room room;                          // 현재 접속중인 방 정보.
    List<User> userList;                // 입장 중인 유저의 리스트.

    System.Action<bool> onSuccess;      // 룸 입장 성공 여부 함수.

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        userList = new List<User>();
    }

    public void Setup(string name, System.Action<bool> onSuccess)
    {
        this.onSuccess = onSuccess;

        // null과 ""는 다르다.
        // 만약 방 이름을 null로 하면 랜덤한 방을 만들라는 의미다.
        roomName = name.Equals(string.Empty) ? null : name;
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 9 });

        // CreateRoom               : 방 생성
        // JoinRoom                 : 방 입장
        // JoinOrCreateRoom         : 방 입장 시도, 실패 시 방 생성.
        // JoinRandomOrCreateRoom   : 랜덤 방 입장 시도, 실패 시 방 생성.
    }

    private void JoinedRoom()
    {
        room = PhotonNetwork.CurrentRoom;           // 방에 입장하면 방 정보를 대입.

        roomName = room.Name;                       // 방에 성공적으로 접속 후. 실제 방 이름을 대입.
        roomNameText.text = roomName;               // (실제)방이름 텍스트에 대입.

        userList.AddRange(room.Players.Values);     // 현재 접속중인 유저의 정보를 리스트에 대입.
        UpdateUserList();

        // 방에 성공적으로 접속 시, 룸 전용 채팅 UI에 접속한다.
        roomChat.OnJoinedRoom(PhotonNetwork.NickName, room.Name);
    }
    private void EnterUser(User user)
    {
        userList.Add(user);
        UpdateUserList();
    }
    private void LeaveUser(User user)
    {
        userList.Remove(user);
        UpdateUserList();
    }
    private void UpdateUserList()
    {
        // actorNumber를 기준으로 유저들을 오름차순 정렬한다.
        var query = userList.OrderBy((user) => user.ActorNumber).ToList();
        userList.Clear();
        userList.AddRange(query);

        // 방의 세부 정보 텍스트.
        const string FORMAT = "<color=#{0}>Host | </color>{1}";
        string infoColor = ColorUtility.ToHtmlStringRGBA(Color.green);
        string hostName = query.First().NickName;

        roomInfoText.text = string.Format(FORMAT, infoColor, hostName);


        // 기존 유저 목록 UI 삭제.
        foreach (Transform child in userParent)
            Destroy(child.gameObject);

        // user의 count만큼 프리팹 생성.
        for (int i = 0; i < userList.Count; i++)
        {
            RoomUserUI ui = Instantiate(userPrefab, userParent);
            User user = userList[i];

            ui.Setup(i + 1, user.NickName, user.IsMasterClient);
        }
    }





    // 방 생성 (Create)
    public override void OnCreatedRoom()
    {
        // 사실상 마스터 클라이언트(방장)만 호출 받을 수 있다.
        // 여기서 방장이란 해당 룸을 생성한 사람이다.
        // 또는 방장이 나갔을 때 그 다음으로 들어온 사람이다.
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
        onSuccess?.Invoke(true);
        JoinedRoom();
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"방 입장 실패 [CODE:{returnCode}] : {message}");
        onSuccess?.Invoke(false);
    }

    // 유저의 입장, 퇴장.
    public override void OnPlayerEnteredRoom(User newPlayer)
    {
        EnterUser(newPlayer);
    }
    public override void OnPlayerLeftRoom(User otherPlayer)
    {
        LeaveUser(otherPlayer);
    }

}
