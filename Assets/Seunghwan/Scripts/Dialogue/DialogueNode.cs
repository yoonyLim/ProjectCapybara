using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MyDialogueNode", menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    [TextArea]
    public string DialogueText;
    public List<DialogueChoice> Choices;
    [Header("If there are no choices this is the next dialogue node")] 
    public DialogueNode NextNode;
}
