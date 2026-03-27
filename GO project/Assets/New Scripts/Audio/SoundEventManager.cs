using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Moddwyn.Audio
{
    [System.Serializable]
    public class AudioMixerVolume
    {
        public string exposedParameter;
        public Slider slider;
        public TMP_Text volumeText;
        [Range(0, 100)] public int startingVolumePercent = 100;
    }

    public class SoundEventManager : MonoBehaviour
    {
        public AudioMixer audioMixer;
        public List<AudioMixerVolume> volumes = new List<AudioMixerVolume>();


        void Start()
        {
            foreach (var volume in volumes)
            {
                float linear = Mathf.Clamp01(volume.startingVolumePercent / 100f);
                linear = Mathf.Clamp(linear, 0.0001f, 1f);
                float dB = Mathf.Log10(linear) * 20f;
                audioMixer.SetFloat(volume.exposedParameter, dB);

                volume.slider?.SetValueWithoutNotify(linear);
                UpdateVolumeText(volume, linear);

                volume.slider?.onValueChanged.AddListener(val => SetVolume(volume, val));
            }
        }

        public void SetVolume(AudioMixerVolume volume, float sliderValue)
        {
            float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
            audioMixer.SetFloat(volume.exposedParameter, dB);
            UpdateVolumeText(volume, sliderValue);
        }

        void UpdateVolumeText(AudioMixerVolume volume, float sliderValue)
        {
            int percentage = Mathf.RoundToInt(sliderValue * 100f);

            if (volume.volumeText == null)
                return;

            volume.volumeText.text = percentage + "%";
        }
    }
}