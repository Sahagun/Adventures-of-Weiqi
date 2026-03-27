using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moddwyn.Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        public void PlaySound(string name)
        {
            AudioManager.Instance.PlaySound(name);
        }
    }
}
