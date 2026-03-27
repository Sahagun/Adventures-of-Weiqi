using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public void ReturnToMainMenu()
    {
        Moddwyn.SceneLoader.Instance.LoadScene(0);
    }

    public void LoadThisSceneName(string sceneName)
    {
        Moddwyn.SceneLoader.Instance.LoadScene(sceneName);
    }

    public void LoadThisSceneNumber(int sceneNumber)
    {
        Moddwyn.SceneLoader.Instance.LoadScene(sceneNumber);
    }

    public void RestartThisScene()
    {
        Moddwyn.SceneLoader.Instance.LoadScene(SceneManager.GetActiveScene().name);
    }
}
