using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class EditorControl : MonoBehaviour
{
    [MenuItem("Custom/Load Login &1")]
    private static void OnLoadLoginScene()
    {
        if (Application.isPlaying)
            Application.Quit();

        EditorSceneManager.OpenScene("Assets/Scenes/Login.unity"); 
    }

    [MenuItem("Custom/Load Room &2")]
    private static void OnLoadRoomScene()
    {
        if (Application.isPlaying)
            Application.Quit();

        EditorSceneManager.OpenScene("Assets/Scenes/Room.unity");
    }

    [MenuItem("Custom/Play Game &q")]
    private static void OnPlayButton()
    {
        if (Application.isPlaying)
            EditorApplication.ExitPlaymode();
        else
            EditorApplication.EnterPlaymode();
    }
}
