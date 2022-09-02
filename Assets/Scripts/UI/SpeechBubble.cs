using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SpeechBubble : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text speechText;

    INameField nameField;
    Transform pivot;
    float showTime;

    public void Speech(INameField nameField, Transform pivot, string talk, float showTime)
    {
        // 매개 변수 저장.
        this.nameField = nameField;
        this.pivot = pivot;
        this.showTime = showTime;

        nameText.text = nameField.Name;
        speechText.text = talk;        

        if(nameField != null)
        {
            // 이름 필드를 꺼준다.
            nameField.SwitchVisible(false);
        }

        // 크기 지정.
        nameText.Fit();
        speechText.Fit();

        StartCoroutine(Updates());
    }

    IEnumerator Updates()
    {
        float timer = showTime;
        while((timer -= Time.deltaTime) > 0f)
        {
            // 기준점인 pivot이 null이라는 건 말을 한 대상이 사라졌다는 것이다.
            if (pivot == null)
            {
                break;
            }
            else
            {
                transform.position = Camera.main.WorldToScreenPoint(pivot.position);
            }
            yield return null;
        }


        // 시간이 다 되었으니 말풍선 삭제 + 이름 필드 띄우기
        if(nameField != null)
            nameField.SwitchVisible(true);

        Destroy(gameObject);
    }
}
