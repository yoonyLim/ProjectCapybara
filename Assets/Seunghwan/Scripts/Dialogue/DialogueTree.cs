using UnityEngine;

[CreateAssetMenu(fileName = "MyDialogueTree", menuName = "Dialogue/Tree")]
public class DialogueTree : ScriptableObject
{
    public DialogueNode RootNode;
}
