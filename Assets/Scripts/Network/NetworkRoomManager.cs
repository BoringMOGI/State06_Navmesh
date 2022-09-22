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

    [SerializeField] Transform userParent;

    [SerializeField] TMP_Text roomNameText;     // �� �̸� �ؽ�Ʈ.
    [SerializeField] TMP_Text roomInfoText;     // �� ���� ���� �ؽ�Ʈ.

    [SerializeField] RoomChatUI roomChat;

    string roomName;                            // ���� �������� ���� �̸�.
    Room room;                                  // ���� �������� �� ����.

    List<RoomUserUI> userList;                  // �������� ������ Ŭ�� ����Ʈ.

    System.Action<bool> onSuccess;              // �� ���� ���� ���� �Լ�.

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        userList = new List<RoomUserUI>();
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

        // �濡 ���������� ���� ��, �� ���� ä�� UI�� �����Ѵ�.
        roomChat.OnJoinedRoom(PhotonNetwork.NickName, room.Name);

        // �濡 ���ӽ� ��(user)�� ��ü�� �����Ѵ�.
        // Setup�� RPC�� �̿��� ��Ʈ��ũ ���� ��� '��'���� ���۵ȴ�.
        GameObject obj = PhotonNetwork.Instantiate("Room/RoomUser", Vector3.zero, Quaternion.identity);
        RoomUserUI userUI = obj.GetComponent<RoomUserUI>();
        userUI.transform.SetParent(userParent);
        userUI.Setup(PhotonNetwork.LocalPlayer);
              
        UpdateUserList();
    }
    private void EnterUser(User user)
    {
        UpdateUserList();
    }
    private void LeaveUser(User user)
    {
        UpdateUserList();
    }
    private void UpdateUserList()
    {
        // ���� �濡 �����ϴ� ��� Ŭ���� ã�´�.
        RoomUserUI[] users = FindObjectsOfType<RoomUserUI>();

        // actorNumber�� �������� �������� �������� �����Ѵ�.
        var query = users.OrderBy((user) => user.ActorNumber).ToList();
        userList.Clear();
        userList.AddRange(query);

        // ���� ���� ���� �ؽ�Ʈ.
        const string FORMAT = "<color=#{0}>Host | </color>{1}";
        string infoColor = ColorUtility.ToHtmlStringRGBA(Color.green);
        string hostName = query.First().NickName;

        roomInfoText.text = string.Format(FORMAT, infoColor, hostName);

        // ������ UI�� ������� �θ� �ؿ� �д�.
        for (int i = 0; i < userList.Count; i++)
        {
            RoomUserUI user = userList[i];
            user.transform.SetAsLastSibling();

            // �� UI�� ��� ���� �����͸� ������Ʈ ��Ų��.
            if (user.IsMine)
                user.UpdateInfo();
        }
    }

    
    // �غ� ��ư�� ������.
    public void OnClickReady()
    {
        
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
