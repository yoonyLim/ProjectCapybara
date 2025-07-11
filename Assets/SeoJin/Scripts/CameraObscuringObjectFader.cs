using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// this script needs to be attached to Main Camera Object. It finds obscuring objects and make them fade.
/// </summary>
public class CameraObscuringObjectFader : MonoBehaviour
{
    public Camera cam;

    public LayerMask ObscuringLayers;
    public float CheckInterval = 0.1f;
    
    
    private GameObject player;
    private List<ObjectFader> fadedObjects = new List<ObjectFader>();
    
    void Awake()
    {
        cam = GetComponent<Camera>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        StartCoroutine(CheckForObscuringObjects());
    }


    IEnumerator CheckForObscuringObjects()
    {
        while (true)
        {
            Vector3 dir = player.transform.position - cam.transform.position;
            
            RaycastHit[] hits = Physics.RaycastAll(cam.transform.position, dir.normalized, dir.magnitude, ObscuringLayers);

            List<ObjectFader> hitFadersThisFrame = new List<ObjectFader>();
            
            foreach (RaycastHit hit in hits)
            {
                ObjectFader fader = hit.collider.GetComponent<ObjectFader>();
                if (fader) hitFadersThisFrame.Add(fader);
                
            }

            var objectsToUnfade = fadedObjects.Except(hitFadersThisFrame);
            foreach (var fader in objectsToUnfade)
            {
                fader.DoFade = false;
            }

            foreach (var fader in hitFadersThisFrame)
            {
                fader.DoFade = true;
            }
            
            fadedObjects = hitFadersThisFrame;

            yield return new WaitForSeconds(CheckInterval);
        }
    }
}


