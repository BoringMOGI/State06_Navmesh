using ChatNetwork;
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

    private static System.Action<string> onSwitchButton;    // 버튼 스위칭 이벤트.
    private static string FORMAT = "{0} [{1}]";             // 출력 포멧.

    private string channelName;     // 채널 이름.
    private bool isSelected;        // 선택 중인가?
    
    public void Setup(string channelName, int count, bool isSelected = false)
    {
        this.channelName = channelName;     // 채널 이름 대입.
        this.isSelected = isSelected;       // 초기 선택 상태.
        onSwitchButton += OnSwitchButton;   // 이벤트 등록.

        UpdateUserCount(count);             // 네임 필드 세팅.

        // 초기 선택 상태라면 선택한다!
        if(isSelected)
            OnSeletecd();

        // 백그라운드 초기 색상.
        backImage.color = isSelected ? selectColor : deselectColor;
    }
    private void OnDestroy()
    {
        onSwitchButton -= OnSwitchButton;   // 이벤트 제거.
    }

    // 버튼이 선택이 되면...
    private void OnSeletecd()
    {
        onSwitchButton?.Invoke(channelName);            // 모든 버튼 동기화.
        Channel.GetChannel(channelName).Select();       // 채널에게 채널 변경 요청.
    }
    private void OnSwitchButton(string selectedChannel)
    {
        isSelected = string.Equals(channelName, selectedChannel);       // 선택된 채널명과 내 채널명이 같을 경우.
        backImage.color = isSelected ? selectColor : deselectColor;     // 선택 상태에 따라 컬러 변경.
    }
    public void UpdateUserCount(int count)
    {
        nameText.text = string.Format(FORMAT, channelName, count);
    }

    // 클릭 이벤트 인터페이스.
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("선택!");

        OnSeletecd();
    }
}
