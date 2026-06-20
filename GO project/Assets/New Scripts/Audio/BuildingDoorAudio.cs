using UnityEngine;

/// <summary>
/// Plays building enter/leave sound effects.
///  - The ENTER sound fires immediately when a <see cref="UITrigger"/> sends the player into a building.
///  - The matching LEAVE sound fires when the player returns to the lobby (this component's Start),
///    because the player leaves a building from a different scene.
///
/// Put one of these in the New Lobby (e.g. on the SoundManager object) and assign the four clips
/// from Assets/Imported Assets/Go Kit/Sounds/Sound Effects:
///   Door    buildings (Archive Center): EnterDoor / ExitDoor
///   Doorless buildings (Training Hub, Academy): EnterDoorless / ExitDoorless
/// Then tick "Building Has Door" on each building's UITrigger that has a real door.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BuildingDoorAudio : MonoBehaviour
{
    public static BuildingDoorAudio Instance { get; private set; }

    [Header("Clips (Go Kit/Sounds/Sound Effects)")]
    public AudioClip enterDoor;
    public AudioClip exitDoor;
    public AudioClip enterDoorless;
    public AudioClip exitDoorless;

    [SerializeField] private AudioSource audioSource;

    // Static so it survives the scene load between leaving the lobby and coming back.
    private static bool pendingExit;
    private static bool pendingExitHasDoor;

    private void Awake()
    {
        Instance = this;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // Arriving back in the lobby after visiting a building -> play the leave sound.
        if (pendingExit)
        {
            Play(pendingExitHasDoor ? exitDoor : exitDoorless);
            pendingExit = false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Call right before loading a building scene. Plays the enter SFX and arms the leave SFX.</summary>
    public void PlayEnter(bool hasDoor)
    {
        Play(hasDoor ? enterDoor : enterDoorless);
        pendingExit = true;
        pendingExitHasDoor = hasDoor;
    }

    private void Play(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
