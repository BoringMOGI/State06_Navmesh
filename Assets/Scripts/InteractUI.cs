using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class InteractUI : MonoBehaviour
{
    // Singleton Pattern.
    private static InteractUI instance;
    public static InteractUI Instance => instance;

    // Member Variable.
    [SerializeField] Text shortcutText;
    [SerializeField] Text contentText;

    CanvasGroup group;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        group = GetComponent<CanvasGroup>();
        CloseUI();
    }

    public void CloseUI()
    {
        group.alpha = 0f;
    }
    public void UpdateUI(IInteraction target)
    {
        group.alpha = 1f;

        shortcutText.text = target.Key.ToString();
        contentText.text = target.ActionName;

        // Fit width.
        RectTransform rect = contentText.rectTransform;
        rect.sizeDelta = new Vector2(contentText.preferredWidth, rect.sizeDelta.y);

        // target의 위치(월드 좌표)를 UI를 출력하기 위한 위치(스크린 좌표)로 맞춰주기 위해서.
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(target.UiPosition);
        transform.position = screenPoint;
    }

}
