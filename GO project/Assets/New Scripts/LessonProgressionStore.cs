using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Persists which lessons the player has completed (by lessonId) to PlayerPrefs.
/// Mirrors the style of <see cref="PuzzleProgressionStore"/>.
/// </summary>
public static class LessonProgressionStore
{
    private const string KeyPrefix = "LessonProgression";
    private const string CompletedSuffix = "Completed";

    public static bool IsLessonCompleted(string lessonId)
    {
        string normalizedId = NormalizeId(lessonId);
        if (string.IsNullOrEmpty(normalizedId))
            return false;

        return PlayerPrefs.GetInt(BuildKey(normalizedId, CompletedSuffix), 0) == 1;
    }

    public static void SetLessonCompleted(string lessonId, bool isCompleted = true)
    {
        string normalizedId = NormalizeId(lessonId);
        if (string.IsNullOrEmpty(normalizedId))
            return;

        PlayerPrefs.SetInt(BuildKey(normalizedId, CompletedSuffix), isCompleted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool AreAllCompleted(IEnumerable<string> lessonIds)
    {
        if (lessonIds == null)
            return false;

        bool any = false;
        foreach (string lessonId in lessonIds)
        {
            any = true;
            if (!IsLessonCompleted(lessonId))
                return false;
        }

        return any;
    }

    public static void ClearLesson(string lessonId)
    {
        string normalizedId = NormalizeId(lessonId);
        if (string.IsNullOrEmpty(normalizedId))
            return;

        PlayerPrefs.DeleteKey(BuildKey(normalizedId, CompletedSuffix));
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    private static string BuildKey(string normalizedId, string suffix)
    {
        return $"{KeyPrefix}.{normalizedId}.{suffix}";
    }

    private static string NormalizeId(string lessonId)
    {
        if (string.IsNullOrWhiteSpace(lessonId))
            return string.Empty;

        StringBuilder builder = new StringBuilder(lessonId.Length);
        string trimmed = lessonId.Trim().ToLowerInvariant();

        foreach (char character in trimmed)
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');

        return builder.ToString().Trim('_');
    }
}
