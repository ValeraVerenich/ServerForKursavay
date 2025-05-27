using UnityEngine;

public class Player2 : MonoBehaviour
{
    private float speed = 5f;
    private float moveInput;
    private float boundaryY = 4f;
    public Rigidbody2D opponentRacket;

    private void Start()
    {
        opponentRacket = GameObject.FindWithTag("Player1")?.GetComponent<Rigidbody2D>();
        if (opponentRacket == null)
        {
            Debug.LogError("Player1 не найден в сцене! Убедитесь, что объект с тегом 'Player1' существует.");
        }
    }

    private void Update()
    {
        if (!NetworkManager.IsHost) // Клиент управляет Player2
        {
            moveInput = Input.GetAxisRaw("Vertical");
            Vector3 newPosition = transform.position + new Vector3(0, moveInput * speed * Time.deltaTime, 0);
            newPosition.y = Mathf.Clamp(newPosition.y, -boundaryY, boundaryY);
            transform.position = newPosition;
            NetworkClient.Instance.Send($"P2:{transform.position.y:F2}");
            Debug.Log($"Клиент отправил позицию Player2: {transform.position.y:F2}");
        }
    }

    public void UpdatePosition(float y)
    {
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
        Debug.Log($"Хост обновил позицию Player2: {y}");
    }
}