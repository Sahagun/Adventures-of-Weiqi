using System.Collections;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueConfig", menuName = "DialogueSystem/DialogueConfig", order = 1)]
public class DialogueConfig : ScriptableObject
{
    [System.Serializable]
    public class DialogueEntry
    {
        [Dropdown(nameof(GetCharacterNames))] public string character;
        public string dialogue;
    }

    public List<DialogueCharacter> characters = new();
    public List<DialogueEntry> entries = new();

    public (DialogueCharacter, DialogueEntry) GetDialogue(int index)
    {
        DialogueEntry entry = entries[index];
        DialogueCharacter character = characters.Find(c => c.title == entry.character);
        return (character, entry);
    }

    public List<string> GetCharacterNames()
    {
        List<string> names = new();
        foreach (DialogueCharacter chara in characters)
        {
            names.Add(chara.title);
        }
        return names;
    }

    
}
