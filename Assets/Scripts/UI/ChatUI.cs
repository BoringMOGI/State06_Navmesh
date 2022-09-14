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

    [Header("Channel")]
    [SerializeField] ChannelButton channelPrefab;   // ä�� ������.
    [SerializeField] Transform channelParent;       // ä�� ������Ʈ�� �θ� ������Ʈ.

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

        // �⺻������ �����ؾ��ϴ� Local ä�� ����.
        OnAddChannel("Local", true);
    }
    private void Update()
    {
        // ����Ű�� ������ �� �Է� �ʵ尡 ���õǾ����� �ʾҴٸ�...
        if (Input.GetKeyDown(KeyCode.Return) && !inputField.isFocused)
        {
            inputField.Select();
        }

        // ���ο� �޽����� �Դ��� üũ.
        if(Channel.Current != null && Channel.Current.IsNewMessage)
        {
            // �ؽ�Ʈ �ʵ忡 ä���� �ؽ�Ʈ�� ������ ���� �Ѵ�.
            textField.text = Channel.Current.GetAllMessage();

            // �ؽ�Ʈ �ʵ��� ũ��� ��ġ�� ������ �Ѵ�.
            textFieldRect.sizeDelta = new Vector2(textFieldRect.sizeDelta.x, textField.preferredHeight);
            textFieldRect.localPosition = Vector3.zero;
        }
    }

    // '����' ä���� �Է����� �� �Ҹ��� �̺�Ʈ �Լ�.
    private void OnEndEdit(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        // �Է� �ʵ��� ���� ����������.
        inputField.text = string.Empty;

        // �Է��� ���ڿ��� �޼��� ��ü�� ���� ä�� ������ ������.
        Channel current = Channel.Current;
        server.OnSendMessage(new ChatMessage(current.Name, ChatServer.UserID, str, userName, job));

        // �ٽ� ���Է� �� �� �ֵ��� Ȱ��ȭ ���ش�.
        // ���ʿ� Select�� ȣ���ϸ� �̺�Ʈ �ý��ۿ� ���õǰ� ��ü������ Initializer�� �ҷ� Activate�Ѵ�.
        // ���⼭ Enter�� ġ�� ��Ŀ���� Ǯ���� ���� �ƴ϶� Deactivate�ȴ�.
        //inputField.Select();
        inputField.ActivateInputField();
    }        
    private void OnAddChannel(string channelName, bool isDefault = false)
    {
        ChannelButton button = Instantiate(channelPrefab, channelParent);   // ���ο� ä�� �߰�.
        button.transform.SetSiblingIndex(channelParent.childCount - 2);     // �׻� ������ �� index�� �߰�.
        button.Setup(channelName, 0, isDefault);                            // ä�� ��ư ����.
    }

    public void OnClickAddChannel()
    {
        InputPopup.Instance.Show("ä�θ��� �Է��ϼ���", (channelName, isConfirm) =>
        {
            if (!isConfirm)
                return;

            // ä�θ��� �ߺ��ȴ�. ��� ȣ��.
            if (Channel.IsContains(channelName))
            {
                OnClickAddChannel();                    // ä�� �ߺ�. ��� ȣ��.
            }
            else
            {
                OnAddChannel(channelName);              // ä�� ��ư �߰�.
                server.ConnectToChannel(channelName);   // ä�� ����.
            }
        });
    }
}
