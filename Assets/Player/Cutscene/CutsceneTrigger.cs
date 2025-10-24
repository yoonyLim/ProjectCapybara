using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;

public class CutsceneTrigger : MonoBehaviour
{
    public PlayableDirector timeline;
    public Image img; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        StartCoroutine("PlayCutScene");
    }


    IEnumerator PlayCutScene()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / 0.6f;
            float a = Mathf.SmoothStep(0f, 1f, t);
            var c = img.color;
            c.a = a;
            img.color = c;
            yield return null;
        }

        timeline.Play();
        yield return null;
    }
    
}

