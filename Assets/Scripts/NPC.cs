using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour, IInteraction, INameField
{
    [System.Serializable]
    struct Talk
    {
        public string[] talks;
        public float talkRate;      // �� �� �������� ���� �ϴ°�?
        public float showTime;      // �󸶳� ���� �����ִ°�?

        public string GetTalk()
        {
            if (talks.Length <= 0)
                return "��簡 �ԷµǾ� ���� �ʽ��ϴ�.";

            return talks[Random.Range(0, talks.Length)];
        }
    }

    [SerializeField] string npcName;
    [SerializeField] string npcJob;
    [SerializeField] string actionName;
    [SerializeField] KeyCode key;
    [SerializeField] Transform nameFieldPivot;      // �̸� ǥ�� ��ġ.
    [SerializeField] Transform actionPivot;         // ��ȣ�ۿ� ǥ�� ��ġ.

    [Header("Talk")]
    [SerializeField] Talk talk;

    public string Name => npcName;
    public string ActionName => actionName;
    public string Job => npcJob;
    public KeyCode Key => key;
    public Vector3 Position => transform.position;
    public bool IsVisible => isVisible;

    Vector3 INameField.UiPosition => nameFieldPivot.position;
    Vector3 IInteraction.UiPosition => actionPivot.position;

    private bool isVisible = true;

    private void Start()
    {
        StringFieldManager.Instance.RegestedName(this);
        StartCoroutine(UpdateTalk());
    }

    // ��ȣ�ۿ�!
    void IInteraction.OnInteract()
    {
        Debug.Log("NPC����");
    }

    // �̸��� ����ϰų� ���� �Ѵ�.
    void INameField.SwitchVisible(bool isVisible)
    {
        this.isVisible = isVisible;
    }

    IEnumerator UpdateTalk()
    {
        // ���� �����ϰ�, ���� ���� ���� ��ٸ��� �ð��� ���� �����ؾ��ϴϱ�...
        WaitForSeconds wait = new WaitForSeconds(talk.talkRate + talk.showTime);

        while (true)
        {
            yield return wait;
            StringFieldManager.Instance.ShowSpeechBubble(this, nameFieldPivot, talk.GetTalk(), talk.showTime);
        }
    }
  
}
