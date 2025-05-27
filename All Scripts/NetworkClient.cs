using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
    public static NetworkClient Instance { get; private set; }
    private TcpClient client;
    private NetworkStream stream;
    public int score1 = 0;
    public int score2 = 0;
    public float player1Y = 0f;
    public float player2Y = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("NetworkClient ������� � DontDestroyOnLoad");
            ConnectToServer();
        }
        else if (Instance != this)
        {
            Debug.LogWarning("�������� NetworkClient ������, ���������...");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (!IsConnected())
        {
            ConnectToServer();
        }
    }

    public void ConnectToServer()
    {
        if (client == null || !client.Connected)
        {
            try
            {
                client = new TcpClient("127.0.0.1", 7777);
                stream = client.GetStream();
                Debug.Log("NetworkClient ��������������� � ��������� � �������.");
                Thread receiveThread = new Thread(ReceiveMessages);
                receiveThread.Start();

                if (!NetworkManager.IsHost)
                {
                    Send("REQUEST_INITIAL_STATE");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("������ ������������� NetworkClient: " + e.Message);
            }
        }
    }

    public bool IsConnected()
    {
        return client != null && client.Connected && stream != null;
    }

    public void Send(string message)
    {
        if (stream == null || !stream.CanWrite)
        {
            Debug.LogWarning("���������� �� �����������. ���������������...");
            ConnectToServer();
            if (stream == null)
            {
                Debug.LogError("�� ������� ������������ ����������.");
                return;
            }
        }
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            stream.Write(data, 0, data.Length);
            Debug.Log($"���������� �������: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"�� ������� ��������� ���������: {e.Message}");
        }
    }

    private void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (IsConnected())
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    Debug.Log($"[������] �������� �� �������: {message}");
                    ProcessMessage(message);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"������ ��� ��������� ���������: {e.Message}");
                break;
            }
        }
    }

    private void ProcessMessage(string message)
    {
        string[] parts = message.Split(':');
        if (string.IsNullOrEmpty(message) || parts.Length < 1) return;

        try
        {
            switch (parts[0])
            {
                case "P1":
                    if (parts.Length == 2 && float.TryParse(parts[1], out float player1YValue))
                    {
                        player1Y = player1YValue;
                        Debug.Log($"�������� P1: {player1YValue} (����: {(NetworkManager.IsHost ? "����" : "������")})");
                        var player1 = GameObject.FindWithTag("Player1");
                        if (player1 != null)
                        {
                            var player1Script = player1.GetComponent<Player1>();
                            if (player1Script != null)
                            {
                                player1Script.UpdatePosition(player1YValue);
                                Debug.Log($"��������� ������� Player1 ��: {player1YValue}");
                            }
                        }
                    }
                    break;

                case "P2":
                    if (parts.Length == 2 && float.TryParse(parts[1], out float player2YValue))
                    {
                        player2Y = player2YValue;
                        Debug.Log($"�������� P2: {player2YValue} (����: {(NetworkManager.IsHost ? "����" : "������")})");
                        var player2 = GameObject.FindWithTag("Player2");
                        if (player2 != null)
                        {
                            var player2Script = player2.GetComponent<Player2>();
                            if (player2Script != null)
                            {
                                player2Script.UpdatePosition(player2YValue);
                                Debug.Log($"��������� ������� Player2 ��: {player2YValue}");
                            }
                        }
                    }
                    break;

                case "BALL":
                    if (parts.Length >= 3)
                    {
                        float ballX = float.Parse(parts[1]);
                        float ballY = float.Parse(parts[2]);
                        Debug.Log($"[������] �������� BALL: {ballX}, {ballY} (����: {(NetworkManager.IsHost ? "����" : "������")})");
                        if (BallManager.Instance == null)
                        {
                            Debug.LogWarning("BallManager ����������� � �����!");
                            return;
                        }
                        if (BallManager.Instance.ball == null)
                        {
                            BallManager.Instance.InitializeBall();
                            Debug.Log("������: ��� ��������������� ��-�� ����������.");
                        }
                        var ballScript = BallManager.Instance.ball.GetComponent<BallBounce>();
                        if (ballScript != null)
                        {
                            ballScript.UpdateBallPosition(ballX, ballY);
                            Debug.Log($"������: ������ UpdateBallPosition � ������������ ({ballX}, {ballY})");
                        }
                        else
                        {
                            Debug.LogError("BallBounce �� ������ �� ������!");
                        }
                    }
                    break;

                case "SCORE":
                    if (parts.Length == 3 && int.TryParse(parts[1], out int newScore1) && int.TryParse(parts[2], out int newScore2))
                    {
                        score1 = newScore1;
                        score2 = newScore2;
                        Debug.Log($"�������� SCORE: {score1}, {score2}");
                        if (BallManager.Instance != null && BallManager.Instance.ball == null)
                        {
                            BallManager.Instance.InitializeBall();
                        }
                        if (BallManager.Instance != null)
                        {
                            var ballScriptScore = BallManager.Instance.ball.GetComponent<BallBounce>();
                            if (ballScriptScore != null)
                            {
                                ballScriptScore.UpdateScore(score1, score2);
                            }
                        }
                    }
                    break;

                case "REQUEST_INITIAL_STATE":
                    if (NetworkManager.IsHost)
                    {
                        Debug.Log("������� ������ �� ��������� ���������, ���������� ������...");
                        Send($"SCORE:{score1}:{score2}");
                        if (BallManager.Instance != null && BallManager.Instance.ball == null)
                        {
                            BallManager.Instance.InitializeBall();
                        }
                        if (BallManager.Instance != null && BallManager.Instance.ball != null)
                        {
                            Rigidbody2D rb = BallManager.Instance.ball.GetComponent<Rigidbody2D>();
                            if (rb != null)
                            {
                                Send($"BALL:{rb.position.x:F2}:{rb.position.y:F2}");
                                Debug.Log($"���� �������� ��������� ��������� ������: ({rb.position.x:F2}, {rb.position.y:F2})");
                            }
                        }
                    }
                    break;

                case "PAUSE":
                    Debug.Log("�������� PAUSE");
                    GameManager.Instance?.TogglePauseMenu(true);
                    break;

                case "UNPAUSE":
                    Debug.Log("�������� UNPAUSE");
                    GameManager.Instance?.TogglePauseMenu(false);
                    if (!NetworkManager.IsHost)
                    {
                        Send("REQUEST_INITIAL_STATE");
                    }
                    break;

                case "GAMEOVER":
                    Debug.Log("�������� GAMEOVER");
                    break;

                case "RESET_SCORES":
                    Debug.Log("�������� RESET_SCORES");
                    score1 = 0;
                    score2 = 0;
                    GameManager.Instance?.ResetScores();
                    break;

                case "MAINMENU":
                    Debug.Log("�������� MAINMENU");
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                    break;

                default:
                    Debug.LogWarning($"����������� ���������: {message}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"������ �������� ��������� '{message}': {e.Message}");
        }
    }

    public void ResetScores()
    {
        score1 = 0;
        score2 = 0;
        Send($"SCORE:{score1}:{score2}");
    }

    private void OnDestroy()
    {
        if (client != null)
        {
            stream?.Close();
            client.Close();
        }
    }
}