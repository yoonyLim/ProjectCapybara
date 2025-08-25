using System;
using TMPro;
using UnityEngine;

public class DebugCanvas : MonoBehaviour
{
    public static DebugCanvas Instance;

    public TextMeshProUGUI debugText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
