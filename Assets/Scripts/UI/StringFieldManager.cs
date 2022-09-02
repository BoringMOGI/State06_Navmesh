using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StringFieldManager : MonoBehaviour
{
    static StringFieldManager instance;
    public static StringFieldManager Instance=> instance;

    [SerializeField] NameField nameFieldPrefab;
    [SerializeField] SpeechBubble speechPrefab;

    private void Awake()
    {
        instance = this;
    }

    public void RegestedName(INameField target)
    {
        NameField nameField = Instantiate(nameFieldPrefab, transform);
        nameField.Setup(target);
    }
    public void ShowSpeechBubble(INameField nameField, Transform pivot, string talk, float showTime)
    {
        SpeechBubble speechBubble = Instantiate(speechPrefab, transform);
        speechBubble.Speech(nameField, pivot, talk, showTime);
    }
}


// 확장 메소드 구현.
public static class Method
{
    public static void Fit(this Text target)
    {
        float width = target.preferredWidth;
        float height = target.preferredHeight;

        target.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    }
}
    