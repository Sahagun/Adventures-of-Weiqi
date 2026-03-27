using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moddwyn
{
    public class LoadingScreen : MonoBehaviour
    {
        public GameObject loadingScreen;
        public Animator loadingAnimator;
        public string triggerStart = "Start";
        public string triggerEnd = "Finish";

        public void StartLoading()
        {
            loadingAnimator?.SetTrigger(triggerStart);
        }

        public void EndLoading()
        {
            loadingAnimator?.SetTrigger(triggerEnd);
        }

        public void SetState(bool isActive)
        {
            loadingScreen?.SetActive(isActive);
        }
    }
}