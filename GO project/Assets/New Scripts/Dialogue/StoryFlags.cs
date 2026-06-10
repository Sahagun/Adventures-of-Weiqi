using UnityEngine;

/// <summary>
/// Persistent one-shot flags for the story beats, stored in PlayerPrefs so each
/// conversation only plays once and the Training-Hub visit survives scene loads.
/// </summary>
public static class StoryFlags
{
    private const string IntroPlayedKey = "Story.IntroPlayed";
    private const string AfterLessonsPlayedKey = "Story.AfterLessonsPlayed";
    private const string AfterTrainingHubPlayedKey = "Story.AfterTrainingHubPlayed";
    private const string TrainingHubVisitedKey = "Story.TrainingHubVisited";

    public static bool IntroPlayed
    {
        get => GetBool(IntroPlayedKey);
        set => SetBool(IntroPlayedKey, value);
    }

    public static bool AfterLessonsPlayed
    {
        get => GetBool(AfterLessonsPlayedKey);
        set => SetBool(AfterLessonsPlayedKey, value);
    }

    public static bool AfterTrainingHubPlayed
    {
        get => GetBool(AfterTrainingHubPlayedKey);
        set => SetBool(AfterTrainingHubPlayedKey, value);
    }

    public static bool TrainingHubVisited
    {
        get => GetBool(TrainingHubVisitedKey);
        set => SetBool(TrainingHubVisitedKey, value);
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(IntroPlayedKey);
        PlayerPrefs.DeleteKey(AfterLessonsPlayedKey);
        PlayerPrefs.DeleteKey(AfterTrainingHubPlayedKey);
        PlayerPrefs.DeleteKey(TrainingHubVisitedKey);
        PlayerPrefs.Save();
    }

    private static bool GetBool(string key) => PlayerPrefs.GetInt(key, 0) == 1;

    private static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
