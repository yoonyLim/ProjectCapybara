using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("이동 속도")]
    public float moveSpeed = 5.0f; // 카메라의 이동 속도

    [Header("회전 속도")]
    public float rotationSpeed = 100.0f; // 카메라의 회전 속도

    void Update()
    {
        // --- 카메라 이동 처리 ---

        // 전후 이동 (W, S)
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
        }

        // 좌우 이동 (A, D)
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }

        // 상하 이동 (E, Q)
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
        }


        // --- 카메라 회전 처리 ---

        // 상하 회전 (Pitch) - 위/아래 방향키
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Rotate(Vector3.right, -rotationSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
        }

        // 좌우 회전 (Yaw) - 왼쪽/오른쪽 방향키
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        // 좌우 기울기 (Roll) - Z, C 키
        if (Input.GetKey(KeyCode.Z))
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.C))
        {
            transform.Rotate(Vector3.forward, -rotationSpeed * Time.deltaTime);
        }
    }
}