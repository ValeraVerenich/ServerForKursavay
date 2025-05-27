using UnityEngine;

public class Player1 : MonoBehaviour
{
    private float speed = 5f;
    private float moveInput;
    private float boundaryY = 4f;
    public Rigidbody2D opponentRacket; // Изменён с GameObject на Rigidbody2D

    private void Start()
    {
        opponentRacket = GameObject.FindWithTag("Player2")?.GetComponent<Rigidbody2D>();
        if (opponentRacket == null)
        {
            Debug.LogError("Player2 не найден в сцене! Убедитесь, что объект с тегом 'Player2' существует.");
        }
    }

    private void Update()
    {
        if (NetworkManager.IsHost)
        {
            moveInput = Input.GetAxisRaw("Vertical");
            Vector3 newPosition = transform.position + new Vector3(0, moveInput * speed * Time.deltaTime, 0);
            newPosition.y = Mathf.Clamp(newPosition.y, -boundaryY, boundaryY);
            transform.position = newPosition;
            NetworkClient.Instance.Send($"P1:{transform.position.y:F2}");
        }
    }

    public void UpdatePosition(float y)
    {
        if (!NetworkManager.IsHost)
        {
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
            Debug.Log($"Клиент обновил позицию Player1: {y}");
        }
    }
}