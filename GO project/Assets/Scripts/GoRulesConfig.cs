// GoRulesConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "GoRulesConfig",menuName = "Go/Rules Config")]
public class GoRulesConfig : ScriptableObject
{
    [Header("Keep defaults = current behavior")]
    public bool captureBeforeSuicide = false;      // off = your current order
    public bool simpleKo = false;                  // off = your current ko behavior
    public bool allowScriptedAIThrowIn = false;    // off = AI suicide still rejected
    public bool validatePresetsOnLoad = false;     // off = no editor/runtime validation
    public bool verboseLogs = false;
}
