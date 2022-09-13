using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChannelButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image backImage;
    [SerializeField] Text nameText;
    [SerializeField] Color selectColor;
    [SerializeField] Color deselectColor;

    private static System.Action<string> onSwitchButton;    // ��ư ����Ī �̺�Ʈ.
    private static string FORMAT = "{0} [{1}]";             // ��� ����.

    private string channelName;     // ä�� �̸�.
    private bool isSelected;        // ���� ���ΰ�?
    
    public void Setup(string channelName, int count, bool isSelected = false)
    {
        this.channelName = channelName;     // ä�� �̸� ����.
        this.isSelected = isSelected;       // �ʱ� ���� ����.
        onSwitchButton += OnSwitchButton;   // �̺�Ʈ ���.

        UpdateUserCount(count);             // ���� �ʵ� ����.

        // ��׶��� �ʱ� ����.
        backImage.color = isSelected ? selectColor : deselectColor;
    }
    private void OnDestroy()
    {
        onSwitchButton -= OnSwitchButton;   // �̺�Ʈ ����.
    }

    private void OnSwitchButton(string selectedChannel)
    {
        isSelected = string.Equals(channelName, selectedChannel);       // ���õ� ä�θ��� �� ä�θ��� ���� ���.
        backImage.color = isSelected ? selectColor : deselectColor;     // ���� ���¿� ���� �÷� ����.
    }
    private void OnSeletecd()
    {
        if (isSelected)
            return;

        onSwitchButton?.Invoke(channelName);
    }
    public void UpdateUserCount(int count)
    {
        nameText.text = string.Format(FORMAT, channelName, count);
    }



    // Ŭ�� �̺�Ʈ �������̽�.
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("����!");

        OnSeletecd();
    }
}