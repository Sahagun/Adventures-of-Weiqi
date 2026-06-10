using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the Philosophy Academy lesson unlock graph and answers "is this lesson unlocked?".
/// Place one instance in the Philosophy Academy scene and fill in the rules in the Inspector.
///
/// PDF rules:
///  - Basic Rules (Capture) is unlocked by default.
///  - Every lesson unlocks the next (model this with a single prerequisite = the previous lesson).
///  - Eyes (Life &amp; Death) unlocks after completing Liberties 2 (Capture).
///  - Shapes 2 (Life &amp; Death) unlocks after completing BOTH Shapes 1 and Ko Rule.
///  - "Territorial Skills" lessons are left locked (no rule / unlockedByDefault = false, no prereqs).
/// </summary>
public class LessonUnlockManager : MonoBehaviour
{
    [System.Serializable]
    public class LessonUnlockRule
    {
        public GoLessonData lesson;
        [Tooltip("If true, this lesson is always available (e.g. Basic Rules).")]
        public bool unlockedByDefault = false;
        [Tooltip("ALL of these lessons must be completed before this one unlocks.")]
        public List<GoLessonData> prerequisites = new();
    }

    public static LessonUnlockManager Instance { get; private set; }

    [SerializeField] private List<LessonUnlockRule> rules = new();
    [Tooltip("Lessons with no matching rule are treated as locked when this is on (recommended for the Academy).")]
    [SerializeField] private bool lockLessonsWithoutRule = false;

    private void Awake()
    {
        Instance = this;
    }

    public bool IsLessonUnlocked(GoLessonData lesson)
    {
        if (lesson == null)
            return false;

        LessonUnlockRule rule = rules.Find(r => r != null && r.lesson == lesson);
        if (rule == null)
            return !lockLessonsWithoutRule;

        if (rule.unlockedByDefault)
            return true;

        if (rule.prerequisites == null || rule.prerequisites.Count == 0)
            return false; // Gated but with no prerequisites set -> stays locked.

        foreach (GoLessonData prerequisite in rule.prerequisites)
        {
            if (prerequisite == null)
                continue;

            if (!LessonProgressionStore.IsLessonCompleted(prerequisite.lessonId))
                return false;
        }

        return true;
    }

    public bool IsLessonUnlockedById(string lessonId)
    {
        if (string.IsNullOrWhiteSpace(lessonId))
            return false;

        LessonUnlockRule rule = rules.Find(r => r != null && r.lesson != null && r.lesson.lessonId == lessonId);
        return rule != null && IsLessonUnlocked(rule.lesson);
    }
}
