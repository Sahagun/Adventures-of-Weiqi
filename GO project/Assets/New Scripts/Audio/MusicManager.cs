using UnityEngine;
using UnityEngine.SceneManagement;

namespace Moddwyn.Audio
{
    /// <summary>
    /// Persistent background-music controller.
    /// - Plays the lobby track (Dawn of Heiqi) only in the New Lobby scene.
    /// - Plays the main track (Swords Down, Minds Up) everywhere else, and does NOT
    ///   restart it when moving between non-lobby scenes (only swaps when the target
    ///   track actually changes).
    /// Place one instance in the scene (New Lobby or Bootstrap); it survives scene loads.
    /// </summary>
    public class MusicManager : Singleton<MusicManager>
    {
        [Header("Tracks")]
        [Tooltip("Dawn of Heiqi - plays in the lobby scene only.")]
        [SerializeField] private AudioClip lobbyMusic;
        [Tooltip("Swords Down, Minds Up - plays in every non-lobby, non-ignored scene.")]
        [SerializeField] private AudioClip mainMusic;

        [Header("Scenes")]
        [Tooltip("Scene name that should play the lobby track.")]
        [SerializeField] private string lobbySceneName = "New Lobby";
        [Tooltip("Scenes where the music should be left unchanged (e.g. the Bootstrap loader).")]
        [SerializeField] private string[] ignoredSceneNames = { "Bootstrap" };

        [Header("Playback")]
        [SerializeField, Range(0f, 1f)] private float volume = 0.6f;
        [SerializeField] private AudioSource musicSource;

        private AudioClip currentClip;

        protected override void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            base.Awake();
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
                musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = volume;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            // Apply the correct track for the scene that was already active when we spawned.
            ApplyMusicForScene(SceneManager.GetActiveScene());
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyMusicForScene(scene);
        }

        private void ApplyMusicForScene(Scene scene)
        {
            if (IsIgnoredScene(scene))
                return; // Leave whatever is currently playing (or silence at startup).

            AudioClip target = IsLobbyScene(scene) ? lobbyMusic : mainMusic;
            PlayIfDifferent(target);
        }

        private bool IsLobbyScene(Scene scene)
        {
            return string.Equals(scene.name, lobbySceneName, System.StringComparison.OrdinalIgnoreCase);
        }

        private bool IsIgnoredScene(Scene scene)
        {
            if (ignoredSceneNames == null)
                return false;

            foreach (string ignored in ignoredSceneNames)
            {
                if (string.Equals(scene.name, ignored, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private void PlayIfDifferent(AudioClip clip)
        {
            if (clip == null)
                return;

            // Same track already playing -> keep it going so it doesn't restart on scene change.
            if (currentClip == clip && musicSource.isPlaying)
                return;

            currentClip = clip;
            musicSource.clip = clip;
            musicSource.volume = volume;
            musicSource.Play();
        }

        /// <summary>Optional runtime volume control (e.g. from a settings slider).</summary>
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (musicSource != null)
                musicSource.volume = volume;
        }
    }
}
