using ChatNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatUserUI : MonoBehaviour
{
    [SerializeField] Image userImage;
    [SerializeField] Text nameText;
    [SerializeField] Text statusText;
    
    public void Setup(ChatUser user)
    {
        nameText.text = user.name;
        statusText.text = user.status.ToString();
    }
}
