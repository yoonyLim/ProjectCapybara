using UnityEditor.Recorder.Input;
using UnityEngine;

public class CollectMovingGlowingOrb : CollectGlowingOrb
{
    [SerializeField] private Transform posA;
    [SerializeField] private Transform posB;
    [SerializeField] private float speed = 5f;

    private Vector3 currentTargetPos;
    
    void Start()
    {
        currentTargetPos = posA.position;
    }
    
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        
        transform.position = Vector3.MoveTowards(transform.position, currentTargetPos, speed * Time.deltaTime);

        if (transform.position == currentTargetPos)
        {
            currentTargetPos = currentTargetPos != posB.position ? posB.position : posA.position;
        }
    }
}
