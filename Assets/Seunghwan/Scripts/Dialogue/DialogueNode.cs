using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MyDialogueNode", menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    [TextArea]
    public string DialogueText;
    public Animal.FacialAnimationType FacialAnimation;
    // public List<DialogueChoice> Choices;
    public DialogueNode NextNode;
    
}
