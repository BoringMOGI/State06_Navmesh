using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwitchButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image buttonImage;
    [SerializeField] TMP_Text contentText;
    [SerializeField] Color enableColor;
    [SerializeField] Color disableColor;
    
    public void Switch(bool isOn, string content = null)
    {
        button.enabled = isOn;
        buttonImage.color = isOn ? enableColor : disableColor;

        if(!string.IsNullOrEmpty(content))
            contentText.text = content;
    }

}
