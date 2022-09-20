using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

using ChatNetwork;
using ChatNetwork.Message;

[RequireComponent(typeof(ChatServer))]
public class ChatUI : InputHandler
{
    [SerializeField] TMP_Text textField;            // 입력된 채팅이 들어갈 필드.
    [SerializeField] TMP_InputField inputField;     // 나의 입력 필드.
    [SerializeField] Color nameColor;               // 닉네임 색상.
    [SerializeField] int limitLine;                 // 최대 입력 라인 수.

    [Header("Channel")]
    [SerializeField] ChannelButton channelPrefab;   // 채널 프리팹.
    [SerializeField] Transform channelParent;       // 채널 오브젝트의 부모 오브젝트.

    [Header("User")]
    [SerializeField] ChatUserUI userPrefab;
    [SerializeField] Transform userParent;

    protected RectTransform textFieldRect;                    // 텍스트 필드의 사각 트랜스폼.
    protected ChatServer server;

    protected string userName = "테스터AB";
    protected string job = "가렌";

    void Start()
    {
        textField.text = string.Empty;
        inputField.text = string.Empty;

        textFieldRect = textField.rectTransform;
        server = GetComponent<ChatServer>();

        // 이벤트 등록
        inputField.onSubmit.AddListener(OnEndEdit);
        AddEvent(KeyCode.Return, OnSelectInputField);

        ConnectToChatServer();
    }

    protected virtual void ConnectToChatServer()
    {
        // 서버에 접속 시도!!
        server.ConnectToServer(userName, () => {

            // 서버에 접속이 완료되면 Local채널을 추가한다.
            OnAddChannel("Local", true);
        });
    }

    private void Update()
    {
        if (Channel.Current == null)
            return;

        // 새로운 메시지가 왔는지 체크.
        if(Channel.Current.IsNewMessage)
        {
            // 텍스트 필드에 채널의 텍스트를 가져와 대입 한다.
            textField.text = Channel.Current.GetAllMessage();

            // 텍스트 필드의 크기와 위치를 재조정 한다.
            textFieldRect.sizeDelta = new Vector2(textFieldRect.sizeDelta.x, textField.preferredHeight);
            //textFieldRect.localPosition = Vector3.zero;
        }
        if(Channel.Current.IsUpdateUser)
        {

        }
    }

    private void OnSelectInputField()
    {
        // 엔터키를 눌렀을 때 입력 필드가 선택되어있지 않았다면...
        if (!inputField.isFocused)
            inputField.Select();
    }


    // '내가' 채팅을 입력했을 때 불리는 이벤트 함수.
    private void OnEndEdit(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        // 입력 필드의 값을 지워버린다.
        inputField.text = string.Empty;

        // 입력한 문자열을 메세지 객체로 만들어서 채팅 서버로 보낸다.
        Channel current = Channel.Current;
        server.OnSendMessage(new ChatMessage(current.Name, ChatServer.UserID, str, userName, job));

        // 다시 재입력 할 수 있도록 활성화 해준다.
        // 최초에 Select를 호출하면 이벤트 시스템에 선택되고 자체적으로 Initializer를 불러 Activate한다.
        // 여기서 Enter를 치면 포커싱이 풀리는 것이 아니라 Deactivate된다.
        //inputField.Select();
        inputField.ActivateInputField();
    }        

    // 채널을 새로 추가한다.
    public void OnAddChannel(string channelName, bool isDefault = false)
    {
        server.ConnectToChannel(channelName, true);                         // 채널 입장 (데이터 추가)

        ChannelButton button = Instantiate(channelPrefab, channelParent);   // 새로운 채널 추가.
        button.transform.SetSiblingIndex(channelParent.childCount - 2);     // 항상 마지막 전 index로 추가.
        button.Setup(channelName, 0, isDefault);                            // 채널 버튼 생성.        
    }

    // 채널 추가 버튼 : 채널명을 유저에게 입력 받아온다.
    public void OnClickAddChannel()
    {
        InputPopup.Instance.Show("채널명을 입력하세요", (channelName, isConfirm) =>
        {
            if (!isConfirm)
                return;

            // 채널명이 중복된다. 재귀 호출.
            if (Channel.IsContains(channelName))
            {
                OnClickAddChannel();                    // 채널 중복. 재귀 호출.
            }
            else
            {
                OnAddChannel(channelName, true);              // 채널 버튼 추가.
            }
        });
    }
}
