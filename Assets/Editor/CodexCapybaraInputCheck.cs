using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class CodexCapybaraInputCheck
{
    public static void InspectRain()
    {
        EditorSceneManager.OpenScene("Assets/00Scenes/FinishedLevels/Level1_Capy.unity");
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Debug.Log($"CAPY_RAIN_COUNTS: renderers={renderers.Length}; shadowCasters={renderers.Count(r => r.shadowCastingMode != ShadowCastingMode.Off)}; realtimeLights={lights.Count(l => l.lightmapBakeType != LightmapBakeType.Baked)}; shadowLights={lights.Count(l => l.shadows != LightShadows.None)}");
        foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            var data = camera.GetUniversalAdditionalCameraData();
            Debug.Log($"CAPY_RAIN_CAMERA: {camera.name}; enabled={camera.enabled}; depth={camera.depth}; post={data.renderPostProcessing}; shadows={data.renderShadows}; target={camera.targetTexture?.name}; renderType={data.renderType}; far={camera.farClipPlane}");
        }
        foreach (var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Debug.Log($"CAPY_RAIN_PARTICLES: {ps.name}; active={ps.gameObject.activeInHierarchy}; max={ps.main.maxParticles}; rate={ps.emission.rateOverTime.constantMax}; collision={ps.collision.enabled}; noise={ps.noise.enabled}; collisionQuality={ps.collision.quality}");
        foreach (var component in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(c => c != null && c.GetType().Name == "CozyWeather"))
        {
            Debug.Log("CAPY_RAIN_WEATHER: " + JsonUtility.ToJson(component));
        }
        Debug.Log("CAPY_RAIN_INSPECT: complete (static scene configuration; no GPU timing)");
    }

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
