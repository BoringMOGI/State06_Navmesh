using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface INameField
{
    string Name { get; }
    string Job { get; }
    bool IsVisible { get; }                 // ȭ�鿡 ����� ���ΰ�?
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

        // ũ�� ���߱�.
        nameText.Fit();
        jobText.Fit();
    }


    private void Update()
    {
        if(target == null)
        {
            // �ʵ� �Ҹ�...
            Destroy(gameObject);
            return;
        }

        // ���� ������ ���� �޶�����..
        if(isVisible != target.IsVisible)
        {
            Debug.Log("���� �ʵ� ���� : " + target.IsVisible);
            isVisible = target.IsVisible;
            jobText.gameObject.SetActive(isVisible);
            nameText.gameObject.SetActive(isVisible);
        }

        transform.position = cam.WorldToScreenPoint(target.UiPosition);
    }
}
