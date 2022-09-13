using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

using ChatNetwork;

[RequireComponent(typeof(ChatServer))]
public class ChatUI : MonoBehaviour
{

    [SerializeField] TMP_Text textField;            // �Էµ� ä���� �� �ʵ�.
    [SerializeField] TMP_InputField inputField;     // ���� �Է� �ʵ�.
    [SerializeField] Color nameColor;               // �г��� ����.
    [SerializeField] int limitLine;                 // �ִ� �Է� ���� ��.

    RectTransform textFieldRect;                    // �ؽ�Ʈ �ʵ��� �簢 Ʈ������.
    ChatServer server;

    string userName = "�׽���AB";
    string job = "����";

    private void Start()
    {
        textField.text = string.Empty;
        inputField.text = string.Empty;

        textFieldRect = textField.rectTransform;

        inputField.onEndEdit.AddListener(OnEndEdit);

        // ������ ���� �õ�!!
        server = GetComponent<ChatServer>();
        server.ConnectToServer(userName);
    }
    private void Update()
    {
        // ����Ű�� ������ �� �Է� �ʵ尡 ���õǾ����� �ʾҴٸ�...
        if (Input.GetKeyDown(KeyCode.Return) && !inputField.isFocused)
        {
            inputField.Select();
        }
    }

    // '����' ä���� �Է����� �� �Ҹ��� �̺�Ʈ �Լ�.
    private void OnEndEdit(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        // �Է� �ʵ��� ���� ����������.
        inputField.text = string.Empty;

        // �޼����� ä�� ������ ������.
        server.OnSendMessage(new ChatMessage() { 
            channel = server.currentChannelName,
            userName = userName,
            job = job,
            msg = str 
        });
        
      

        // �ٽ� ���Է� �� �� �ֵ��� Ȱ��ȭ ���ش�.
        // ���ʿ� Select�� ȣ���ϸ� �̺�Ʈ �ý��ۿ� ���õǰ� ��ü������ Initializer�� �ҷ� Activate�Ѵ�.
        // ���⼭ Enter�� ġ�� ��Ŀ���� Ǯ���� ���� �ƴ϶� Deactivate�ȴ�.
        //inputField.Select();
        inputField.ActivateInputField();
    }

    // �ؽ�Ʈ �信 ä���� �߰��Ѵ�.
    public void OnUpdateChatView(string str)
    {
        // �ؽ�Ʈ �ʵ忡 �Ű����� str�� ���� �����Ѵ�.
        textField.text = str;

        // �ؽ�Ʈ �ʵ��� ũ��� ��ġ�� ������ �Ѵ�.
        textFieldRect.sizeDelta = new Vector2(textFieldRect.sizeDelta.x, textField.preferredHeight);
        textFieldRect.localPosition = Vector3.zero;
    }
}
