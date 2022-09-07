using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

using Photon.Pun;               // Pun(Photon unity network) : ���� ���ӿ�
using Photon.Chat;              // ä�� ���� ���ӽ����̽�.
using ExitGames.Client.Photon;  // Ŭ���̾�Ʈ ������.


public partial class Chatting : MonoBehaviour
{
    [SerializeField] TMP_Text textField;            // �Էµ� ä���� �� �ʵ�.
    [SerializeField] TMP_InputField inputField;     // ���� �Է� �ʵ�.
    [SerializeField] Color nameColor;               // �г��� ����.
    [SerializeField] int limitLine;                 // �ִ� �Է� ���� ��.

    RectTransform textFieldRect;                    // �ؽ�Ʈ �ʵ��� �簢 Ʈ������.
    Queue<string> records;                          // ����Ǿ��ִ� ä�� ������.

    string userName = "�׽���AB";
    string job = "����";
    
    private void Start()
    {
        textField.text = string.Empty;
        inputField.text = string.Empty;

        textFieldRect = textField.rectTransform;
        records = new Queue<string>();

        inputField.onEndEdit.AddListener(OnEndEdit);

        ConnectToServer();
    }
    private void Update()
    {
        // ����Ű�� ������ �� �Է� �ʵ尡 ���õǾ����� �ʾҴٸ�...
        if(Input.GetKeyDown(KeyCode.Return) && !inputField.isFocused)
        {
            inputField.Select();
        }

        OnUpdate();
    }

    private static string FORMAT = "[{0}:{1}] <color=#{2}>{3}({4}) : </color>{5}";

    private string ConvertChat(Message message)
    {
        return ConvertChat(message.name, message.job, message.msg);
    }
    private string ConvertChat(string name, string job, string msg)
    {
        //TimeSpan span = TimeSpan.FromSeconds(time);
        DateTime time = DateTime.Now;       // ���� �ð� �ޱ�.

        return string.Format(FORMAT,
            time.Hour.ToString("00"),
            time.Minute.ToString("00"),
            ColorUtility.ToHtmlStringRGB(nameColor),
            name,
            job,
            msg);
    }

    // '����' ä���� �Է����� �� �Ҹ��� �̺�Ʈ �Լ�.
    private void OnEndEdit(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        // �Է� �ʵ��� ���� ����������.
        inputField.text = string.Empty;

        // �޼����� ä�� ������ ������.
        OnSendMessage(new Message(userName, job, str));

        // �ٽ� ���Է� �� �� �ֵ��� Ȱ��ȭ ���ش�.
        // ���ʿ� Select�� ȣ���ϸ� �̺�Ʈ �ý��ۿ� ���õǰ� ��ü������ Initializer�� �ҷ� Activate�Ѵ�.
        // ���⼭ Enter�� ġ�� ��Ŀ���� Ǯ���� ���� �ƴ϶� Deactivate�ȴ�.
        //inputField.Select();
        inputField.ActivateInputField();
    }

    // �ؽ�Ʈ �信 ä���� �߰��Ѵ�.
    private void OnAddChat(string str)
    {
        // �Է��� �ؽ�Ʈ�� ����ִٸ� �������� �ʴ´�.
        if (string.IsNullOrEmpty(str))
            return;

        // ä���� ������� �����ϵ� 20���� ������ ���� ������ ä���� �����.
        records.Enqueue(str);

        // ����� �����Ͱ� limitLine�� ������ ���� ������ ������ ����.
        if (records.Count > limitLine)
            records.Dequeue();

        // �Է� �����͸� �������� ä�� �ؽ�Ʈ ��ȯ.
        textField.text = String.Join('\n', records);

        // �ؽ�Ʈ �ʵ��� ũ��� ��ġ�� ������ �Ѵ�.
        textFieldRect.sizeDelta = new Vector2(textFieldRect.sizeDelta.x, textField.preferredHeight);
        textFieldRect.localPosition = Vector3.zero;
    }
}

public partial class Chatting : IChatClientListener
{
    [System.Serializable]
    public struct Message
    {
        public string name;
        public string job;
        public string msg;


        public Message(string name, string job, string msg)
        {
            this.name = name;
            this.job = job;
            this.msg = msg;
        }
    }


    [Header("Photon")]
    [SerializeField] string channelName;

    private ChatClient client;      // ä�� Ŭ���̾�Ʈ(������ ����� ��)

    // ������ �����ϰڴ�.
    [ContextMenu("����")]
    private void ConnectToServer()
    {
        // ��׶��� ���°� �Ǹ� �⺻������ '�Ͻ�����' ���°� �ȴ�.
        // �׷��ԵǸ� ä�� �������� ������ ��������.
        Application.runInBackground = true;
        client = new ChatClient(this);                  // Ŭ���̾�Ʈ ����.
        client.UseBackgroundWorkerForSending = true;    // ��׶��� ���¿����� ������ ����.

        string chatId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat;
        string appVersion = "1.0";

        // ������ : ä�� ���� ������ ���� �ǹ�(�ĺ�)�ϴ� ������ ���ڿ�.
        AuthenticationValues authValues = new AuthenticationValues(userName);

        // Ŭ���̾�Ʈ�� chatId������ �������� ��� ������ �õ��Ѵ�.
        client.Connect(chatId, appVersion, authValues);

        Debug.Log("���� ���� �õ���..");
    }
    private void OnUpdate()
    {
        // Ŭ���̾�Ʈ�� ����.
        // Ŭ���̾�Ʈ�� �������� ������ �����ϰ� ������ �޽����� ó���ϱ� ���ؼ� ���������� ȣ��.
        if (client != null)
            client.Service();
    }
    private void OnSendMessage(Message message)
    {
        if (client == null)
            return;

        string json = JsonUtility.ToJson(message);
        if(!client.PublishMessage(channelName, json))
        {
            Debug.Log("�޼����� ���� �� �����ϴ�.");
        }
    }
    private void OnReceivedMessage(string channel, Message message)
    {
        OnAddChat(ConvertChat(message));
    }



    #region ä�� �̺�Ʈ �������̽�


    // �������� ���� ����� �޽��� (���� ���� ����)
    public void DebugReturn(DebugLevel level, string message)
    {
        switch(level)
        {
            case DebugLevel.ERROR:
                Debug.LogError(message);
                break;
            case DebugLevel.WARNING:
                Debug.LogWarning(message);
                break;
            default:
                Debug.Log(message);
                break;
        }
    }

    public void OnChatStateChange(ChatState state)
    {
        Debug.Log($"Change Chat State : {state}");
    }

    // ������ ������ �����ߴ�.
    public void OnConnected()
    {
        OnAddChat("ä�� ���� ������ �����߽��ϴ�.");

        // ������(ä��) �������� ������ ���������� �̷��������
        // ���� �޽����� ������ ä�ο� �����Ѵ�(�����Ѵ�)
        ChannelCreationOptions option = new ChannelCreationOptions() { PublishSubscribers = true };
        client.Subscribe(channelName, messagesFromHistory:0, creationOptions:option);
    }

    public void OnDisconnected()
    {
        Debug.Log("�������� ������ ��������.");
    }


    // ��ü �޼��� ����.
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for(int i= 0; i<senders.Length; i++)
        {
            // json���·� ���ŵ� ���ڿ��� Message��ü�� convert��Ų��.
            string json = messages[i].ToString();
            Debug.Log(json);
            Message message = JsonUtility.FromJson<Message>(json);
            OnReceivedMessage(channelName, message);
        }
    }
    // �ӼӸ� ����.
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        throw new NotImplementedException();
    }


    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new NotImplementedException();
    }

    // ä�� ������ �õ��߰� �׿� ���� ���.
    public void OnSubscribed(string[] channels, bool[] results)
    {
        for(int i = 0; i<channels.Length; i++)
        {
            if(results[i])
            {
                OnAddChat($"{channels[i]} ä�� ���� ����!");
            }
            else
            {
                OnAddChat($"{channels[i]} ä�� ���� ����..");
            }
        }
    }
    // ä�ο��� ������.
    public void OnUnsubscribed(string[] channels)
    {
        foreach (string channel in channels)
            OnAddChat($"{channel} ä�ο��� �����߽��ϴ�.");
    }

    public void OnUserSubscribed(string channel, string user)
    {
        Debug.Log("�������� : " + user);
        OnAddChat($"{user}�� �����߽��ϴ�!");
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        OnAddChat($"{user}�� �����߽��ϴ�!");
    }

    #endregion
}

