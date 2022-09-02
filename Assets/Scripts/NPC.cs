using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour, IInteraction, INameField
{
    [System.Serializable]
    struct Talk
    {
        public string[] talks;
        public float talkRate;      // 몇 초 간격으로 말을 하는가?
        public float showTime;      // 얼마나 오래 보여주는가?

        public string GetTalk()
        {
            if (talks.Length <= 0)
                return "대사가 입력되어 있지 않습니다.";

            return talks[Random.Range(0, talks.Length)];
        }
    }

    [SerializeField] string npcName;
    [SerializeField] string npcJob;
    [SerializeField] string actionName;
    [SerializeField] KeyCode key;
    [SerializeField] Transform nameFieldPivot;      // 이름 표시 위치.
    [SerializeField] Transform actionPivot;         // 상호작용 표시 위치.

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

    // 상호작용!
    void IInteraction.OnInteract()
    {
        Debug.Log("NPC반응");
    }

    // 이름을 출력하거나 끄게 한다.
    void INameField.SwitchVisible(bool isVisible)
    {
        this.isVisible = isVisible;
    }

    IEnumerator UpdateTalk()
    {
        // 말을 시작하고, 말을 끝낸 이후 기다리는 시간을 전부 포함해야하니까...
        WaitForSeconds wait = new WaitForSeconds(talk.talkRate + talk.showTime);

        while (true)
        {
            yield return wait;
            StringFieldManager.Instance.ShowSpeechBubble(this, nameFieldPivot, talk.GetTalk(), talk.showTime);
        }
    }
  
}
