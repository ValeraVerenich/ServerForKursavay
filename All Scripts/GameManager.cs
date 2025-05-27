using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject pauseMenu;
    public int player1Score = 0;
    public int player2Score = 0;
    public Text scoreText;

    private GameObject player1Racket;
    private GameObject player2Racket;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeRackets();
        InitializeBorders();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Pong Game")
        {
            if (NetworkManager.IsHost && NetworkClient.Instance.IsConnected())
            {
                NetworkClient.Instance.score1 = 0;
                NetworkClient.Instance.score2 = 0;
                StartCoroutine(InitializeGame());
                NetworkClient.Instance.Send($"SCORE:{0}:{0}");
                if (BallManager.Instance != null && BallManager.Instance.ball != null)
                {
                    NetworkClient.Instance.Send($"BALL:{BallManager.Instance.ball.transform.position.x:F2}:{BallManager.Instance.ball.transform.position.y:F2}");
                }
            }
            else if (!NetworkManager.IsHost && NetworkClient.Instance.IsConnected())
            {
                NetworkClient.Instance.Send("REQUEST_INITIAL_STATE");
            }
        }
    }

    private IEnumerator InitializeGame()
    {
        yield return new WaitForSeconds(0.1f);
        ResetBall();
    }

    private void InitializeRackets()
    {
        player1Racket = GameObject.FindWithTag("Player1");
        if (player1Racket == null)
        {
            Debug.Log("Ракетка игрока 1 не найдена, создаём новую.");
            player1Racket = new GameObject("Player1Racket");
            player1Racket.tag = "Player1";
            Rigidbody2D rb = player1Racket.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            player1Racket.AddComponent<Player1>();
            player1Racket.transform.position = new Vector2(-8f, 0f);

            BoxCollider2D collider = player1Racket.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.5f, 2f);

            SpriteRenderer sr = player1Racket.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Square");
            sr.color = Color.green;
            player1Racket.transform.localScale = new Vector3(0.5f, 2f, 1f);
        }

        player2Racket = GameObject.FindWithTag("Player2");
        if (player2Racket == null)
        {
            Debug.Log("Ракетка игрока 2 не найдена, создаём новую.");
            player2Racket = new GameObject("Player2Racket");
            player2Racket.tag = "Player2";
            Rigidbody2D rb = player2Racket.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            player2Racket.AddComponent<Player2>();
            player2Racket.transform.position = new Vector2(8f, 0f);

            BoxCollider2D collider = player2Racket.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.5f, 2f);

            SpriteRenderer sr = player2Racket.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Square");
            sr.color = Color.red;
            player2Racket.transform.localScale = new Vector3(0.5f, 2f, 1f);
        }

        var p1Script = player1Racket.GetComponent<Player1>();
        var p2Script = player2Racket.GetComponent<Player2>();
        if (p1Script != null && p2Script != null)
        {
            p1Script.opponentRacket = player2Racket.GetComponent<Rigidbody2D>();
            p2Script.opponentRacket = player1Racket.GetComponent<Rigidbody2D>();
        }
    }

    private void InitializeBorders()
    {
        var leftBorder = GameObject.FindWithTag("LeftBorder");
        if (leftBorder == null)
        {
            Debug.Log("Левая граница не найдена, создаём новую.");
            leftBorder = new GameObject("LeftBorder");
            leftBorder.tag = "LeftBorder";
            var collider = leftBorder.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 10f);
            leftBorder.transform.position = new Vector2(-10f, 0f);
        }

        var rightBorder = GameObject.FindWithTag("RightBorder");
        if (rightBorder == null)
        {
            Debug.Log("Правая граница не найдена, создаём новую.");
            rightBorder = new GameObject("RightBorder");
            rightBorder.tag = "RightBorder";
            var collider = rightBorder.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 10f);
            rightBorder.transform.position = new Vector2(10f, 0f);
        }
    }

    private void ResetBall()
    {
        if (SceneManager.GetActiveScene().name == "Pong Game" && BallManager.Instance != null)
        {
            BallManager.Instance.ResetBall();
            if (NetworkManager.IsHost && BallManager.Instance.ball != null)
            {
                NetworkClient.Instance.Send($"BALL:{BallManager.Instance.ball.transform.position.x:F2}:{BallManager.Instance.ball.transform.position.y:F2}");
            }
        }
    }

    public void ResetScores()
    {
        player1Score = 0;
        player2Score = 0;
        NetworkClient.Instance.ResetScores();
        UpdateScoreDisplay(player1Score, player2Score);
    }

    public void TogglePauseMenu(bool show)
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(show);
    }

    public void OnPausePressed()
    {
        if (!NetworkManager.IsHost) return;
        TogglePauseMenu(true);
        NetworkClient.Instance.Send("PAUSE");
    }

    public void OnResumePressed()
    {
        if (!NetworkManager.IsHost) return;
        TogglePauseMenu(false);
        NetworkClient.Instance.Send("UNPAUSE");
        if (BallManager.Instance != null && BallManager.Instance.ball != null)
        {
            NetworkClient.Instance.Send($"BALL:{BallManager.Instance.ball.transform.position.x:F2}:{BallManager.Instance.ball.transform.position.y:F2}");
            Debug.Log("[Хост] Отправлено положение мяча после UNPAUSE");
        }
    }

    public void UpdateScoreDisplay(int newScore1, int newScore2)
    {
        player1Score = newScore1;
        player2Score = newScore2;
        Debug.Log($"Обновлён счёт: Player1 = {player1Score}, Player2 = {player2Score}");
        if (scoreText != null)
        {
            scoreText.text = $"Player1: {player1Score} - Player2: {player2Score}";
        }
    }
}