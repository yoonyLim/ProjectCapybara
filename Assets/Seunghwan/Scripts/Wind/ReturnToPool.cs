using UnityEngine;
using UnityEngine.Pool;

public class ReturnToPool : MonoBehaviour
{
    private ParticleSystem ps;
    private IObjectPool<ParticleSystem> pool;

    public void SetPool(IObjectPool<ParticleSystem> inPool)
    {
        pool = inPool;
    }

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    private void OnParticleSystemStopped()
    {
        pool.Release(ps);
    }
}