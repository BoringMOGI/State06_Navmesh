using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool isPause
    {
        get;
        private set;
    }

    private const string SCENE_OPTION = "Option";

    private void Start()
    {
        // �� �Ŵ������� �̺�Ʈ ���.
        SceneManager.sceneUnloaded += OnUnloaded;
    }
    private void Update()
    {
        OnCheckPause();
    }

    private void OnDestroy()
    {
        // �� �Ŵ������� ����� �̺�Ʈ ����.
        SceneManager.sceneUnloaded -= OnUnloaded;
    }

    private void OnPause()
    {
        // ���� �ɼ� ���� �ε�Ǿ����� �ʴٸ� �ɼǾ��� (���ϱ� ����) �ε��Ѵ�.
        if (!SceneManager.GetSceneByName(SCENE_OPTION).isLoaded)
        {
            Debug.Log("���� �Ͻ� ����");
            SceneManager.LoadSceneAsync(SCENE_OPTION, LoadSceneMode.Additive);
        }
    }
    private void OnUnloaded(Scene scene)
    {
        if (scene.name == SCENE_OPTION)
        {
            Debug.Log("�Ͻ����� ����");
            isPause = false;
        }
    }
    private bool OnCheckPause()
    {
        // ������ �������� �ʰ� ESC�� ������ ������ �����.
        if (!isPause && Input.GetKeyDown(KeyCode.Escape))
        {
            isPause = true;     // isPuase�� ���� �ݴ�� �ٲ۴�.
            OnPause();          // Pauseâ(�ɼ�â)�� �Ҵ�.
        }

        return isPause;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            isPause = true;
            OnPause();
        }
    }

}
