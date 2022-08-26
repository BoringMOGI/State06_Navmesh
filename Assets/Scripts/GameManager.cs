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
        // 씬 매니저에게 이벤트 등록.
        SceneManager.sceneUnloaded += OnUnloaded;
    }
    private void Update()
    {
        OnCheckPause();
    }

    private void OnDestroy()
    {
        // 씬 매니저에게 등록한 이벤트 해제.
        SceneManager.sceneUnloaded -= OnUnloaded;
    }

    private void OnPause()
    {
        // 만약 옵션 씬이 로드되어있지 않다면 옵션씬을 (더하기 모드로) 로드한다.
        if (!SceneManager.GetSceneByName(SCENE_OPTION).isLoaded)
        {
            Debug.Log("게임 일시 정지");
            SceneManager.LoadSceneAsync(SCENE_OPTION, LoadSceneMode.Additive);
        }
    }
    private void OnUnloaded(Scene scene)
    {
        if (scene.name == SCENE_OPTION)
        {
            Debug.Log("일시정지 해제");
            isPause = false;
        }
    }
    private bool OnCheckPause()
    {
        // 게임이 멈춰있지 않고 ESC를 누르면 게임을 멈춘다.
        if (!isPause && Input.GetKeyDown(KeyCode.Escape))
        {
            isPause = true;     // isPuase의 값을 반대로 바꾼다.
            OnPause();          // Pause창(옵션창)을 켠다.
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
