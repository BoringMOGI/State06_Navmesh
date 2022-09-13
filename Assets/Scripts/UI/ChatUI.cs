using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

using ChatNetwork;

[RequireComponent(typeof(ChatServer))]
public class ChatUI : MonoBehaviour
{

    [SerializeField] TMP_Text textField;            // 입력된 채팅이 들어갈 필드.
    [SerializeField] TMP_InputField inputField;     // 나의 입력 필드.
    [SerializeField] Color nameColor;               // 닉네임 색상.
    [SerializeField] int limitLine;                 // 최대 입력 라인 수.

    RectTransform textFieldRect;                    // 텍스트 필드의 사각 트랜스폼.
    ChatServer server;

    string userName = "테스터AB";
    string job = "가렌";

    private void Start()
    {
        textField.text = string.Empty;
        inputField.text = string.Empty;

        textFieldRect = textField.rectTransform;

        inputField.onEndEdit.AddListener(OnEndEdit);

        // 서버에 접속 시도!!
        server = GetComponent<ChatServer>();
        server.ConnectToServer(userName);
    }
    private void Update()
    {
        // 엔터키를 눌렀을 때 입력 필드가 선택되어있지 않았다면...
        if (Input.GetKeyDown(KeyCode.Return) && !inputField.isFocused)
        {
            inputField.Select();
        }
    }

    // '내가' 채팅을 입력했을 때 불리는 이벤트 함수.
    private void OnEndEdit(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        // 입력 필드의 값을 지워버린다.
        inputField.text = string.Empty;

        // 메세지를 채팅 서버로 보낸다.
        server.OnSendMessage(new ChatMessage() { 
            channel = server.currentChannelName,
            userName = userName,
            job = job,
            msg = str 
        });
        
      

        // 다시 재입력 할 수 있도록 활성화 해준다.
        // 최초에 Select를 호출하면 이벤트 시스템에 선택되고 자체적으로 Initializer를 불러 Activate한다.
        // 여기서 Enter를 치면 포커싱이 풀리는 것이 아니라 Deactivate된다.
        //inputField.Select();
        inputField.ActivateInputField();
    }

    // 텍스트 뷰에 채팅을 추가한다.
    public void OnUpdateChatView(string str)
    {
        // 텍스트 필드에 매개변수 str의 값을 대입한다.
        textField.text = str;

        // 텍스트 필드의 크기와 위치를 재조정 한다.
        textFieldRect.sizeDelta = new Vector2(textFieldRect.sizeDelta.x, textField.preferredHeight);
        textFieldRect.localPosition = Vector3.zero;
    }
}
