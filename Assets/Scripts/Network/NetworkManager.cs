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

        connectButton.Switch(false, "���� ���� ��");
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

        PhotonNetwork.NickName = nameField.text;    // ��Ʈ��ũ�� ���� �г����� ����.
        connectButton.Switch(false, "�˻���");       // ��ư ��Ȱ��ȭ.
        SaveOrLoad(true);                           // ������ ���̺�.

        StartCoroutine(Connecting());               // �� ���� �õ�.
    }
    IEnumerator Connecting()
    {
        // Room���� �񵿱������ �ε� (Mode:���ϱ�)
        AsyncOperation op = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Additive);
        while (!op.isDone)
            yield return null;

        // �� �ε尡 �Ϸ�Ǹ� Setup�� ��Ų��. 
        NetworkRoomManager room = NetworkRoomManager.Instance;
        room.Setup(roomField.text, (isSuccess) => {

            // �� ���� ���� ���ο� ���� ���� ��ư�� �ٲ۴�.
            connectButton.Switch(!isSuccess, isSuccess ? "����" : "�� ����");

            // �� ���� ���� ���ο� ���� ���� ��ε��Ѵ�.
            SceneManager.UnloadSceneAsync(isSuccess ? "Login" : "Room");
        });
    }

    // Network evnets...
    public override void OnConnectedToMaster()
    {
        // ������ ������ ����.
        Debug.Log("������ ���� ���� ����");
        connectButton.Switch(true, "�� ����");
    }


}
