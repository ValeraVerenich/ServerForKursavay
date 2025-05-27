using UnityEngine;

public class BallMovement : MonoBehaviour
{
    public float speed = 8f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ��������� ��������
        LaunchBall();
    }

    public void LaunchBall()
    {
        float dirX = Random.Range(0, 2) == 0 ? -1 : 1;
        float dirY = Random.Range(-1f, 1f);
        Vector2 direction = new Vector2(dirX, dirY).normalized;
        rb.linearVelocity = direction * speed;
    }

    public void ResetBall()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = Vector2.zero;
        LaunchBall();
    }
}
