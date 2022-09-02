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
        // �Ű� ���� ����.
        this.nameField = nameField;
        this.pivot = pivot;
        this.showTime = showTime;

        nameText.text = nameField.Name;
        speechText.text = talk;        

        if(nameField != null)
        {
            // �̸� �ʵ带 ���ش�.
            nameField.SwitchVisible(false);
        }

        // ũ�� ����.
        nameText.Fit();
        speechText.Fit();

        StartCoroutine(Updates());
    }

    IEnumerator Updates()
    {
        float timer = showTime;
        while((timer -= Time.deltaTime) > 0f)
        {
            // �������� pivot�� null�̶�� �� ���� �� ����� ������ٴ� ���̴�.
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


        // �ð��� �� �Ǿ����� ��ǳ�� ���� + �̸� �ʵ� ����
        if(nameField != null)
            nameField.SwitchVisible(true);

        Destroy(gameObject);
    }
}
