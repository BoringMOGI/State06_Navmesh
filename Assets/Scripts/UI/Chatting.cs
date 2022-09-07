using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

using Photon.Pun;               // Pun(Photon unity network) : 서버 접속용
using Photon.Chat;              // 채팅 관련 네임스페이스.
using ExitGames.Client.Photon;  // 클라이언트 생성용.


public partial class Chatting : MonoBehaviour
{
    [SerializeField] TMP_Text textField;            // 입력된 채팅이 들어갈 필드.
    [SerializeField] TMP_InputField inputField;     // 나의 입력 필드.
    [SerializeField] Color nameColor;               // 닉네임 색상.
    [SerializeField] int limitLine;                 // 최대 입력 라인 수.

    RectTransform textFieldRect;                    // 텍스트 필드의 사각 트랜스폼.
    Queue<string> records;                          // 저장되어있는 채팅 데이터.

    string userName = "테스터AB";
    string job = "가렌";
    
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
        // 엔터키를 눌렀을 때 입력 필드가 선택되어있지 않았다면...
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
        DateTime time = DateTime.Now;       // 현재 시간 받기.

        return string.Format(FORMAT,
            time.Hour.ToString("00"),
            time.Minute.ToString("00"),
            ColorUtility.ToHtmlStringRGB(nameColor),
            name,
            job,
            msg);
    }

    // '내가' 채팅을 입력했을 때 불리는 이벤트 함수.
    private void OnEndEdit(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        // 입력 필드의 값을 지워버린다.
        inputField.text = string.Empty;

        // 메세지를 채팅 서버로 보낸다.
        OnSendMessage(new Message(userName, job, str));

        // 다시 재입력 할 수 있도록 활성화 해준다.
        // 최초에 Select를 호출하면 이벤트 시스템에 선택되고 자체적으로 Initializer를 불러 Activate한다.
        // 여기서 Enter를 치면 포커싱이 풀리는 것이 아니라 Deactivate된다.
        //inputField.Select();
        inputField.ActivateInputField();
    }

    // 텍스트 뷰에 채팅을 추가한다.
    private void OnAddChat(string str)
    {
        // 입력한 텍스트가 비어있다면 실행하지 않는다.
        if (string.IsNullOrEmpty(str))
            return;

        // 채팅은 순서대로 저장하되 20개가 넘으면 가장 오래된 채팅을 지운다.
        records.Enqueue(str);

        // 저장된 데이터가 limitLine을 넘으면 가장 오래된 데이터 제거.
        if (records.Count > limitLine)
            records.Dequeue();

        // 입력 데이터를 기준으로 채팅 텍스트 변환.
        textField.text = String.Join('\n', records);

        // 텍스트 필드의 크기와 위치를 재조정 한다.
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

    private ChatClient client;      // 채팅 클라이언트(서버와 연결된 나)

    // 서버에 접속하겠다.
    [ContextMenu("연결")]
    private void ConnectToServer()
    {
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

        Debug.Log("서버 접속 시도중..");
    }
    private void OnUpdate()
    {
        // 클라이언트의 교신.
        // 클라이언트는 서버와의 연결을 유지하고 들어오는 메시지를 처리하기 위해서 정기적으로 호출.
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
            Debug.Log("메세지를 보낼 수 없습니다.");
        }
    }
    private void OnReceivedMessage(string channel, Message message)
    {
        OnAddChat(ConvertChat(message));
    }



    #region 채팅 이벤트 인터페이스


    // 서버에서 오는 디버깅 메시지 (레벨 별로 구분)
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

    // 서버에 접속을 성공했다.
    public void OnConnected()
    {
        OnAddChat("채팅 서버 접속을 성공했습니다.");

        // 마스터(채텅) 서버와의 연결이 성공적으로 이루어졌으니
        // 실제 메시지가 오가는 채널에 입장한다(구독한다)
        ChannelCreationOptions option = new ChannelCreationOptions() { PublishSubscribers = true };
        client.Subscribe(channelName, messagesFromHistory:0, creationOptions:option);
    }

    public void OnDisconnected()
    {
        Debug.Log("서버와의 접속이 끊어졌다.");
    }


    // 전체 메세지 수신.
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for(int i= 0; i<senders.Length; i++)
        {
            // json형태로 수신된 문자열을 Message객체로 convert시킨다.
            string json = messages[i].ToString();
            Debug.Log(json);
            Message message = JsonUtility.FromJson<Message>(json);
            OnReceivedMessage(channelName, message);
        }
    }
    // 귓속말 수신.
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        throw new NotImplementedException();
    }


    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new NotImplementedException();
    }

    // 채널 입장을 시도했고 그에 따른 결과.
    public void OnSubscribed(string[] channels, bool[] results)
    {
        for(int i = 0; i<channels.Length; i++)
        {
            if(results[i])
            {
                OnAddChat($"{channels[i]} 채널 입장 성공!");
            }
            else
            {
                OnAddChat($"{channels[i]} 채널 입장 실패..");
            }
        }
    }
    // 채널에서 나갔다.
    public void OnUnsubscribed(string[] channels)
    {
        foreach (string channel in channels)
            OnAddChat($"{channel} 채널에서 퇴장했습니다.");
    }

    public void OnUserSubscribed(string channel, string user)
    {
        Debug.Log("유저입장 : " + user);
        OnAddChat($"{user}가 입장했습니다!");
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        OnAddChat($"{user}가 퇴장했습니다!");
    }

    #endregion
}

