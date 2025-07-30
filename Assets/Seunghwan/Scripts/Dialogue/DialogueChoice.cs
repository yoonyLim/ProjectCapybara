using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    // Text to be shown on the choice button.
    public string ChoiceText;
    // The next node to proceed if this choice is selected.
    public DialogueNode NextNode;
}
