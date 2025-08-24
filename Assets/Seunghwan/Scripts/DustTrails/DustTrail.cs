using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

public class DustTrail : MonoBehaviour
{
    private VisualEffect vfx;
    public IObjectPool<VisualEffect> Pool;

    private float vfxDuration;

    private void Awake()
    {
        vfx = GetComponent<VisualEffect>();
        
    }

    private void Update()
    {
        if (vfx.aliveParticleCount == 0)
        {
            Pool.Release(vfx);
        }
    }
}
