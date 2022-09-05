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

        public string nickName;     // �г���.
        public string message;      // ä�� ����.
        public int time;            // �Է� �ð�.
        public Color textColor;     // �г��� ����.

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

    [SerializeField] TMP_Text textField;            // �Էµ� ä���� �� �ʵ�.
    [SerializeField] TMP_InputField inputField;     // ���� �Է� �ʵ�.
    [SerializeField] Color nameColor;               // �г��� ����.
    [SerializeField] int limitLine;                 // �ִ� �Է� ���� ��.

    RectTransform textFieldRect;                    // �ؽ�Ʈ �ʵ��� �簢 Ʈ������.
    Queue<ChatData> chatting;                       // ����Ǿ��ִ� ä�� ������.
    StringBuilder builder;                          // ��Ʈ�� ����.

    string nickname = "�׽���AB";
    
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
        // ����Ű�� ������ �� �Է� �ʵ尡 ���õǾ����� �ʾҴٸ�...
        if(Input.GetKeyDown(KeyCode.Return) && !inputField.isFocused)
        {
            inputField.Select();
        }
    }

    private void OnChat(string str)
    {
        // �Է��� �ؽ�Ʈ�� ����ִٸ� �������� �ʴ´�.
        if (string.IsNullOrEmpty(str))
            return;

        // �Է� �ʵ��� ���� ����������.
        inputField.text = string.Empty;

        // ä���� ������� �����ϵ� 20���� ������ ���� ������ ä���� �����.
        chatting.Enqueue(new ChatData() { 
            nickName = nickname,
            message = str,
            textColor = nameColor,
            time = Mathf.FloorToInt(Time.time)
        });

        // ����� �����Ͱ� limitLine�� ������ ���� ������ ������ ����.
        if (chatting.Count > limitLine)
            chatting.Dequeue();

        // �Է� �����͸� �������� ä�� �ؽ�Ʈ ��ȯ.
        builder.Clear();
        textField.text = String.Join('\n', chatting);

        // �ؽ�Ʈ �ʵ��� ũ��� ��ġ�� ������ �Ѵ�.
        textFieldRect.sizeDelta = new Vector2(textFieldRect.sizeDelta.x, textField.preferredHeight);
        textFieldRect.localPosition = Vector3.zero;

        // �ٽ� ���Է� �� �� �ֵ��� Ȱ��ȭ ���ش�.
        // ���ʿ� Select�� ȣ���ϸ� �̺�Ʈ �ý��ۿ� ���õǰ� ��ü������ Initializer�� �ҷ� Activate�Ѵ�.
        // ���⼭ Enter�� ġ�� ��Ŀ���� Ǯ���� ���� �ƴ϶� Deactivate�ȴ�.
        //inputField.Select();
        inputField.ActivateInputField();
    }

}
