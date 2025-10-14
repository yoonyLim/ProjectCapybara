using System;
using UnityEngine;

public class Player_PROTO : MonoBehaviour
{
    [SerializeField] private Transform floorLeft;
    [SerializeField] private Transform floorMiddle;
    [SerializeField] private Transform floorRight;

    [SerializeField] private float speed = 5f;

    private Vector3 leftPos;
    private Vector3 rightPos;

    private float unitLength = 2f;


    private void Awake()
    {
        leftPos = new Vector3(transform.position.x - unitLength, transform.position.y, transform.position.z);
        rightPos = new Vector3(transform.position.x + unitLength, transform.position.y, transform.position.z);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft();
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight();
        }
        
        float xPos = transform.position.x;
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, leftPos.x, rightPos.x), transform.position.y, transform.position.z);
    }

    private void MoveLeft()
    {
        transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.left * unitLength, speed * Time.deltaTime);
    }

    private void MoveRight()
    {
        transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.right * unitLength, speed * Time.deltaTime);
    }
}
