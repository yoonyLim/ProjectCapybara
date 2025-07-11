using UnityEngine;


[CreateAssetMenu(fileName = "MyDialogueTree", menuName = "Dialogue/Tree")]
public class DialogueTree : ScriptableObject
{
    // Starts with the root node dialogue.
    public DialogueNode RootNode;
}
