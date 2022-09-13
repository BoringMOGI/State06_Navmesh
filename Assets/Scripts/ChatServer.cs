using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;               // Pun(Photon unity network) : ���� ���ӿ�
using Photon.Chat;              // ä�� ���� ���ӽ����̽�.
using ExitGames.Client.Photon;  // Ŭ���̾�Ʈ ������.
using System.Linq;

using ChatNetwork;

namespace ChatNetwork
{
    [System.Serializable]
    public class Message
    {        
        public string channel;
        public string msg;

        public override string ToString()
        {
            return msg;
        }
    }
    public class ChatMessage : Message
    {
        private static string FORMAT = "[{0}:{1}] <color=#{2}>{3}({4}) : </color>{5}";

        public string userName;
        public string job;

        public override string ToString()
        {
            System.DateTime time = System.DateTime.Now;       // ���� �ð� �ޱ�.

            return string.Format(FORMAT,
                time.Hour.ToString("00"),
                time.Minute.ToString("00"),
                ColorUtility.ToHtmlStringRGB(Color.white),
                userName,
                job,
                msg);
        }
    }
}

[RequireComponent(typeof(ChatUI))]
public class ChatServer : MonoBehaviour, IChatClientListener
{
    #region ����

    public class Channel
    {
        static int MAX_CHAT_COUNT = 30;

        ChatChannel info;          // ä�� ä�� ����.
        Queue<string> records;     // ���� �޽���.

        public Channel(ChatChannel info)
        {
            this.info = info;
            records = new Queue<string>();
        }

        public string Name => info.Name;
        public int UserCount => info.Subscribers.Count;
        public string[] AllUsers => info.Subscribers.Select((user) => user).ToArray();
        public string AllMessage => string.Join('\n', records);

        public void AddMessage(string message)
        {
            // �޽����� �߰��Ѵ�. �ִ� ������ ������ ���� ������ �޽����� �����.
            records.Enqueue(message);
            if (records.Count > MAX_CHAT_COUNT)
                records.Dequeue();
        }
    }

    // ��� ����.
    Dictionary<string, Channel> channelList;    // ���� �������� ä�� ����Ʈ.
    ChatClient client;                          // ä�� Ŭ���̾�Ʈ(������ ����� ��)
    ChatUI chatUI;

    public string currentChannelName
    {
        get;
        private set;
    }

    #endregion


    void Awake()
    {
        chatUI = GetComponent<ChatUI>();
    }

    private void Update()
    {
        // Ŭ���̾�Ʈ�� ����.
        // Ŭ���̾�Ʈ�� �������� ������ �����ϰ� ������ �޽����� ó���ϱ� ���ؼ� ���������� ȣ��.
        if (client != null)
            client.Service();
    }

    // ������ �����ϰڴ�.
    public void ConnectToServer(string userName)
    {
        Debug.Log("���� ������ ���� �õ�...");

        // ������ �񿬰�� ���°� �ƴ϶�� �����Ѵ�.
        if (client != null && client.State != ChatState.Uninitialized)
            return;

        // ä�� ����Ʈ ��ü ����.
        channelList = new Dictionary<string, Channel>();

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
    }

    // ä�ο� �޽����� �۽��Ѵ�.
    public void OnSendMessage(Message message)
    {
        if (client == null)
            return;

        string json = JsonUtility.ToJson(message);
        if (!client.PublishMessage(currentChannelName, json))
            Debug.Log("�޼����� ���� �� �����ϴ�.");
    }

    // ä�ηκ��� �޽����� �����Ѵ�.
    private void OnReceivedMessage(Message message)
    {
        Channel channel = channelList[message.channel];

        // ������ �޽����� �ؽ�Ʈ(string)�������� ������ ä�ο� �߰�.
        channel.AddMessage(message.ToString());

        // ���� ���� ��Ŀ�� �ϰ� �ִ� ä�ΰ� ���ŵ� �޽����� ä���� ���� ���.
        // ��ϵǾ��ִ� �����ʵ鿡�� �˷��ش�.
        if(message.channel == currentChannelName)
        {
            // chatUI���� �޽��� ����.
            chatUI.OnUpdateChatView(channel.AllMessage);
        }
    }



    #region ä�� �̺�Ʈ �������̽�


    // �������� ���� ����� �޽��� (���� ���� ����)
    public void DebugReturn(DebugLevel level, string message)
    {
        switch (level)
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

    // Ŭ���̾�Ʈ�� ���� ������ ������ �����ߴ�. (���� ä�ο� ���� �ʾҴ�.)
    public void OnConnected()
    {
        // OnAddChat("ä�� ���� ������ �����߽��ϴ�.");
        // �⺻ ä�� �̸� (Default)
        currentChannelName = "Local";

        // ������(ä��) �������� ������ ���������� �̷��������
        // ���� �޽����� ������ ä�ο� �����Ѵ�(�����Ѵ�)
        ChannelCreationOptions option = new ChannelCreationOptions() { PublishSubscribers = true };
        client.Subscribe(currentChannelName, messagesFromHistory: 0, creationOptions: option);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 300), "AAA", new GUIStyle() { fontSize = 40 });
    }

    public void OnDisconnected()
    {
        Debug.Log("�������� ������ ��������.");
    }


    // ��ü �޼��� ����.
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            // json���·� ���ŵ� ���ڿ��� Message��ü�� convert��Ų��.
            string json = messages[i].ToString();
            Message message = JsonUtility.FromJson<Message>(json);

            // ���� �Լ� ȣ��.
            OnReceivedMessage(message);
        }
    }
    // �ӼӸ� ����.
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        
    }


    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        
    }

    // ä�� ������ �õ��߰� �׿� ���� ���.
    public void OnSubscribed(string[] channels, bool[] results)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            // ����(����) ������ ��� �ش� ������ �������� �ʴ´�.
            if (!results[i])
                continue;

            ChatChannel channel = null;

            // ������ ������ chennels[i]�� ä�� �����͸� �޶�.
            if (!client.TryGetChannel(channels[i], out channel))
                continue;

            // ���� ä�� �����͸� �츮�� ������ 'Channel'�� ���� �� ����Ʈ�� �߰�.
            channelList.Add(channels[i], new Channel(channel));
        }
    }
    // ä�ο��� ������.
    public void OnUnsubscribed(string[] channels)
    {
        foreach (string channel in channels)
        {

        }
    }

    public void OnUserSubscribed(string channel, string user)
    {
        
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
       
    }

    #endregion
}
