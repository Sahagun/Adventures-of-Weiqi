using EditorAttributes;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Title("Data")]
    public DialogueConfig config;

    [Title("UI")]
    public GameObject dialogueRoot;
    public Image characterImage;
    public TMP_Text characterName;
    public TMP_Text dialogueText;
    public Button skipButton;

    [Title("Typing")]
    [Min(0f)] public float characterRevealDelay = 0.03f;

    [Title("Events")]
    public UnityEvent<int> onDialogueFinished;

    private Coroutine typingCoroutine;
    [SerializeField, ReadOnly] int currentDialogueIndex;
    [SerializeField, ReadOnly] bool isDialogueActive;
    [SerializeField, ReadOnly] bool isTyping;
    [SerializeField, ReadOnly] bool hasInvokedFinishEvent;

    void Start()
    {
        HideDialogue();
    }

    private void OnEnable()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(AdvanceDialogue);
    }

    private void OnDisable()
    {
        if (skipButton != null)
            skipButton.onClick.RemoveListener(AdvanceDialogue);

        StopTyping();
        isDialogueActive = false;
        isTyping = false;
        hasInvokedFinishEvent = false;
    }

    [Button]
    public void ShowDialogue(int index)
    {
        if (isDialogueActive)
            return;

        if (config == null)
        {
            Debug.LogWarning("DialogueManager is missing a DialogueConfig.", this);
            return;
        }

        if (index < 0 || index >= config.entries.Count)
        {
            Debug.LogWarning($"Dialogue index {index} is out of range.", this);
            return;
        }

        currentDialogueIndex = index;
        (DialogueCharacter character, DialogueConfig.DialogueEntry entry) = config.GetDialogue(index);

        isDialogueActive = true;
        isTyping = true;
        hasInvokedFinishEvent = false;

        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        if (characterImage != null)
        {
            characterImage.sprite = character != null ? character.icon : null;
            characterImage.enabled = characterImage.sprite != null;
        }

        if (characterName != null)
            characterName.text = character != null ? character.title : entry.character;

        if (dialogueText != null)
        {
            dialogueText.text = entry.dialogue;
            dialogueText.maxVisibleCharacters = 0;
        }

        StopTyping();
        typingCoroutine = StartCoroutine(TypeDialogue());
    }

    public void AdvanceDialogue()
    {

        if (isTyping)
        {
            CompleteTypingImmediately();
            return;
        }

        HideDialogue();
    }

    public void HideDialogue()
    {

        StopTyping();
        isTyping = false;
        isDialogueActive = false;

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);
    }

    private IEnumerator TypeDialogue()
    {
        if (dialogueText == null)
        {
            FinishTyping();
            yield break;
        }

        dialogueText.ForceMeshUpdate();
        int visibleCharacterCount = dialogueText.textInfo.characterCount;

        if (visibleCharacterCount <= 0 || characterRevealDelay <= 0f)
        {
            dialogueText.maxVisibleCharacters = visibleCharacterCount;
            FinishTyping();
            yield break;
        }

        WaitForSeconds delay = new(characterRevealDelay);

        for (int i = 1; i <= visibleCharacterCount; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return delay;
        }

        FinishTyping();
    }

    private void CompleteTypingImmediately()
    {
        if (!isTyping)
            return;

        StopTyping();

        if (dialogueText != null)
        {
            dialogueText.ForceMeshUpdate();
            dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        }

        FinishTyping();
    }

    private void FinishTyping()
    {
        typingCoroutine = null;
        isTyping = false;
        isDialogueActive = false;

        if (hasInvokedFinishEvent)
            return;

        hasInvokedFinishEvent = true;
        onDialogueFinished?.Invoke(currentDialogueIndex);
    }

    private void StopTyping()
    {
        if (typingCoroutine == null)
            return;

        StopCoroutine(typingCoroutine);
        typingCoroutine = null;
    }
}
