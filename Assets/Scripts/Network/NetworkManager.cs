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
        // ������ Ŭ���̾�Ʈ�� ��û���� ���ο� ���� �ε�� ��
        // ���� ���� ��� Ŭ���̾�Ʈ�� �ڵ����� ������ ����ȭ�Ѵ�.
        // MasterClinet�� PhotonNetwork.LoadLevel("")�� ȣ���� �� �ִ�.
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
        SaveOrLoad(false);                          // ������ �ε�.
        PhotonNetwork.GameVersion = gameVersion;    // ���� ���� (���� �� ������ ����)
        PhotonNetwork.ConnectUsingSettings();       // ������ ���� ���� �õ�.
    }

    private void SaveOrLoad(bool isSave)
    {
        // ������ ���̺�.
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
        // ������ �ε�.
        else
        {
            // ���۽� ������ �Է� ������ �����ߴ��� üũ.
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
            Debug.Log("���� ��Ʈ��ũ�� ����Ǿ����� �ʽ��ϴ�.");
            return;
        }

        if(string.IsNullOrEmpty(nameField.text))
        {
            Debug.Log("�̸��� �Էµ��� �ʾҽ��ϴ�.");
            return;
        }

        SaveOrLoad(true);       // ������ ���̺�.

        // null�� ""�� �ٸ���.
        string roomName = roomField.text.Equals(string.Empty) ? null : roomField.text;

        // ���� �� �̸��� null�� �ϸ� ������ ���� ������ �ǹ̴�.
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 10 });

        // CreateRoom               : �� ����
        // JoinRoom                 : �� ����
        // JoinOrCreateRoom         : �� ���� �õ�, ���� �� �� ����.
        // JoinRandomOrCreateRoom   : ���� �� ���� �õ�, ���� �� �� ����.
    }


    // Network evnets...
    public override void OnConnectedToMaster()
    {
        // ������ ������ ����,����.
        if (PhotonNetwork.IsConnected)
            Debug.Log("������ ���� ���� ����");
    }

    // �� ���� (Create)
    public override void OnCreatedRoom()
    {
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
        Debug.Log("�� ���� ����");
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"�� ���� ���� [CODE:{returnCode}] : {message}");
        PhotonNetwork.Disconnect();
    }
}
