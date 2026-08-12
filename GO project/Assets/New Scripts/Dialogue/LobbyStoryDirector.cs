using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Decides which story conversation should play in the New Lobby and plays it once.
/// Call <see cref="BeginStory"/> when the player dismisses the title screen
/// (wire it onto the same "Start" button that hides the title).
///
/// Beats (each plays only once, tracked by <see cref="StoryFlags"/>):
///  1. Intro            - first time the player starts the game.
///  2. After 3 lessons  - once Basic Rules, Liberties 1 & Liberties 2 are all complete.
///  3. After Training Hub - once the player has entered the Training Hub at least once.
/// </summary>
public class LobbyStoryDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StoryDialogueRunner runner;

    [Header("Guiding arrows (optional scene objects)")]
    [SerializeField] private GameObject arrowToAcademy;
    [SerializeField] private GameObject arrowToTrainingHub;

    [Header("First three lessons (drag the lesson assets)")]
    [Tooltip("Basic Rules, Liberties 1, Liberties 2 - all must be completed to trigger beat 2.")]
    [SerializeField] private List<GoLessonData> requiredFirstLessons = new();

    [Header("Timing")]
    [Tooltip("Delay after BeginStory() before the conversation starts (lets the scene settle).")]
    [SerializeField] private float startDelay = 0.5f;

    private bool hasBegun;

    private void Awake()
    {
        if (runner == null)
            runner = FindObjectOfType<StoryDialogueRunner>(true);

        HideArrows();
    }

    /// <summary>Hook this to the New Lobby "Start" button (the one that hides the title screen).</summary>
    public void BeginStory()
    {
        if (hasBegun)
            return;

        hasBegun = true;
        StartCoroutine(BeginStoryRoutine());
    }

    private IEnumerator BeginStoryRoutine()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        HideArrows();

        if (!StoryFlags.IntroPlayed)
        {
            PlayBeat(StoryScript.Intro(), () =>
            {
                StoryFlags.IntroPlayed = true;
                ShowArrow(arrowToAcademy);
            });
            yield break;
        }

        // The beats are strictly ordered. Visiting the Training Hub before the first
        // three lessons must not skip past the after-lessons conversation, so nothing
        // below this block can run until that beat has played.
        if (!StoryFlags.AfterLessonsPlayed)
        {
            if (AreFirstLessonsComplete())
            {
                PlayBeat(StoryScript.AfterFirstLessons(), () =>
                {
                    StoryFlags.AfterLessonsPlayed = true;
                    ShowArrow(arrowToTrainingHub);
                });
            }
            else
            {
                // Lessons still owed - keep pointing back at the Academy.
                ShowArrow(arrowToAcademy);
            }

            yield break;
        }

        if (!StoryFlags.AfterTrainingHubPlayed && StoryFlags.TrainingHubVisited)
        {
            PlayBeat(StoryScript.AfterTrainingHub(), () =>
            {
                StoryFlags.AfterTrainingHubPlayed = true;
            });
            yield break;
        }

        if (!StoryFlags.TrainingHubVisited)
            ShowArrow(arrowToTrainingHub);
    }

    private void PlayBeat(StoryConversation conversation, System.Action onFinished)
    {
        if (runner == null)
        {
            Debug.LogWarning("LobbyStoryDirector has no StoryDialogueRunner assigned; skipping conversation.");
            onFinished?.Invoke();
            return;
        }

        runner.Play(conversation, onFinished);
    }

    private bool AreFirstLessonsComplete()
    {
        if (requiredFirstLessons == null || requiredFirstLessons.Count == 0)
            return false;

        foreach (GoLessonData lesson in requiredFirstLessons)
        {
            if (lesson == null)
                continue;

            if (!LessonProgressionStore.IsLessonCompleted(lesson.lessonId))
                return false;
        }

        return true;
    }

    // Right-click the component header in the Inspector to replay the story from scratch while testing.
    [ContextMenu("Reset Story Flags (debug)")]
    private void ResetStoryFlagsDebug()
    {
        StoryFlags.ResetAll();
        Debug.Log("Story flags reset - all lobby conversations (intro, after-lessons, after-hub) will play again.");
    }

    private void ShowArrow(GameObject arrow)
    {
        if (arrow != null)
            arrow.SetActive(true);
    }

    private void HideArrows()
    {
        if (arrowToAcademy != null)
            arrowToAcademy.SetActive(false);
        if (arrowToTrainingHub != null)
            arrowToTrainingHub.SetActive(false);
    }
}
