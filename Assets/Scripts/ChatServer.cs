using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;               // Pun(Photon unity network) : 서버 접속용
using Photon.Chat;              // 채팅 관련 네임스페이스.
using ExitGames.Client.Photon;  // 클라이언트 생성용.
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
            System.DateTime time = System.DateTime.Now;       // 현재 시간 받기.

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
    #region 변수

    public class Channel
    {
        static int MAX_CHAT_COUNT = 30;

        ChatChannel info;          // 채팅 채널 정보.
        Queue<string> records;     // 수신 메시지.

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
            // 메시지를 추가한다. 최대 개수를 넘으면 가장 마지막 메시지를 지운다.
            records.Enqueue(message);
            if (records.Count > MAX_CHAT_COUNT)
                records.Dequeue();
        }
    }

    // 멤버 변수.
    Dictionary<string, Channel> channelList;    // 내가 구독중인 채널 리스트.
    ChatClient client;                          // 채팅 클라이언트(서버와 연결된 나)
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
        // 클라이언트의 교신.
        // 클라이언트는 서버와의 연결을 유지하고 들어오는 메시지를 처리하기 위해서 정기적으로 호출.
        if (client != null)
            client.Service();
    }

    // 서버에 접속하겠다.
    public void ConnectToServer(string userName)
    {
        Debug.Log("포톤 서버에 접속 시도...");

        // 서버와 비연결된 상태가 아니라면 리턴한다.
        if (client != null && client.State != ChatState.Uninitialized)
            return;

        // 채널 리스트 객체 생성.
        channelList = new Dictionary<string, Channel>();

        // 백그라운드 상태가 되면 기본적으로 '일시정지' 상태가 된다.
        // 그렇게되면 채팅 서버와의 연결이 끊어진다.
        Application.runInBackground = true;
        client = new ChatClient(this);                  // 클라이언트 생성.
        client.UseBackgroundWorkerForSending = true;    // 백그라운드 상태에서도 데이터 전송.                

        string chatId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat;
        string appVersion = "1.0";

        // 인증서 : 채팅 서버 내에서 나를 의미(식별)하는 고유한 문자열.
        AuthenticationValues authValues = new AuthenticationValues(userName);

        // 클라이언트가 chatId서버에 인증서를 들고 연결을 시도한다.
        client.Connect(chatId, appVersion, authValues);
    }

    // 채널에 메시지를 송신한다.
    public void OnSendMessage(Message message)
    {
        if (client == null)
            return;

        string json = JsonUtility.ToJson(message);
        if (!client.PublishMessage(currentChannelName, json))
            Debug.Log("메세지를 보낼 수 없습니다.");
    }

    // 채널로부터 메시지를 수신한다.
    private void OnReceivedMessage(Message message)
    {
        Channel channel = channelList[message.channel];

        // 수신한 메시지를 텍스트(string)형식으로 변경해 채널에 추가.
        channel.AddMessage(message.ToString());

        // 만약 내가 포커싱 하고 있는 채널과 수신된 메시지의 채널이 같을 경우.
        // 등록되어있는 리스너들에게 알려준다.
        if(message.channel == currentChannelName)
        {
            // chatUI에게 메시지 전달.
            chatUI.OnUpdateChatView(channel.AllMessage);
        }
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
        // 기본 채널 이름 (Default)
        currentChannelName = "Local";

        // 마스터(채텅) 서버와의 연결이 성공적으로 이루어졌으니
        // 실제 메시지가 오가는 채널에 입장한다(구독한다)
        ChannelCreationOptions option = new ChannelCreationOptions() { PublishSubscribers = true };
        client.Subscribe(currentChannelName, messagesFromHistory: 0, creationOptions: option);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 300), "AAA", new GUIStyle() { fontSize = 40 });
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
            // json형태로 수신된 문자열을 Message객체로 convert시킨다.
            string json = messages[i].ToString();
            Message message = JsonUtility.FromJson<Message>(json);

            // 수신 함수 호출.
            OnReceivedMessage(message);
        }
    }
    // 귓속말 수신.
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        
    }


    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        
    }

    // 채널 입장을 시도했고 그에 따른 결과.
    public void OnSubscribed(string[] channels, bool[] results)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            // 구독(접속) 실패의 경우 해당 루프는 실행하지 않는다.
            if (!results[i])
                continue;

            ChatChannel channel = null;

            // 구독에 성공한 chennels[i]의 채널 데이터를 달라.
            if (!client.TryGetChannel(channels[i], out channel))
                continue;

            // 받은 채널 데이터를 우리의 데이터 'Channel'로 변경 후 리스트에 추가.
            channelList.Add(channels[i], new Channel(channel));
        }
    }
    // 채널에서 나갔다.
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
