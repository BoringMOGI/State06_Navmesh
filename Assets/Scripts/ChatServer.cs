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
        public enum TYPE
        {
            Default,     // �⺻ �޽���.
            Chatting,    // ä�� �޽���.
        }

        public string channel;
        public string msg;
        public TYPE type;

        public Message(string channel, string msg)
        {
            this.channel = channel;
            this.msg = msg;
            type = TYPE.Default;
        }

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

        public ChatMessage(string channel, string msg, string userName, string job)
            : base(channel, msg)
        {
            this.userName = userName;
            this.job = job;
            type = TYPE.Chatting;
        }

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

        string json = string.Empty;
        switch(message.type)
        {
            case Message.TYPE.Default:
                json = JsonUtility.ToJson(message);
                break;

            case Message.TYPE.Chatting:
                ChatMessage chatMessage = message as ChatMessage;
                if(chatMessage != null)
                    json = JsonUtility.ToJson(chatMessage);
                break;
        }
        
        if (!client.PublishMessage(currentChannelName, json))
            Debug.Log("�޼����� ���� �� �����ϴ�.");
    }

    // ä�ηκ��� �޽����� �����Ѵ�.
    private void OnReceivedMessage(string json)
    {
        // �����κ��� ���۹��� json ���ڿ��� type�� �������� ���.
        Dictionary<string, string> obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Message.TYPE type = (Message.TYPE)System.Enum.Parse(typeof(Message.TYPE), obj["type"]);

        // ��ȣȭ ��Ų �����͸� ����Ű�� Message ������ ����.
        Message message = null;
        switch(type)
        {
            case Message.TYPE.Default:
                message = JsonUtility.FromJson<Message>(json);
                break;
            case Message.TYPE.Chatting:
                message = JsonUtility.FromJson<ChatMessage>(json);
                break;
        }

        Channel channel = channelList[message.channel];     // ä�� ����Ʈ���� �̸����� ä�� ������ ������.
        channel.AddMessage(message.ToString());             // �ش� ä�� ���ο� �޼��� �߰�.

        chatUI.OnUpdateChatView(channel.AllMessage);        // �ش� ä���� ��� ä�� ����� UI�� ���.
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
    public void OnDisconnected()
    {
        Debug.Log("�������� ������ ��������.");
    }


    // ��ü �޼��� ����.
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            // ���� �Լ� ȣ��.
            OnReceivedMessage(messages[i].ToString());
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
