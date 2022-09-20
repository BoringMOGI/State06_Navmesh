using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoomUserUI : MonoBehaviour
{
    [SerializeField] TMP_Text indexText;
    [SerializeField] TMP_Text nameText;

    public void Setup(int index, string name, bool isMaster)
    {
        indexText.text = index.ToString("00");

        if (isMaster)
            nameText.text = $"{name}(πÊ¿Â)";
        else
            nameText.text = name;
    }
}
