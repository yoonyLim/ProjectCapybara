using Moko;
using UnityEngine;

public class MovementTest : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField]
    private float movementSpeed, coefficient;
    public LayerMask groundLayer;
    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private float timer;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Movement();
    }
    private void Update()
    {
        SurfaceAlignment();
    }

    private void Movement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3 (x, 0, y);
        Vector3 counterMovement = new Vector3(-rb.linearVelocity.x, 0, -rb.linearVelocity.z);

        rb.AddForce(movement * movementSpeed);
        rb.AddForce(counterMovement * coefficient);
    }

    private void SurfaceAlignment()
    {
        Ray ray = new Ray(transform.position, -Vector3.up);
        RaycastHit info = new RaycastHit();
        Quaternion RotationRef = Quaternion.Euler(0, 0, 0);


        if (Physics.Raycast(ray, out info, 5f, groundLayer))
        {
            Debug.Log(info.normal);
            RotationRef = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(Vector3.up, info.normal), animCurve.Evaluate(timer));
            transform.rotation = Quaternion.Euler(RotationRef.eulerAngles.x, transform.eulerAngles.y, RotationRef.eulerAngles.z);
            Debug.Log(Quaternion.FromToRotation(Vector3.up, info.normal));
        }
    }   

}
