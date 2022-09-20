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

    [SerializeField] TMP_Text roomNameText;     // �� �̸� �ؽ�Ʈ.
    [SerializeField] TMP_Text roomInfoText;     // �� ���� ���� �ؽ�Ʈ.

    [SerializeField] RoomChatUI roomChat;


    string roomName;                    // ���� �������� ���� �̸�.
    Room room;                          // ���� �������� �� ����.
    List<User> userList;                // ���� ���� ������ ����Ʈ.

    System.Action<bool> onSuccess;      // �� ���� ���� ���� �Լ�.

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

        // null�� ""�� �ٸ���.
        // ���� �� �̸��� null�� �ϸ� ������ ���� ������ �ǹ̴�.
        roomName = name.Equals(string.Empty) ? null : name;
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 9 });

        // CreateRoom               : �� ����
        // JoinRoom                 : �� ����
        // JoinOrCreateRoom         : �� ���� �õ�, ���� �� �� ����.
        // JoinRandomOrCreateRoom   : ���� �� ���� �õ�, ���� �� �� ����.
    }

    private void JoinedRoom()
    {
        room = PhotonNetwork.CurrentRoom;           // �濡 �����ϸ� �� ������ ����.

        roomName = room.Name;                       // �濡 ���������� ���� ��. ���� �� �̸��� ����.
        roomNameText.text = roomName;               // (����)���̸� �ؽ�Ʈ�� ����.

        userList.AddRange(room.Players.Values);     // ���� �������� ������ ������ ����Ʈ�� ����.
        UpdateUserList();

        // �濡 ���������� ���� ��, �� ���� ä�� UI�� �����Ѵ�.
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
        // actorNumber�� �������� �������� �������� �����Ѵ�.
        var query = userList.OrderBy((user) => user.ActorNumber).ToList();
        userList.Clear();
        userList.AddRange(query);

        // ���� ���� ���� �ؽ�Ʈ.
        const string FORMAT = "<color=#{0}>Host | </color>{1}";
        string infoColor = ColorUtility.ToHtmlStringRGBA(Color.green);
        string hostName = query.First().NickName;

        roomInfoText.text = string.Format(FORMAT, infoColor, hostName);


        // ���� ���� ��� UI ����.
        foreach (Transform child in userParent)
            Destroy(child.gameObject);

        // user�� count��ŭ ������ ����.
        for (int i = 0; i < userList.Count; i++)
        {
            RoomUserUI ui = Instantiate(userPrefab, userParent);
            User user = userList[i];

            ui.Setup(i + 1, user.NickName, user.IsMasterClient);
        }
    }





    // �� ���� (Create)
    public override void OnCreatedRoom()
    {
        // ��ǻ� ������ Ŭ���̾�Ʈ(����)�� ȣ�� ���� �� �ִ�.
        // ���⼭ �����̶� �ش� ���� ������ ����̴�.
        // �Ǵ� ������ ������ �� �� �������� ���� ����̴�.
        Debug.Log("�� ���� �Ϸ�!!");
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log($"�� ���� ���� [CODE:{returnCode}] : {message}");
        Debug.Log($"{roomName}�� ���� �õ�");
        PhotonNetwork.JoinRoom(roomName);
    }

    // �� ���� (Join)
    public override void OnJoinedRoom()
    {
        onSuccess?.Invoke(true);
        JoinedRoom();
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"�� ���� ���� [CODE:{returnCode}] : {message}");
        onSuccess?.Invoke(false);
    }

    // ������ ����, ����.
    public override void OnPlayerEnteredRoom(User newPlayer)
    {
        EnterUser(newPlayer);
    }
    public override void OnPlayerLeftRoom(User otherPlayer)
    {
        LeaveUser(otherPlayer);
    }

}
