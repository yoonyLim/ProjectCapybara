using System;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;

    // Invoked when progress is advanced. e.g) FoundSuckGoat -> RescuedGoat
    public event Action OnProgressAdvance;

    public enum ProgressType
    {
        GameStart,
        ReceivedRecipeFromMom, 

        // Level 1 Flying Squirrel
        FoundInjuredSquirrel,
        HealedSquirrel,
        AcquiredCloudFlour,

        // Level 2 Bat
        FoundLostBat, 
        RescuedBat, 
        AcquiredSparklingSugar, 

        // Level 3 Mountain Goat
        FoundStuckGoat, 
        RescuedGoat,
        AcquiredWinterBerry,

        // Level 4 Siblings
        HeardSiblingsAreLost, 
        RescuedFirstSibling, 
        RescuedSecondSibling, 
        RescuedThirdSibling, 

        // Ending
        AllSiblingsRescued, 
        Ending     
        
    }
    
    private ProgressType currentProgress = ProgressType.GameStart;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    public ProgressType GetCurrentProgress()
    {
        return currentProgress;
    }

    // Use this for normal progress advances.
    public void AdvanceProgress()
    {
        // Instead of comparing with ProgressType.Ending for index range, made a more robust check.
        Array allProgressValues = Enum.GetValues(typeof(ProgressType));
        ProgressType lastProgress = (ProgressType)allProgressValues.GetValue(allProgressValues.Length - 1);
        
        if (currentProgress < lastProgress)
        {
            currentProgress++;
            OnProgressAdvance?.Invoke();
        }
        else
        {
            Debug.LogWarning("Cannot advance progress since currentProgress is at last index");
        }
    }

    // Setter for current progress. Might be useful when implementing save & load.
    public void SetCurrentProgress(ProgressType progress)
    {
        currentProgress = progress;
    }
    
}
