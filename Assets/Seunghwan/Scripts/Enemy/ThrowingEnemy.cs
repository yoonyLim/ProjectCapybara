using UnityEngine;

public class ThrowingEnemy : MonoBehaviour
{
    [SerializeField] private bool debugSight = true;
    private State currentState;
    private Animator animator;

    private GameObject player;
    private Quaternion startRotation;

    [SerializeField] private float searchRadius = 10f;
    [SerializeField] private float searchAngle = 70f;

    private int throwHash;
    private int idleHash;
    private int deathHash;
    private float lastThrowStartTime;
    [SerializeField] private float throwCooldown = 2f;
    [SerializeField] private float throwTargetOffset = 0.15f;

    [SerializeField] private float rotationSpeed = 2f;
    
    [Header("Inspector Assignment Needed")]
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private Transform projectileSpawnPoint;
    
    enum State
    {
        Idle,
        Throwing,
        Dead
    }

    private void Awake()
    {
        startRotation = transform.rotation;
        
        animator = GetComponent<Animator>();
        throwHash = Animator.StringToHash("Throw");
        idleHash = Animator.StringToHash("Idle");
        deathHash = Animator.StringToHash("Death");
        
        // Start as idle state (^o^)=b
        currentState = State.Idle;
        
        player = GameObject.FindGameObjectWithTag("Player");
    }
    

    

    private void Update()
    {
        switch (currentState)
        {
            case State.Idle:
            {
                float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
                if (distanceToPlayer > searchRadius)
                {
                    if (transform.rotation != startRotation)
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, startRotation, rotationSpeed);
                    }
                    break;
                }

                float angleToPlayer = Vector3.Angle(transform.forward, player.transform.position - transform.position);

                if (angleToPlayer > searchAngle / 2)
                {
                    if (transform.rotation != startRotation)
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, startRotation, rotationSpeed);
                    }
                    break;
                }
                
                Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
                dirToPlayer.y = 0;
                Quaternion lookRotation = Quaternion.LookRotation(dirToPlayer, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed);

                if (Time.time - lastThrowStartTime < throwCooldown) break;
                
                lastThrowStartTime = Time.time;
                animator.CrossFadeInFixedTime(throwHash, 0.15f);
                currentState = State.Throwing;
                break;
            }
            case State.Throwing:
            {
                Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
                dirToPlayer.y = 0;
                Quaternion lookRotation = Quaternion.LookRotation(dirToPlayer, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed);
                
                // TODO: If player headbutts or jumps on top go to Dead state.
                break;
            }
            case State.Dead:
            {
                break;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (currentState == State.Dead) return;

        if (collision.collider.CompareTag("Player"))
        {
            animator.CrossFadeInFixedTime(deathHash, 0.1f);
            currentState = State.Dead;
        }
    }

    private void LaunchProjectile()
    {
        Vector3 dirToPlayer = (player.transform.position - projectileSpawnPoint.position).normalized;
        Quaternion rotationToPlayer = Quaternion.LookRotation(dirToPlayer + Vector3.up * throwTargetOffset);
        Instantiate(projectilePrefab, projectileSpawnPoint.position, rotationToPlayer);
        
    }

    private void OnThrowAnimationEnd()
    {
        if (currentState == State.Dead) return;
        
        animator.CrossFadeInFixedTime(idleHash, 0.15f);
        currentState = State.Idle;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debugSight) return;
        
        
        
        Vector3 coneBaseCenter = transform.position + transform.forward * 
            Mathf.Cos(Mathf.Deg2Rad * (searchAngle / 2)) * searchRadius;
        
        float coneRadius = searchRadius * Mathf.Sin(Mathf.Deg2Rad * (searchAngle / 2));
        
        
        UnityEditor.Handles.color = Color.yellow;
        
        UnityEditor.Handles.DrawWireDisc(coneBaseCenter, transform.forward, coneRadius);
        
        Vector3 horizontalFrom = Quaternion.AngleAxis(-searchAngle / 2, transform.up) * transform.forward;
        UnityEditor.Handles.DrawWireArc(transform.position, transform.up, horizontalFrom, searchAngle, searchRadius);
        
        Vector3 verticalFrom = Quaternion.AngleAxis(-searchAngle / 2, transform.right) * transform.forward;
       
        UnityEditor.Handles.DrawWireArc(transform.position, transform.right, verticalFrom, searchAngle, searchRadius);
        
        UnityEditor.Handles.DrawLine(transform.position, coneBaseCenter + transform.up * coneRadius);
        UnityEditor.Handles.DrawLine(transform.position, coneBaseCenter - transform.up * coneRadius);
        UnityEditor.Handles.DrawLine(transform.position, coneBaseCenter + transform.right * coneRadius);
        UnityEditor.Handles.DrawLine(transform.position, coneBaseCenter - transform.right * coneRadius);
    }
#endif
    
}
