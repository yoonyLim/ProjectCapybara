using System;
using UnityEngine;
using UnityEngine.Playables;

public class TriggerCinematics : MonoBehaviour
{
    public PlayableDirector timeline;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timeline.Play();
            
            GetComponent<Collider>().enabled = false;
        }
    }
}
