using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Moddwyn
{
    public class SceneLoader : MonoBehaviour
    {
        public LoadingScreen loadingScreen;
        public int initialSceneIndex = 1;
        public float delayBeforeLoading = 1f;
        public float delayBeforeActivation = 0.25f;
        public float delayBeforeDeactivation = 1.5f;

        private bool isLoading;

        public static SceneLoader Instance { get; private set; }

        public event Action OnSceneLoaded;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            LoadScene(initialSceneIndex);
        }

        public void LoadScene(string sceneName)
        {
            if (isLoading)
            {
                return;
            }

            LoadScene(SceneManager.GetSceneByName(sceneName).buildIndex);
        }

        public void LoadScene(int index)
        {
            if (isLoading)
            {
                return;
            }

            StartCoroutine(LoadSceneByIndexRoutine(index));
        }

        public void RestartCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private IEnumerator LoadSceneByIndexRoutine(int index)
        {
            isLoading = true;

            if (loadingScreen != null)
            {
                loadingScreen.SetState(true);
                loadingScreen.StartLoading();
            }

            if (delayBeforeLoading > 0f)
            {
                yield return new WaitForSeconds(delayBeforeLoading);
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(index);
            yield return HandleLoadOperation(loadOperation);
        }

        private IEnumerator HandleLoadOperation(AsyncOperation loadOperation)
        {
            if (loadOperation == null)
            {
                isLoading = false;
                yield break;
            }

            loadOperation.allowSceneActivation = false;

            while (loadOperation.progress < 0.9f)
            {
                yield return null;
            }

            if (loadingScreen != null)
            {
                loadingScreen.EndLoading();
            }

            if (delayBeforeActivation > 0f)
            {
                yield return new WaitForSeconds(delayBeforeActivation);
            }

            loadOperation.allowSceneActivation = true;

            while (!loadOperation.isDone)
            {
                yield return null;
            }

            if (loadingScreen != null && delayBeforeDeactivation > 0f)
            {
                yield return new WaitForSeconds(delayBeforeDeactivation);
                loadingScreen.SetState(false);
            }

            isLoading = false;

            OnSceneLoaded?.Invoke();
        }
    }
}
