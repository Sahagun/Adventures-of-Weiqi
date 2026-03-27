using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoLesson",menuName = "Go Lessons/Lesson Data")]
public class GoLessonData : ScriptableObject
{
    public string lessonTitle = "New Lesson";
    public string lessonId = "new_lesson";

    public List<GoLessonSlideData> slides = new();

    public int SlideCount => slides != null ? slides.Count : 0;

    public GoLessonSlideData GetSlide(int slideIndex)
    {
        if (slides == null || slides.Count == 0)
            return null;

        int clampedIndex = Mathf.Clamp(slideIndex,0,slides.Count - 1);
        return slides[clampedIndex];
    }

    public int ClampSlideIndex(int slideIndex)
    {
        if (slides == null || slides.Count == 0)
            return 0;

        return Mathf.Clamp(slideIndex,0,slides.Count - 1);
    }

    public string GetDisplayTitle()
    {
        if (!string.IsNullOrWhiteSpace(lessonTitle))
            return lessonTitle;

        return string.IsNullOrWhiteSpace(name) ? "Lesson" : name;
    }
}

[Serializable]
public class GoLessonSlideData
{
    public string slideName = "New Slide";
    public GoLessonSlideType slideType = GoLessonSlideType.Content;

    [TextArea(3,10)]
    public string bodyText = string.Empty;
    public TextAsset boardJsonFile;

    public bool correctYesAnswer = true;
    public int correctNumberAnswer = 0;

    public bool RequiresPuzzleCompletion => slideType == GoLessonSlideType.Puzzle;
    public bool UsesYesNoAnswer => slideType == GoLessonSlideType.YesNo;
    public bool UsesNumberAnswer => slideType == GoLessonSlideType.Number;
    public bool HasBoardReference => boardJsonFile != null;

    public string GetDisplayName(int slideIndex)
    {
        if (!string.IsNullOrWhiteSpace(slideName))
            return slideName;

        return $"Slide {slideIndex + 1}";
    }
}

public enum GoLessonSlideType
{
    Content = 0,
    Puzzle = 1,
    YesNo = 2,
    Number = 3
}
