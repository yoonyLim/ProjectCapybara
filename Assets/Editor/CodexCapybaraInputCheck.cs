using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public static class CodexCapybaraInputCheck
{
    public static void Play()
    {
        EditorSceneManager.OpenScene("Assets/00Scenes/FinishedLevels/Level1_Capy.unity");
        EditorApplication.EnterPlaymode();
    }

    public static void Inspect()
    {
        EditorSceneManager.OpenScene("Assets/00Scenes/FinishedLevels/Level1_Capy.unity");
        foreach (var system in Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Debug.Log($"CAPY_INPUT_SYSTEM: {system.name}; active={system.gameObject.activeInHierarchy}; enabled={system.enabled}");
            foreach (var module in system.GetComponents<InputSystemUIInputModule>())
            {
                Debug.Log($"CAPY_INPUT_MODULE: enabled={module.enabled}; asset={module.actionsAsset?.name}; move={module.move?.action?.name ?? "NULL"}; submit={module.submit?.action?.name ?? "NULL"}; cancel={module.cancel?.action?.name ?? "NULL"}; point={module.point?.action?.name ?? "NULL"}; click={module.leftClick?.action?.name ?? "NULL"}");
            }
        }
        foreach (var manager in Object.FindObjectsByType<UIManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var serialized = new SerializedObject(manager);
            var button = serialized.FindProperty("startMenuFirstButton").objectReferenceValue as GameObject;
            Debug.Log($"CAPY_INPUT_MENU: active={manager.gameObject.activeInHierarchy}; firstButton={button?.name}; buttonActive={button?.activeInHierarchy}");
        }
        foreach (var reference in AssetDatabase.LoadAllAssetsAtPath("Assets/02Configurations/Inputs/CapybaraInput.inputactions").OfType<InputActionReference>())
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(reference, out string guid, out long id);
            Debug.Log($"CAPY_ACTION_REFERENCE: {reference.name}; id={id}; bindings={string.Join(",", reference.action.bindings.Select(binding => binding.path))}");
        }
        Debug.Log("CAPY_INPUT_INSPECT: complete");
    }
}
