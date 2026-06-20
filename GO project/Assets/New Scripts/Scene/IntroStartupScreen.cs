using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// First-launch intro splash (Bootstrap). Replaces the loading animator for the very first
/// app load with custom art: the screen sits opaque, the logo fades in, holds, then the lobby
/// loads underneath and the whole screen fades away to reveal it.
///
/// SETUP:
///  - Put this component ON the "First Startup Screen" root (make it a top-level Canvas object
///    so it can persist across the load and draw on top).
///  - Drag the logo child into <see cref="logo"/>.
///  - On the Bootstrap SceneLoader, UNCHECK "Load Initial Scene On Start" (this intro loads it).
/// CanvasGroups are added automatically for fading.
///
/// Flow: root alpha 1 / logo alpha 0  ->  logo 0->1  ->  wait  ->  load lobby  ->  root 1->0  ->  off.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class IntroStartupScreen : MonoBehaviour
{
    [Header("Logo (faded in over the screen)")]
    [SerializeField] private GameObject logo;

    [Header("Scene")]
    [Tooltip("Build index of the lobby to load (New Lobby is 1).")]
    [SerializeField] private int lobbySceneIndex = 1;
    [Tooltip("Optional. The normal loading-screen object to force OFF during the intro so it never plays on first startup.")]
    [SerializeField] private GameObject loadingScreenToHide;

    [Header("Timing (seconds)")]
    [Tooltip("Blank screen hold before the logo starts fading in.")]
    [SerializeField] private float delayBeforeLogo = 2f;
    [SerializeField] private float logoFadeInDuration = 0.6f;
    [SerializeField] private float holdSeconds = 1f;
    [SerializeField] private float logoFadeOutDuration = 0.6f;
    [SerializeField] private float screenFadeOutDuration = 0.6f;

    private CanvasGroup screenGroup;
    private CanvasGroup logoGroup;

    private void Awake()
    {
        // Persist across the lobby load so we can fade out on top of it.
        DontDestroyOnLoad(gameObject);

        // Make sure the normal loading-screen never plays during the first startup.
        if (loadingScreenToHide != null)
            loadingScreenToHide.SetActive(false);

        screenGroup = GetComponent<CanvasGroup>();
        logoGroup = EnsureCanvasGroup(logo);

        // Draw above everything, including the lobby that loads underneath.
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 32760;
        }

        screenGroup.alpha = 1f;     // screen fully covers
        if (logoGroup != null)
            logoGroup.alpha = 0f;   // logo hidden

        gameObject.SetActive(true);
    }

    private void Start()
    {
        StartCoroutine(RunIntro());
    }

    private IEnumerator RunIntro()
    {
        // Blank, fully-covering screen for a moment before the logo appears.
        if (delayBeforeLogo > 0f)
            yield return new WaitForSeconds(delayBeforeLogo);

        yield return Fade(logoGroup, 0f, 1f, logoFadeInDuration);

        if (holdSeconds > 0f)
            yield return new WaitForSeconds(holdSeconds);

        // Load the lobby behind the still-opaque screen.
        AsyncOperation load = SceneManager.LoadSceneAsync(lobbySceneIndex);
        if (load != null)
        {
            while (!load.isDone)
                yield return null;
        }

        // One frame for the lobby to render. Fade the logo out fully first, then the screen.
        yield return null;
        yield return Fade(logoGroup, 1f, 0f, logoFadeOutDuration);
        yield return Fade(screenGroup, 1f, 0f, screenFadeOutDuration);

        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
            yield break;

        group.alpha = from;
        if (duration <= 0f)
        {
            group.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = to;
    }

    private static CanvasGroup EnsureCanvasGroup(GameObject target)
    {
        if (target == null)
            return null;

        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null)
            group = target.AddComponent<CanvasGroup>();
        return group;
    }
}
