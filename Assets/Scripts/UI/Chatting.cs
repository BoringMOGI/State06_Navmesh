using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Text;


public class Chatting : MonoBehaviour
{
    private struct ChatData
    {
        static string FORMAT = "[{0}:{1}] <color=#{2}>{3} : </color>{4}";

        public string nickName;     // 닉네임.
        public string message;      // 채팅 내용.
        public int time;            // 입력 시간.
        public Color textColor;     // 닉네임 색상.

        public override string ToString()
        {
            TimeSpan span = TimeSpan.FromSeconds(time);
            return string.Format(FORMAT,
                span.Minutes.ToString("00"),
                span.Seconds.ToString("00"),
                ColorUtility.ToHtmlStringRGB(textColor),
                nickName,
                message);
        }
    }

    [SerializeField] TMP_Text textField;            // 입력된 채팅이 들어갈 필드.
    [SerializeField] TMP_InputField inputField;     // 나의 입력 필드.
    [SerializeField] Color nameColor;               // 닉네임 색상.
    [SerializeField] int limitLine;                 // 최대 입력 라인 수.

    RectTransform textFieldRect;                    // 텍스트 필드의 사각 트랜스폼.
    Queue<ChatData> chatting;                       // 저장되어있는 채팅 데이터.
    StringBuilder builder;                          // 스트링 빌더.

    string nickname = "테스터AB";
    
    private void Start()
    {
        textField.text = string.Empty;
        inputField.text = string.Empty;

        textFieldRect = textField.rectTransform;
        chatting = new Queue<ChatData>();
        builder = new StringBuilder();

        inputField.onEndEdit.AddListener(OnChat);
        
    }

    private void Update()
    {
        // 엔터키를 눌렀을 때 입력 필드가 선택되어있지 않았다면...
        if(Input.GetKeyDown(KeyCode.Return) && !inputField.isFocused)
        {
            inputField.Select();
        }
    }

    private void OnChat(string str)
    {
        // 입력한 텍스트가 비어있다면 실행하지 않는다.
        if (string.IsNullOrEmpty(str))
            return;

        // 입력 필드의 값을 지워버린다.
        inputField.text = string.Empty;

        // 채팅은 순서대로 저장하되 20개가 넘으면 가장 오래된 채팅을 지운다.
        chatting.Enqueue(new ChatData() { 
            nickName = nickname,
            message = str,
            textColor = nameColor,
            time = Mathf.FloorToInt(Time.time)
        });

        // 저장된 데이터가 limitLine을 넘으면 가장 오래된 데이터 제거.
        if (chatting.Count > limitLine)
            chatting.Dequeue();

        // 입력 데이터를 기준으로 채팅 텍스트 변환.
        builder.Clear();
        textField.text = String.Join('\n', chatting);

        // 텍스트 필드의 크기와 위치를 재조정 한다.
        textFieldRect.sizeDelta = new Vector2(textFieldRect.sizeDelta.x, textField.preferredHeight);
        textFieldRect.localPosition = Vector3.zero;

        // 다시 재입력 할 수 있도록 활성화 해준다.
        // 최초에 Select를 호출하면 이벤트 시스템에 선택되고 자체적으로 Initializer를 불러 Activate한다.
        // 여기서 Enter를 치면 포커싱이 풀리는 것이 아니라 Deactivate된다.
        //inputField.Select();
        inputField.ActivateInputField();
    }

}
