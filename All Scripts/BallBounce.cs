using UnityEngine;
using UnityEngine.SceneManagement;

public class BallBounce : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 lastVelocity;
    private float speed = 5f;
    private int score1 = 0;
    private int score2 = 0;
    private Vector2 targetPosition;
    private Vector2 currentPosition;
    private float lerpSpeed = 25f;
    private bool hasReceivedPosition = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentPosition = Vector2.zero;
        targetPosition = Vector2.zero;
        Debug.Log($"BallBounce активирован: Ball на позиции: {transform.position}");
    }

    private void Start()
    {
        if (NetworkManager.IsHost)
        {
            ResetBall();
        }
        else
        {
            if (rb != null)
            {
                rb.isKinematic = true;
                transform.position = Vector2.zero;
                Debug.Log("Клиент: Физика мяча отключена, ждём позицию от хоста...");
            }
        }
    }

    private void Update()
    {
        if (NetworkManager.IsHost)
        {
            lastVelocity = rb.linearVelocity;
            if (rb.linearVelocity.magnitude < 0.1f && SceneManager.GetActiveScene().name == "Pong Game")
            {
                Debug.LogWarning("Скорость мяча на хосте слишком мала, сбрасываем!");
                ResetBall();
            }
            NetworkClient.Instance.Send($"BALL:{rb.position.x:F2}:{rb.position.y:F2}");
            Debug.Log($"[Хост] Отправлено BALL: {rb.position.x:F2}, {rb.position.y:F2}");
        }
        else
        {
            if (hasReceivedPosition)
            {
                currentPosition = Vector2.Lerp(currentPosition, targetPosition, Time.deltaTime * lerpSpeed);
                transform.position = currentPosition;
                Debug.Log($"[Клиент] Интерполяция мяча: текущая позиция: {currentPosition}, целевая: {targetPosition}");
            }
            else
            {
                Debug.Log("[Клиент] Ожидаем первую позицию мяча от хоста...");
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!NetworkManager.IsHost) return;

        if (collision.gameObject.CompareTag("TopWall") || collision.gameObject.CompareTag("BottomWall"))
        {
            Vector2 newVelocity = Vector2.Reflect(lastVelocity.normalized, collision.contacts[0].normal) * speed;
            rb.linearVelocity = newVelocity;
            NetworkClient.Instance.Send($"BALL:{rb.position.x:F2}:{rb.position.y:F2}");
            Debug.Log($"Мячик отскочил от {collision.gameObject.tag}. Новая скорость: {rb.linearVelocity}");
        }
        else if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            Vector2 newVelocity = Vector2.Reflect(lastVelocity.normalized, collision.contacts[0].normal) * speed;
            rb.linearVelocity = newVelocity;
            NetworkClient.Instance.Send($"BALL:{rb.position.x:F2}:{rb.position.y:F2}");
            Debug.Log($"Мячик отскочил от {collision.gameObject.tag}. Новая скорость: {rb.linearVelocity}");
        }
        else if (collision.gameObject.CompareTag("LeftWall") || collision.gameObject.CompareTag("RightWall"))
        {
            if (collision.gameObject.CompareTag("LeftWall"))
            {
                score2++;
                Debug.Log($"Счёт увеличен для Player2: {score2}");
            }
            else if (collision.gameObject.CompareTag("RightWall"))
            {
                score1++;
                Debug.Log($"Счёт увеличен для Player1: {score1}");
            }
            NetworkClient.Instance.Send($"SCORE:{score1}:{score2}");
            ResetBall();
        }
    }

    public void ResetBall()
    {
        if (!NetworkManager.IsHost) return;

        transform.position = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        float randomAngle = Random.Range(-45f, 45f);
        Vector2 direction = new Vector2(Random.Range(0.7f, 1f) * (Random.value > 0.5f ? 1 : -1), Mathf.Tan(randomAngle * Mathf.Deg2Rad)).normalized;
        rb.linearVelocity = direction * speed;

        Debug.Log($"Мячик сброшен. Начальная скорость: {rb.linearVelocity}");
        NetworkClient.Instance.Send($"BALL:{transform.position.x:F2}:{transform.position.y:F2}");
    }

    public void UpdateBallPosition(float x, float y)
    {
        if (NetworkManager.IsHost) return;

        targetPosition = new Vector2(x, y);
        if (!hasReceivedPosition)
        {
            currentPosition = targetPosition;
            transform.position = currentPosition;
            hasReceivedPosition = true;
            Debug.Log($"[Клиент] Первая позиция мяча получена: {targetPosition}, hasReceivedPosition: {hasReceivedPosition}");
        }
        Debug.Log($"[Клиент] Обновлена целевая позиция мяча: {targetPosition}");
    }

    public void UpdateScore(int newScore1, int newScore2)
    {
        score1 = newScore1;
        score2 = newScore2;
        Debug.Log($"Обновлён счёт: Player1 = {score1}, Player2 = {score2}");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateScoreDisplay(score1, score2);
        }
    }

    public void GetScore(out int s1, out int s2)
    {
        s1 = score1;
        s2 = score2;
    }
}