using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the audio
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region SINGLETON

    public static AudioManager Instance;
    private void Awake() => Instance = this;

    #endregion

    private AudioSource audioSource;
    [SerializeField] private AudioClip clipMove;
    [SerializeField] private AudioClip clipTake;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();    
    }

    /// <summary>
    /// Plays move sound
    /// </summary>
    public void PlayMove()
    {
        audioSource.PlayOneShot(clipMove);
    }

    /// <summary>
    /// Plays take sound
    /// </summary>
    public void PlayTake()
    {
        audioSource.PlayOneShot(clipTake);
    }
}
