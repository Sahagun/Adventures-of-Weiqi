using System.Collections.Generic;
using UnityEngine;

namespace Moddwyn.Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        public enum AudioGroup { Gameplay, UI, Music }
        public List<AudioData> audioDatas = new();
        public List<GroupMixer> groupMixers = new();


        public void PlaySound(AudioClip clip, AudioGroup group)
        {
            AudioSource source = GetAudioSource(group);
            source?.PlayOneShot(clip);
        }

        public void PlaySound(string name)
        {
            PlaySound(GetAudioClip(name), GetGroupByName(name));
        }

        public AudioClip GetAudioClip(string name)
        {
            return audioDatas.Find(x => x.audioName == name).audioClip;
        }

        public AudioSource GetAudioSource(AudioGroup group)
        {
            return groupMixers.Find(x => x.group == group).source;
        }

        public AudioGroup GetGroupByName(string name)
        {
            return audioDatas.Find(x => x.audioName == name).group;
        }

        [System.Serializable]
        public class AudioData
        {
            public string audioName;
            public AudioGroup group;
            public AudioClip audioClip;
        }

        [System.Serializable]
        public class GroupMixer
        {
            public AudioGroup group;
            public AudioSource source;
        }
    }
}