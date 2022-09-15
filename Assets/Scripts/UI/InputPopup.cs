using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputPopup : InputHandler
{
    [SerializeField] GameObject panel;
    [SerializeField] Text titleText;
    [SerializeField] InputField inputField;
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;

    static InputPopup instance;
    public static InputPopup Instance => instance;

    private System.Action<string, bool> callback;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        // 스크립트에서 버튼의 onClick 이벤트에 등록하는 방법.
        //confirmButton.onClick.AddListener(Confirm);
        //cancelButton.onClick.AddListener(Cancel);

        panel.SetActive(false);                
    }

    public void Show(string title, System.Action<string, bool> callback)
    {
        this.callback = callback;

        panel.SetActive(true);              // 팝업 패널 활성화.

        titleText.text = title;             // 타이틀(지시문) 텍스트 입력.
        inputField.text = string.Empty;     // 입력 필드 초기화.
        inputField.Select();                // 입력 필드 초기 선택.

        // 이벤트 등록.
        AddInherentOwner();
        AddEvent(KeyCode.Return, Confirm);
        AddEvent(KeyCode.Escape, Cancel);
    }
    private void Close()
    {
        callback = null;
        panel.SetActive(false);

        // 이벤트 해제.
        RemoveInherentOwner();
        RemoveEvent(KeyCode.Return);
        RemoveEvent(KeyCode.Escape);
    }

    public void Confirm()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            Debug.Log("값을 입력해야합니다!!");
            return;
        }

        callback?.Invoke(inputField.text, true);
        Close();
    }
    public void Cancel()
    {
        callback?.Invoke(string.Empty, false);
        Close();
    }

   
}
