using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Moddwyn.Audio
{
    public class ButtonSound : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip buttonSound;

        public List<Button> buttons = new();

        void Start()
        {
            SceneLoader.Instance.OnSceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded()
        {
            buttons.Clear();
            Button[] foundButtons = FindObjectsOfType<Button>(true);
            buttons.AddRange(foundButtons);
            
            foreach (Button button in buttons)
            {
                button?.onClick?.AddListener(() => PlayButtonSound());
            }
        }

        public void PlayButtonSound()
        {
            audioSource.PlayOneShot(buttonSound);
        }
    }
}