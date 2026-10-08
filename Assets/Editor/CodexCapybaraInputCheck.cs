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

    public static void InspectWeatherShells()
    {
        EditorSceneManager.OpenScene("Assets/00Scenes/FinishedLevels/Level1_Capy.unity");
        var weather = Object.FindFirstObjectByType<DistantLands.Cozy.CozyWeather>();
        weather.UpdateSkydomePositionAndScale();
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (r.transform.IsChildOf(weather.transform) || r.name.Contains("Sphere"))
                Debug.Log($"CAPY_SHELL: path={GetPath(r.transform)}; active={r.gameObject.activeInHierarchy}; scale={r.transform.lossyScale}; bounds={r.bounds}; materials={string.Join(",", r.sharedMaterials.Where(m => m != null).Select(m => m.name + ":" + m.shader.name + ":queue=" + m.renderQueue))}; shadows={r.shadowCastingMode}");
        }
        foreach (var volume in Object.FindObjectsByType<DistantLands.Cozy.CozyVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Debug.Log("CAPY_VOLUME: " + GetPath(volume.transform) + "; " + EditorJsonUtility.ToJson(volume));
        foreach (var profile in AssetDatabase.FindAssets("t:WeatherProfile").Select(AssetDatabase.GUIDToAssetPath))
        {
            var p = AssetDatabase.LoadAssetAtPath<DistantLands.Cozy.Data.WeatherProfile>(profile);
            if (p.name.Contains("Rain") || p.name.Contains("Storm")) Debug.Log("CAPY_PROFILE: " + profile + "; " + EditorJsonUtility.ToJson(p));
        }
        Debug.Log("CAPY_SHELL_INSPECT: complete");
    }

    private static string GetPath(Transform t) => t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;

    public static void RenderWeatherOcclusion()
    {
        EditorSceneManager.OpenScene("Assets/00Scenes/FinishedLevels/Level1_Capy.unity");
        var args = System.Environment.GetCommandLineArgs();
        int index = System.Array.IndexOf(args, "-capyCapturePath");
        string output = args[index + 1];
        System.IO.Directory.CreateDirectory(output);
        var weather = Object.FindFirstObjectByType<DistantLands.Cozy.CozyWeather>();
        weather.ResetQuality();
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            r.enabled = r == weather.skyMesh || r == weather.cloudMesh;
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)) canvas.enabled = false;
        var camera = new GameObject("Weather distance diagnostic camera").AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 5000, 0);
        camera.transform.rotation = Quaternion.identity;
        camera.nearClipPlane = 1;
        camera.farClipPlane = 2000;
        camera.fieldOfView = 60;
        camera.aspect = 1;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        var data = camera.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = false;
        data.renderShadows = false;
        weather.cozyCamera = camera;
        weather.UpdateSkydomePositionAndScale();
        weather.fogDensity = 0;
        weather.cumulus = 2;
        weather.UpdateShaderVariables();
        var targets = new[] { (name: "near", distance: 1000f, y: -200f, color: Color.green), (name: "far", distance: 1950f, y: 200f, color: Color.red), (name: "beyondClip", distance: 2200f, y: 0f, color: Color.blue) };
        bool passed = true;
        foreach (var target in targets)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = target.name;
            quad.transform.position = camera.transform.position + new Vector3(0, target.y, target.distance);
            quad.transform.localScale = Vector3.one * 180;
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", target.color);
            quad.GetComponent<Renderer>().sharedMaterial = material;
        }
        var rt = new RenderTexture(512, 512, 24);
        rt.Create();
        // Initialize the pipeline before submitting a URP render request.
        camera.targetTexture = rt;
        camera.Render();
        RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = rt });
        RenderTexture.active = rt;
        var image = new Texture2D(512, 512, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        image.Apply();
        System.IO.File.WriteAllBytes(System.IO.Path.Combine(output, "weather-occlusion.png"), image.EncodeToPNG());
        foreach (var target in targets)
        {
            var position = camera.WorldToViewportPoint(camera.transform.position + new Vector3(0, target.y, target.distance));
            var pixel = image.GetPixel((int)(position.x * 512), (int)(position.y * 512));
            Debug.Log($"CAPY_WEATHER_PIXEL: target={target.name}; distance={target.distance}; pixel={pixel}; skyQueue={weather.skyMesh.sharedMaterial.renderQueue}; cloudQueue={weather.cloudMesh.sharedMaterial.renderQueue}");
            passed &= target.name == "near" ? pixel.g > 0.9f && pixel.r < 0.1f :
                target.name == "far" ? pixel.r > 0.9f && pixel.g < 0.1f : pixel.b < 0.9f;
        }
        RenderTexture.active = null;
        Debug.Log("CAPY_WEATHER_RENDER: complete");
        if (System.Array.IndexOf(args, "-capyExpectWeatherFixed") >= 0 && !passed)
            throw new System.Exception("Weather dome occlusion regression: near/far/clip pixel comparison failed.");
        Debug.Log("CAPY_WEATHER_RESULT: " + (passed ? "passed" : "occlusion reproduced"));
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
