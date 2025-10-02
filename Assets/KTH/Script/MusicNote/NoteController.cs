using UnityEngine;

public class NoteController : MonoBehaviour
{
    public float speed;
    public string noteName; // <<-- 이 줄을 추가하세요!

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (transform.position.x < -1500f)
        {
            Destroy(gameObject);
        }
    }
}