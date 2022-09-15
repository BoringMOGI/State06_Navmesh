using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;               // Pun(Photon unity network) : ���� ���ӿ�
using Photon.Chat;              // ä�� ���� ���ӽ����̽�.
using ExitGames.Client.Photon;  // Ŭ���̾�Ʈ ������.
using System.Linq;

using ChatNetwork;
using System;

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
        public string id;
        public string msg;
        public TYPE type;

        public Message(string channel, string id, string msg)
        {
            this.channel = channel;
            this.id = id;
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

        public string nickName;
        public string job;

        public ChatMessage(string channel, string id, string msg, string nickName, string job)
            : base(channel, id, msg)
        {
            this.nickName = nickName;
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
                nickName,
                job,
                msg);
        }
    }
    public class Channel
    {
        // static ����.
        private static Dictionary<string, Channel> list = new Dictionary<string, Channel>();

        private static int MAX_CHAT_COUNT = 30;              // �ִ� ���� ���� ä�� ��.
        private static string currentName;                   // ���� ���� ���� ä�� ��.

        // ���� �������� ä�� ��ü.
        public static Channel Current => list.ContainsKey(currentName) ? list[currentName] : null;

        // ��� ���.
        private ChatChannel info;          // ä�� ä�� ����.
        private Queue<string> records;     // ���� �޽���.

        public string Name { get; private set; }        // ä�� �̸�.
        public bool IsNewMessage { get; private set; }  // �ű� �޽��� �÷���.

        private Channel(string name)
        {
            // ���ʿ� ä�� ������ info�� ����ִ�. (������ ���� �õ� ��.. Ȥ�� ����)
            info = null;
            records = new Queue<string>();
            currentName = "Local";
            Name = name;
        }

        public static void AddChannel(string channelName, params string[] messages)
        {
            // �̹� ä���� �߰��Ǿ� �ִٸ� �������� �ʴ´�.
            if (list.ContainsKey(channelName))
                return;

            // ���ο� ä�� ����. ���޵� �޼��� ����.
            Channel newChannel = new Channel(channelName);
            foreach (string message in messages)
                newChannel.AddMessage(message);

            // ����Ʈ�� �߰�.
            list.Add(channelName, newChannel);
        }
        public static void RemoveChannel(string channelName)
        {
            // ä�� ����.
            if (list.ContainsKey(channelName))
                list.Remove(channelName);
        }
        public static Channel GetChannel(string channelName)
        {
            return list[channelName];
        }
        public static bool IsContains(string channelName)
        {
            return list.ContainsKey(channelName);
        }
        public static void Clear()
        {
            list?.Clear();              // ä�� ����Ʈ �ʱ�ȭ.
            currentName = "Local";      // ���� ä���� �̸��� Local�� �ʱ�ȭ.
        }


        // ��� �Լ�.
        public void LinkedChatChannel(ChatChannel info)
        {
            this.info = info;
        }
        public void AddMessage(string message)
        {
            // �޽����� �߰��Ѵ�. �ִ� ������ ������ ���� ������ �޽����� �����.
            records.Enqueue(message);
            if (records.Count > MAX_CHAT_COUNT)
                records.Dequeue();

            // ���ο� �޽����� �Դٰ� Flag ó��.
            IsNewMessage = true;
        }
        public string GetAllMessage()
        {
            // �޼����� �޾ư����� Flag ��Ȱ��ȭ.
            IsNewMessage = false;
            return string.Join('\n', records);
        }
        public void Select()
        {
            currentName = Name;
        }
    }
}

[RequireComponent(typeof(ChatUI))]
public class ChatServer : MonoBehaviour, IChatClientListener
{
    // ��� ����.
    private ChatClient client;         // ä�� Ŭ���̾�Ʈ(������ ����� ��)
    private static string userID;      // ���� �̸�.

    public static string UserID => userID;

    private void Start()
    {

        
    }

    private void Update()
    {
        // Ŭ���̾�Ʈ�� ����.
        // Ŭ���̾�Ʈ�� �������� ������ �����ϰ� ������ �޽����� ó���ϱ� ���ؼ� ���������� ȣ��.
        if (client != null)
            client.Service();
    }

    // ������ �����ϰڴ�.
    public void ConnectToServer(string userID)
    {
        // ������ �񿬰�� ���°� �ƴ϶�� �����Ѵ�.
        if (client != null && client.State != ChatState.Uninitialized)
            return;

        // ����� �̸� �ʱ�ȭ.
        ChatServer.userID = userID;

        // ä�� ������ �ʱ�ȭ ��Ų��.
        Debug.Log("���� ������ ���� �õ�...");
        Channel.Clear();

        // ��׶��� ���°� �Ǹ� �⺻������ '�Ͻ�����' ���°� �ȴ�.
        // �׷��ԵǸ� ä�� �������� ������ ��������.
        Application.runInBackground = true;
        client = new ChatClient(this);                  // Ŭ���̾�Ʈ ����.
        client.UseBackgroundWorkerForSending = true;    // ��׶��� ���¿����� ������ ����.                

        string chatId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat;
        string appVersion = "1.0";

        // ������ : ä�� ���� ������ ���� �ǹ�(�ĺ�)�ϴ� ������ ���ڿ�.
        AuthenticationValues authValues = new AuthenticationValues(userID);

        // Ŭ���̾�Ʈ�� chatId������ �������� ��� ������ �õ��Ѵ�.
        client.Connect(chatId, appVersion, authValues);
    }
    public void ConnectToChannel(string channelName, bool isSelected = false)
    {
        // ä�� ��ü �߰�.
        Channel.AddChannel(channelName, "������ ���� ���Դϴ�...");

        // �ʱ� ���� ����.
        if(isSelected)
            Channel.GetChannel(channelName).Select();

        // ������(ä��) �������� ������ ���������� �̷��������
        // ���� �޽����� ������ ä�ο� �����Ѵ�(�����Ѵ�)
        ChannelCreationOptions option = new ChannelCreationOptions() { PublishSubscribers = true };
        client.Subscribe(channelName, messagesFromHistory: 0, creationOptions: option);
    }

    // ä�ο� �޽����� �۽��Ѵ�.
    public void OnSendMessage(Message message)
    {
        if (client == null)
            return;

        // ������ �޼����� ���� �� �� �޼����� ���ÿ� �����Ѵ�.
        Channel.GetChannel(message.channel).AddMessage(message.ToString());

        // ������ ������ ���� json���� �Ľ�.
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
        
        if (!client.PublishMessage(message.channel, json))
            Debug.Log("�޼����� ���� �� �����ϴ�.");
    }

    // ä�ηκ��� �޽����� �����Ѵ�.
    private void OnReceivedMessage(string json)
    {
        // �����κ��� ���� ���� �޼��� �� ���� �޼����� �����Ѵ�.
        // ���� �޼����� ������ ������ �̹� ä�� ��ü ���ο� ������ ���̱� �����̴�.
        Dictionary<string, string> obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (obj["id"] == userID)
            return;

        // Ÿ������ �з�.
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

        // �ش��ϴ� ä�ο� �޼��� �߰�.
        Channel channel = Channel.GetChannel(message.channel);
        channel.AddMessage(message.ToString());
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
        ConnectToChannel("Local", true);
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
            string name = channels[i];
            bool result = results[i];

            // ����(����) ������ ��� �ش� ������ �������� �ʴ´�.
            // �׸��� ������ ä���� �����Ѵ�.
            if (!result)
            {
                Channel.RemoveChannel(name);
                // �߰��� UI���� ä�� ��ư�� �����϶�� �˸�.
                continue;
            }

            // (����)ä�� ä�� ����
            ChatChannel chatChannel = null;
            client.TryGetChannel(name, out chatChannel);

            // �ش��ϴ� ä�ο� ���� �޼����� (����)ä�� ä�� ���� ����.
            Channel channel = Channel.GetChannel(name);
            channel.AddMessage("ä�� ���� ����!");
            channel.LinkedChatChannel(chatChannel);
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
