using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;               // Pun(Photon unity network) : ���� ���ӿ�
using Photon.Chat;              // ä�� ���� ���ӽ����̽�.
using ExitGames.Client.Photon;  // Ŭ���̾�Ʈ ������.
using System.Linq;

using ChatNetwork;
using ChatNetwork.Message;
using System;

namespace ChatNetwork.Message
{
    [Serializable]
    public class Message
    {
        public enum TYPE
        {
            Default,     // �⺻ �޽���.
            Chatting,    // ä�� �޽���.
            Status,      // �������ͽ� �޽���.
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

    [Serializable]
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
}

namespace ChatNetwork
{
    public class ChatUser
    {
        public enum STATUS
        {
            Offline,    // ��������.
            Online,     // �¶���.
            Away,       // �ڸ����.
        }

        public string name;
        public STATUS status;
        public ChatUser(string name, STATUS status)
        {
            this.name = name;
            this.status = status;
        }
    }
    public class Channel
    {
        // ==[ static field ] ===================================================================

        #region static field

        private static Dictionary<string, Channel> list = new Dictionary<string, Channel>();

        private static int MAX_CHAT_COUNT = 30;              // �ִ� ���� ���� ä�� ��.
        private static string currentName = string.Empty;    // ���� ���� ���� ä�� ��.

        // ���� �������� ä�� ��ü.
        public static Channel Current => list.ContainsKey(currentName) ? list[currentName] : null;             

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
        public static string[] GetAllConnectedChannel(string findUser)
        {
            // ��� ä���� ��ȸ�Ѵ�.
            // Ư�� ä���� userList���θ� Ž���� findUser�� �ִ��� ã�´�.
            // �ִٸ� �ش� ä���� �̸��� �ݺ��ڿ� �߰��Ѵ�.
            var search = from channel in list.Values
                         where channel.userList.Find(user => user.name.Equals(findUser)) != null
                         select channel.Name;

            return search.ToArray();
        }
        public static void Clear()
        {
            list?.Clear();              // ä�� ����Ʈ �ʱ�ȭ.
            currentName = "Local";      // ���� ä���� �̸��� Local�� �ʱ�ȭ.
        }

        #endregion

        // ======================================================================================

        // ��� ���.
        private ChatChannel info;          // ä�� ä�� ����.
        private Queue<string> records;     // ���� �޽���.
        private List<ChatUser> userList;   // ä�ο� �������� ���� ����Ʈ.

        public string Name { get; private set; }        // ä�� �̸�.
        public bool IsNewMessage { get; private set; }  // �ű� �޽��� �÷���.
        public bool IsUpdateUser { get; private set; }  // ���� ���� ���� �÷���.

        private Channel(string name)
        {
            // ���ʿ� ä�� ������ info�� ����ִ�. (������ ���� �õ� ��.. Ȥ�� ����)
            info = null;
            records = new Queue<string>();          // �޼��� ���.
            userList = new List<ChatUser>();        // ���� ���.
            Name = name;                            // ä�� ä�� �̸�.
        }

        // ��� �Լ�.
        public void LinkedChatChannel(ChatChannel info)
        {
            this.info = info;
            
            // ä�� ä�ΰ� ������ �Ǹ� �̹� �������� �������� �̸��� �޾ƿ� �����Ѵ�.
            foreach(string user in info.Subscribers)
                userList.Add(new ChatUser(user, ChatUser.STATUS.Online));

            IsUpdateUser = true;
        }
        public void Select()
        {
            // ������ ä���� �����ߴ�.
            if (currentName == Name)
                return;

            IsNewMessage = true;    // ä���� ���� ���õǸ� ä��â ������ �ٲ����ϱ� ������ true�� �����.
            currentName = Name;
        }

        // ���� ����.
        public void AddUser(string name)
        {
            userList.Add(new ChatUser(name, ChatUser.STATUS.Online));
            IsUpdateUser = true;
        }
        public void RemoveUser(string name)
        {
            // Ư�� ���ǽ����� List���ο��� Element�� index ã��.
            int index = userList.FindIndex(user => user.name.Equals(name));
            if (index < 0)
                return;

            userList.RemoveAt(index);
            IsUpdateUser = true;
        }
        public void UpdateUserStatus(string name, ChatUser.STATUS status)
        {
            // Ư�� ���ǽ����� List���ο��� Element ã��.
            ChatUser user = userList.Find(user => user.name.Equals(name));
            if (user == null)
                return;

            user.status = status;
            IsUpdateUser = true;
        }
        public ChatUser[] GetAllUser()
        {
            IsUpdateUser = false;
            return userList.ToArray();
        }

        // �޼��� ����.
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

    }
}

[RequireComponent(typeof(ChatUI))]
public class ChatServer : MonoBehaviour, IChatClientListener
{
    // ��� ����.
    private ChatClient client;         // ä�� Ŭ���̾�Ʈ(������ ����� ��)
    private static string userID;      // ���� �̸�.

    public static string UserID => userID;

    private Action onConnected;         // ���� �̺�Ʈ.


    private void Update()
    {
        // Ŭ���̾�Ʈ�� ����.
        // Ŭ���̾�Ʈ�� �������� ������ �����ϰ� ������ �޽����� ó���ϱ� ���ؼ� ���������� ȣ��.
        if (client != null)
            client.Service();
    }

    // ������ �����ϰڴ�.
    public void ConnectToServer(string userID, Action onConnected)
    {
        // ������ �񿬰�� ���°� �ƴ϶�� �����Ѵ�.
        if (client != null && client.State != ChatState.Uninitialized)
            return;

        // ����� �̸� �ʱ�ȭ.
        ChatServer.userID = userID;

        // ���� �̺�Ʈ �߰�.
        this.onConnected = onConnected;

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
        Channel.AddChannel(channelName, $"{channelName} ������ ���� ���Դϴ�...");

        // �ʱ� ���� ����.
        if(isSelected)
            Channel.GetChannel(channelName).Select();

        // ������(ä��) �������� ������ ���������� �̷��������
        // ���� �޽����� ������ ä�ο� �����Ѵ�(�����Ѵ�)
        ChannelCreationOptions option = new ChannelCreationOptions() { PublishSubscribers = true };
        client.Subscribe(channelName, messagesFromHistory: 0, creationOptions: option);
    }
    public void OnChangedStatus(ChatUser.STATUS status)
    {
        client.SetOnlineStatus((int)status);
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
        onConnected?.Invoke();
        onConnected = null;
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

    // ������ ���� ���Դ�.
    public void OnUserSubscribed(string channelName, string user)
    {
        // �ش��ϴ� ä�ο� ������ �߰���Ų��.
        Channel channel = Channel.GetChannel(channelName);
        channel?.AddUser(user);
    }
    public void OnUserUnsubscribed(string channelName, string user)
    {
        // �ش��ϴ� ä�ο��� ������ ���Ž�Ų��.
        Channel channel = Channel.GetChannel(channelName);
        channel?.RemoveUser(user);
    }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        ChatUser.STATUS userStatus = (ChatUser.STATUS)status;       // ����.
        string[] channels = Channel.GetAllConnectedChannel(user);   // ���� ���� ä�� �迭.

        foreach (string channel in channels)
            Channel.GetChannel(channel).UpdateUserStatus(user, userStatus);

        /*
         * ������ ���� ChatUserStatus
         * 1. offline : �������� ����.
         * 2. Invisible : ���� (�������� ����)
         * 3. Online : �¶��� ����.
         * 4. Away : �ڸ����.
         * 5. DND (Do not disturb) : �������� ������.
         * 6. LGF (Looking for game) : ��Ƽ/�׷�/���� ã�� ��...
         * 7. Playing : ������..
         */
    }

    #endregion
}
