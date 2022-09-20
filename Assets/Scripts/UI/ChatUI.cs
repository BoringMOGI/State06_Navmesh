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
    [SerializeField] TMP_Text textField;            // �Էµ� ä���� �� �ʵ�.
    [SerializeField] TMP_InputField inputField;     // ���� �Է� �ʵ�.
    [SerializeField] Color nameColor;               // �г��� ����.
    [SerializeField] int limitLine;                 // �ִ� �Է� ���� ��.

    [Header("Channel")]
    [SerializeField] ChannelButton channelPrefab;   // ä�� ������.
    [SerializeField] Transform channelParent;       // ä�� ������Ʈ�� �θ� ������Ʈ.

    [Header("User")]
    [SerializeField] ChatUserUI userPrefab;
    [SerializeField] Transform userParent;

    protected RectTransform textFieldRect;                    // �ؽ�Ʈ �ʵ��� �簢 Ʈ������.
    protected ChatServer server;

    protected string userName = "�׽���AB";
    protected string job = "����";

    void Start()
    {
        textField.text = string.Empty;
        inputField.text = string.Empty;

        textFieldRect = textField.rectTransform;
        server = GetComponent<ChatServer>();

        // �̺�Ʈ ���
        inputField.onSubmit.AddListener(OnEndEdit);
        AddEvent(KeyCode.Return, OnSelectInputField);

        ConnectToChatServer();
    }

    protected virtual void ConnectToChatServer()
    {
        // ������ ���� �õ�!!
        server.ConnectToServer(userName, () => {

            // ������ ������ �Ϸ�Ǹ� Localä���� �߰��Ѵ�.
            OnAddChannel("Local", true);
        });
    }

    private void Update()
    {
        if (Channel.Current == null)
            return;

        // ���ο� �޽����� �Դ��� üũ.
        if(Channel.Current.IsNewMessage)
        {
            // �ؽ�Ʈ �ʵ忡 ä���� �ؽ�Ʈ�� ������ ���� �Ѵ�.
            textField.text = Channel.Current.GetAllMessage();

            // �ؽ�Ʈ �ʵ��� ũ��� ��ġ�� ������ �Ѵ�.
            textFieldRect.sizeDelta = new Vector2(textFieldRect.sizeDelta.x, textField.preferredHeight);
            //textFieldRect.localPosition = Vector3.zero;
        }
        if(Channel.Current.IsUpdateUser)
        {

        }
    }

    private void OnSelectInputField()
    {
        // ����Ű�� ������ �� �Է� �ʵ尡 ���õǾ����� �ʾҴٸ�...
        if (!inputField.isFocused)
            inputField.Select();
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

    // ä���� ���� �߰��Ѵ�.
    public void OnAddChannel(string channelName, bool isDefault = false)
    {
        server.ConnectToChannel(channelName, true);                         // ä�� ���� (������ �߰�)

        ChannelButton button = Instantiate(channelPrefab, channelParent);   // ���ο� ä�� �߰�.
        button.transform.SetSiblingIndex(channelParent.childCount - 2);     // �׻� ������ �� index�� �߰�.
        button.Setup(channelName, 0, isDefault);                            // ä�� ��ư ����.        
    }

    // ä�� �߰� ��ư : ä�θ��� �������� �Է� �޾ƿ´�.
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
                OnAddChannel(channelName, true);              // ä�� ��ư �߰�.
            }
        });
    }
}
