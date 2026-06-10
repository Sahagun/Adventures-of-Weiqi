using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Plays a multi-line <see cref="StoryConversation"/> through a shared dialogue panel.
/// Click (or press the advance button) to finish the current line's typing, then again
/// to move to the next line. When the last line is dismissed the panel floats away and
/// onComplete fires. The "Goggle floats down/up" effect is driven by <see cref="floatTarget"/>.
///
/// Put this on an ALWAYS-ACTIVE object (not the dialogue panel itself, which starts hidden).
/// </summary>
public class StoryDialogueRunner : MonoBehaviour
{
    [Header("Character icons")]
    [Tooltip("Reuse your DialogueConfig: its characters list maps a speaker title -> icon sprite.")]
    public DialogueConfig characterConfig;

    [Header("UI")]
    public GameObject dialogueRoot;
    public Image characterImage;
    public TMP_Text speakerNameText;
    public TMP_Text bodyText;
    [Tooltip("Optional button to advance; clicking anywhere can also call Advance().")]
    public Button advanceButton;

    [Header("Typing")]
    [Min(0f)] public float characterRevealDelay = 0.03f;

    [Header("Goggle float animation")]
    [Tooltip("RectTransform that floats in from above on start and back up on finish (e.g. the panel or Goggle portrait).")]
    public RectTransform floatTarget;
    public float floatOffsetY = 700f;
    [Min(0f)] public float floatDuration = 0.4f;

    public UnityEvent onConversationFinished;

    private List<StoryLine> lines;
    private int lineIndex;
    private bool isActive;
    private bool isTyping;
    private Coroutine typingCoroutine;
    private Coroutine floatCoroutine;
    private Action onComplete;
    private Vector2 floatRestPosition;
    private bool floatRestCaptured;

    public bool IsActive => isActive;

    private void Awake()
    {
        CaptureFloatRest();
        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);
        isActive = false;
    }

    private void OnEnable()
    {
        if (advanceButton != null)
            advanceButton.onClick.AddListener(Advance);
    }

    private void OnDisable()
    {
        if (advanceButton != null)
            advanceButton.onClick.RemoveListener(Advance);
    }

    /// <summary>Starts a conversation. onCompleteCallback fires after the float-out finishes.</summary>
    public void Play(StoryConversation conversation, Action onCompleteCallback = null)
    {
        if (conversation == null || conversation.lines == null || conversation.lines.Count == 0)
        {
            onCompleteCallback?.Invoke();
            return;
        }

        if (isActive)
            return;

        lines = conversation.lines;
        lineIndex = 0;
        onComplete = onCompleteCallback;
        isActive = true;

        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        StartFloatIn();
        ShowLine(0);
    }

    public void Advance()
    {
        if (!isActive)
            return;

        if (isTyping)
        {
            CompleteTypingImmediately();
            return;
        }

        lineIndex++;
        if (lines == null || lineIndex >= lines.Count)
        {
            Finish();
            return;
        }

        ShowLine(lineIndex);
    }

    private void ShowLine(int index)
    {
        StoryLine line = lines[index];

        if (speakerNameText != null)
            speakerNameText.text = line.speaker;

        if (characterImage != null)
        {
            Sprite icon = ResolveIcon(line.speaker);
            characterImage.sprite = icon;
            characterImage.enabled = icon != null;
        }

        if (bodyText != null)
        {
            bodyText.text = line.text;
            bodyText.maxVisibleCharacters = 0;
        }

        StopTyping();
        typingCoroutine = StartCoroutine(TypeRoutine());
    }

    private IEnumerator TypeRoutine()
    {
        isTyping = true;

        if (bodyText == null)
        {
            isTyping = false;
            yield break;
        }

        bodyText.ForceMeshUpdate();
        int count = bodyText.textInfo.characterCount;

        if (count <= 0 || characterRevealDelay <= 0f)
        {
            bodyText.maxVisibleCharacters = count;
            isTyping = false;
            yield break;
        }

        WaitForSeconds delay = new WaitForSeconds(characterRevealDelay);
        for (int i = 1; i <= count; i++)
        {
            bodyText.maxVisibleCharacters = i;
            yield return delay;
        }

        isTyping = false;
    }

    private void CompleteTypingImmediately()
    {
        StopTyping();

        if (bodyText != null)
        {
            bodyText.ForceMeshUpdate();
            bodyText.maxVisibleCharacters = bodyText.textInfo.characterCount;
        }

        isTyping = false;
    }

    private void Finish()
    {
        isActive = false;
        StopTyping();

        Action callback = onComplete;
        onComplete = null;

        StartFloatOut(() =>
        {
            if (dialogueRoot != null)
                dialogueRoot.SetActive(false);

            onConversationFinished?.Invoke();
            callback?.Invoke();
        });
    }

    private Sprite ResolveIcon(string speaker)
    {
        if (characterConfig == null || characterConfig.characters == null)
            return null;

        DialogueCharacter character = characterConfig.characters.Find(
            c => c != null && string.Equals(c.title, speaker, StringComparison.OrdinalIgnoreCase));
        return character != null ? character.icon : null;
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    // ---------- Float animation ----------

    private void CaptureFloatRest()
    {
        if (floatTarget != null && !floatRestCaptured)
        {
            floatRestPosition = floatTarget.anchoredPosition;
            floatRestCaptured = true;
        }
    }

    private void StartFloatIn()
    {
        if (floatTarget == null)
            return;

        CaptureFloatRest();

        if (floatCoroutine != null)
            StopCoroutine(floatCoroutine);

        Vector2 from = floatRestPosition + Vector2.up * floatOffsetY;
        floatCoroutine = StartCoroutine(FloatRoutine(from, floatRestPosition, null));
    }

    private void StartFloatOut(Action done)
    {
        if (floatTarget == null)
        {
            done?.Invoke();
            return;
        }

        if (floatCoroutine != null)
            StopCoroutine(floatCoroutine);

        Vector2 to = floatRestPosition + Vector2.up * floatOffsetY;
        floatCoroutine = StartCoroutine(FloatRoutine(floatTarget.anchoredPosition, to, done));
    }

    private IEnumerator FloatRoutine(Vector2 from, Vector2 to, Action done)
    {
        floatTarget.anchoredPosition = from;

        if (floatDuration <= 0f)
        {
            floatTarget.anchoredPosition = to;
            done?.Invoke();
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / floatDuration));
            floatTarget.anchoredPosition = Vector2.LerpUnclamped(from, to, k);
            yield return null;
        }

        floatTarget.anchoredPosition = to;
        floatCoroutine = null;
        done?.Invoke();
    }
}
