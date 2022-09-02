using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface INameField
{
    string Name { get; }
    string Job { get; }
    bool IsVisible { get; }                 // 화면에 출력할 것인가?
    Vector3 UiPosition { get; }

    void SwitchVisible(bool isVisible);
}

public class NameField : MonoBehaviour
{
    [SerializeField] Text jobText;
    [SerializeField] Text nameText;

    INameField target;
    new Transform transform;
    Camera cam;
    bool isVisible;

    public void Setup(INameField target)
    {
        this.target = target;
        this.transform = base.transform;
        cam = Camera.main;
        
        nameText.text = target.Name;
        jobText.text = target.Job;
        isVisible = target.IsVisible;

        // 크기 맞추기.
        nameText.Fit();
        jobText.Fit();
    }


    private void Update()
    {
        if(target == null)
        {
            // 필드 소멸...
            Destroy(gameObject);
            return;
        }

        // 만약 변수의 값이 달라지면..
        if(isVisible != target.IsVisible)
        {
            Debug.Log("네임 필드 변경 : " + target.IsVisible);
            isVisible = target.IsVisible;
            jobText.gameObject.SetActive(isVisible);
            nameText.gameObject.SetActive(isVisible);
        }

        transform.position = cam.WorldToScreenPoint(target.UiPosition);
    }
}
