using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private GameObject choiceButtonPrefab;

    [SerializeField] private Transform dialogueChoicesContainer;

	public AudioClip[] MidtoneBeeps;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Creates choice buttons inside the UI canvas
    public void CreateChoiceButtons(List<DialogueChoice> choices, Action<DialogueChoice> onChoiceSelected)
    {
        DeleteChoiceButtons();
        
        foreach (DialogueChoice choice in choices)
        {
            GameObject createdChoice = Instantiate(choiceButtonPrefab, dialogueChoicesContainer);
            TMP_Text choiceText = createdChoice.GetComponentInChildren<TMP_Text>();
            choiceText.text = choice.ChoiceText;
            
            Button choiceButton = createdChoice.GetComponent<Button>();
            choiceButton.onClick.AddListener(() =>
            {
                onChoiceSelected?.Invoke(choice);
            });
        }
    }

    public void DeleteChoiceButtons()
    {
        foreach (Transform child in dialogueChoicesContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
