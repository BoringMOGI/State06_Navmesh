using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;               // Pun(Photon unity network) : 서버 접속용
using Photon.Chat;              // 채팅 관련 네임스페이스.
using ExitGames.Client.Photon;  // 클라이언트 생성용.
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
            Default,     // 기본 메시지.
            Chatting,    // 채팅 메시지.
            Status,      // 스테이터스 메시지.
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
            System.DateTime time = System.DateTime.Now;       // 현재 시간 받기.

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
            Offline,    // 오프라인.
            Online,     // 온라인.
            Away,       // 자리비움.
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

        private static int MAX_CHAT_COUNT = 30;              // 최대 저장 가능 채팅 수.
        private static string currentName = string.Empty;    // 현재 선택 중인 채널 명.

        // 현재 선택중인 채널 객체.
        public static Channel Current => list.ContainsKey(currentName) ? list[currentName] : null;             

        public static void AddChannel(string channelName, params string[] messages)
        {
            // 이미 채널이 추가되어 있다면 실행하지 않는다.
            if (list.ContainsKey(channelName))
                return;

            // 새로운 채널 생성. 전달된 메세지 대입.
            Channel newChannel = new Channel(channelName);
            foreach (string message in messages)
                newChannel.AddMessage(message);

            // 리스트에 추가.
            list.Add(channelName, newChannel);
        }
        public static void RemoveChannel(string channelName)
        {
            // 채널 삭제.
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
            // 모든 채널을 순회한다.
            // 특정 채널의 userList내부를 탐색해 findUser가 있는지 찾는다.
            // 있다면 해당 채널의 이름을 반복자에 추가한다.
            var search = from channel in list.Values
                         where channel.userList.Find(user => user.name.Equals(findUser)) != null
                         select channel.Name;

            return search.ToArray();
        }
        public static void Clear()
        {
            list?.Clear();              // 채널 리스트 초기화.
            currentName = "Local";      // 현재 채널의 이름을 Local로 초기화.
        }

        #endregion

        // ======================================================================================

        // 멤버 면수.
        private ChatChannel info;          // 채팅 채널 정보.
        private Queue<string> records;     // 수신 메시지.
        private List<ChatUser> userList;   // 채널에 접속중인 유저 리스트.

        public string Name { get; private set; }        // 채널 이름.
        public bool IsNewMessage { get; private set; }  // 신규 메시지 플래그.
        public bool IsUpdateUser { get; private set; }  // 유저 상태 변경 플래그.

        private Channel(string name)
        {
            // 최초에 채널 생성시 info는 비어있다. (서버로 연결 시도 중.. 혹은 무언가)
            info = null;
            records = new Queue<string>();          // 메세지 기록.
            userList = new List<ChatUser>();        // 유저 목록.
            Name = name;                            // 채팅 채널 이름.
        }

        // 멤버 함수.
        public void LinkedChatChannel(ChatChannel info)
        {
            this.info = info;
            
            // 채팅 채널과 연결이 되면 이미 접속중인 유저들의 이름을 받아와 대입한다.
            foreach(string user in info.Subscribers)
                userList.Add(new ChatUser(user, ChatUser.STATUS.Online));

            IsUpdateUser = true;
        }
        public void Select()
        {
            // 동일한 채널을 선택했다.
            if (currentName == Name)
                return;

            IsNewMessage = true;    // 채널이 새로 선택되면 채팅창 내용이 바뀌어야하기 때문에 true로 만든다.
            currentName = Name;
        }

        // 유저 관리.
        public void AddUser(string name)
        {
            userList.Add(new ChatUser(name, ChatUser.STATUS.Online));
            IsUpdateUser = true;
        }
        public void RemoveUser(string name)
        {
            // 특정 조건식으로 List내부에서 Element의 index 찾기.
            int index = userList.FindIndex(user => user.name.Equals(name));
            if (index < 0)
                return;

            userList.RemoveAt(index);
            IsUpdateUser = true;
        }
        public void UpdateUserStatus(string name, ChatUser.STATUS status)
        {
            // 특정 조건식으로 List내부에서 Element 찾기.
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

        // 메세지 관리.
        public void AddMessage(string message)
        {
            // 메시지를 추가한다. 최대 개수를 넘으면 가장 마지막 메시지를 지운다.
            records.Enqueue(message);
            if (records.Count > MAX_CHAT_COUNT)
                records.Dequeue();

            // 새로운 메시지가 왔다고 Flag 처리.
            IsNewMessage = true;
        }
        public string GetAllMessage()
        {
            // 메세지를 받아갔으니 Flag 비활성화.
            IsNewMessage = false;
            return string.Join('\n', records);
        }

    }
}

[RequireComponent(typeof(ChatUI))]
public class ChatServer : MonoBehaviour, IChatClientListener
{
    // 멤버 변수.
    private ChatClient client;         // 채팅 클라이언트(서버와 연결된 나)
    private static string userID;      // 나의 이름.

    public static string UserID => userID;

    private Action onConnected;         // 연결 이벤트.


    private void Update()
    {
        // 클라이언트의 교신.
        // 클라이언트는 서버와의 연결을 유지하고 들어오는 메시지를 처리하기 위해서 정기적으로 호출.
        if (client != null)
            client.Service();
    }

    // 서버에 접속하겠다.
    public void ConnectToServer(string userID, Action onConnected)
    {
        // 서버와 비연결된 상태가 아니라면 리턴한다.
        if (client != null && client.State != ChatState.Uninitialized)
            return;

        // 사용자 이름 초기화.
        ChatServer.userID = userID;

        // 연결 이벤트 추가.
        this.onConnected = onConnected;

        // 채널 정보를 초기화 시킨다.
        Debug.Log("포톤 서버에 접속 시도...");
        Channel.Clear();

        // 백그라운드 상태가 되면 기본적으로 '일시정지' 상태가 된다.
        // 그렇게되면 채팅 서버와의 연결이 끊어진다.
        Application.runInBackground = true;
        client = new ChatClient(this);                  // 클라이언트 생성.
        client.UseBackgroundWorkerForSending = true;    // 백그라운드 상태에서도 데이터 전송.                

        string chatId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat;
        string appVersion = "1.0";

        // 인증서 : 채팅 서버 내에서 나를 의미(식별)하는 고유한 문자열.
        AuthenticationValues authValues = new AuthenticationValues(userID);

        // 클라이언트가 chatId서버에 인증서를 들고 연결을 시도한다.
        client.Connect(chatId, appVersion, authValues);
    }
    public void ConnectToChannel(string channelName, bool isSelected = false)
    {
        // 채널 객체 추가.
        Channel.AddChannel(channelName, $"{channelName} 서버에 접속 중입니다...");

        // 초기 선택 상태.
        if(isSelected)
            Channel.GetChannel(channelName).Select();

        // 마스터(채텅) 서버와의 연결이 성공적으로 이루어졌으니
        // 실제 메시지가 오가는 채널에 입장한다(구독한다)
        ChannelCreationOptions option = new ChannelCreationOptions() { PublishSubscribers = true };
        client.Subscribe(channelName, messagesFromHistory: 0, creationOptions: option);
    }
    public void OnChangedStatus(ChatUser.STATUS status)
    {
        client.SetOnlineStatus((int)status);
    }


    // 채널에 메시지를 송신한다.
    public void OnSendMessage(Message message)
    {
        if (client == null)
            return;

        // 서버로 메세지를 보낼 때 내 메세지는 로컬에 저장한다.
        Channel.GetChannel(message.channel).AddMessage(message.ToString());

        // 서버로 보내기 위해 json으로 파싱.
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
            Debug.Log("메세지를 보낼 수 없습니다.");
    }

    // 채널로부터 메시지를 수신한다.
    private void OnReceivedMessage(string json)
    {
        // 서버로부터 수신 받은 메세지 중 나의 메세지는 생략한다.
        // 나의 메세지는 보내는 시점에 이미 채널 객체 내부에 저장할 것이기 때문이다.
        Dictionary<string, string> obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (obj["id"] == userID)
            return;

        // 타입으로 분류.
        Message.TYPE type = (Message.TYPE)System.Enum.Parse(typeof(Message.TYPE), obj["type"]);

        // 복호화 시킨 데이터를 가리키는 Message 참조형 변수.
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

        // 해당하는 채널에 메세지 추가.
        Channel channel = Channel.GetChannel(message.channel);
        channel.AddMessage(message.ToString());
    }
        

    #region 채팅 이벤트 인터페이스


    // 서버에서 오는 디버깅 메시지 (레벨 별로 구분)
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

    // 클라이언트가 포톤 서버에 연결을 성공했다. (아직 채널에 들어가지 않았다.)
    public void OnConnected()
    {
        // OnAddChat("채팅 서버 접속을 성공했습니다.");
        onConnected?.Invoke();
        onConnected = null;
    }
    public void OnDisconnected()
    {
        Debug.Log("서버와의 접속이 끊어졌다.");
    }


    // 전체 메세지 수신.
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            // 수신 함수 호출.
            OnReceivedMessage(messages[i].ToString());
        }
    }
    // 귓속말 수신.
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        
    }

    // 채널 입장을 시도했고 그에 따른 결과.
    public void OnSubscribed(string[] channels, bool[] results)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            string name = channels[i];
            bool result = results[i];

            // 구독(접속) 실패의 경우 해당 루프는 실행하지 않는다.
            // 그리고 가상의 채널은 삭제한다.
            if (!result)
            {
                Channel.RemoveChannel(name);
                // 추가로 UI에게 채널 버튼을 삭제하라고 알림.
                continue;
            }

            // (포톤)채팅 채널 정보
            ChatChannel chatChannel = null;
            client.TryGetChannel(name, out chatChannel);

            // 해당하는 채널에 성공 메세지와 (포톤)채팅 채널 정보 연결.
            Channel channel = Channel.GetChannel(name);
            channel.AddMessage("채널 입장 성공!");
            channel.LinkedChatChannel(chatChannel);
        }
    }

    // 채널에서 나갔다.
    public void OnUnsubscribed(string[] channels)
    {
        foreach (string channel in channels)
        {

        }
    }

    // 유저가 새로 들어왔다.
    public void OnUserSubscribed(string channelName, string user)
    {
        // 해당하는 채널에 유저를 추가시킨다.
        Channel channel = Channel.GetChannel(channelName);
        channel?.AddUser(user);
    }
    public void OnUserUnsubscribed(string channelName, string user)
    {
        // 해당하는 채널에서 유저를 제거시킨다.
        Channel channel = Channel.GetChannel(channelName);
        channel?.RemoveUser(user);
    }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        ChatUser.STATUS userStatus = (ChatUser.STATUS)status;       // 상태.
        string[] channels = Channel.GetAllConnectedChannel(user);   // 연결 중인 채널 배열.

        foreach (string channel in channels)
            Channel.GetChannel(channel).UpdateUserStatus(user, userStatus);

        /*
         * 유저의 상태 ChatUserStatus
         * 1. offline : 오프라인 상태.
         * 2. Invisible : 숨김 (오프라인 상태)
         * 3. Online : 온라인 상태.
         * 4. Away : 자리비움.
         * 5. DND (Do not disturb) : 방해하지 마세요.
         * 6. LGF (Looking for game) : 파티/그룹/게임 찾는 중...
         * 7. Playing : 게임중..
         */
    }

    #endregion
}
