using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputPopup : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Text titleText;
    [SerializeField] InputField inputField;
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;

    static InputPopup instance;
    public static InputPopup Instance => instance;

    private System.Action<string> callback;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        // ��ũ��Ʈ���� ��ư�� onClick �̺�Ʈ�� ����ϴ� ���.
        //confirmButton.onClick.AddListener(Confirm);
        //cancelButton.onClick.AddListener(Cancel);

        panel.SetActive(false);
    }

    public void Show(string title, System.Action<string> callback)
    {
        this.callback = callback;

        panel.SetActive(true);              // �˾� �г� Ȱ��ȭ.

        titleText.text = title;             // Ÿ��Ʋ(���ù�) �ؽ�Ʈ �Է�.
        inputField.text = string.Empty;     // �Է� �ʵ� �ʱ�ȭ.
        inputField.Select();                // �Է� �ʵ� �ʱ� ����.
    }

    public void Confirm()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            Debug.Log("���� �Է��ؾ��մϴ�!!");
            return;
        }

        callback?.Invoke(inputField.text);
        callback = null;
        panel.SetActive(false);
    }
    public void Cancel()
    {
        callback = null;
        panel.SetActive(false);
    }
}
