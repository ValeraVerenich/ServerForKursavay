using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void StartAsHost()
    {
        NetworkManager.IsHost = true; // Устанавливаем роль хоста напрямую

        if (NetworkClient.Instance != null)
        {
            NetworkClient.Instance.ConnectToServer();
            if (NetworkClient.Instance.IsConnected())
            {
                NetworkClient.Instance.Send("CONNECT");
                DestroyExistingBall();
                SceneManager.LoadScene("Pong Game");
            }
            else
            {
                Debug.LogError("Не удалось подключиться к серверу!");
            }
        }
        else
        {
            Debug.LogError("NetworkClient.Instance отсутствует! Убедитесь, что объект с NetworkClient существует в сцене.");
        }
    }

    public void StartAsClient()
    {
        NetworkManager.IsHost = false; // Устанавливаем роль клиента напрямую

        if (NetworkClient.Instance != null)
        {
            NetworkClient.Instance.ConnectToServer();
            if (NetworkClient.Instance.IsConnected())
            {
                NetworkClient.Instance.Send("CONNECT");
                DestroyExistingBall();
                SceneManager.LoadScene("Pong Game");
            }
            else
            {
                Debug.LogError("Не удалось подключиться к серверу!");
            }
        }
        else
        {
            Debug.LogError("NetworkClient.Instance отсутствует! Убедитесь, что объект с NetworkClient существует в сцене.");
        }
    }

    private void DestroyExistingBall()
    {
        GameObject ball = GameObject.FindWithTag("Ball");
        if (ball != null)
        {
            Debug.Log("Уничтожаем старый мячик перед загрузкой новой сцены.");
            Destroy(ball);
        }
    }

    public void MoveToScene(int sceneID)
    {
        DestroyExistingBall();
        SceneManager.LoadScene(sceneID);
    }

    public void Quit()
    {
        if (NetworkClient.Instance != null && NetworkClient.Instance.IsConnected())
        {
            NetworkClient.Instance.Send("QUIT");
        }
        Application.Quit();
    }

    public void GoToSettings()
    {
        DestroyExistingBall();
        SceneManager.LoadScene("Settings");
    }
}

public class GameOverMenu : MonoBehaviour
{
    public void Replay()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetScores();
        }
        SceneManager.LoadScene("Pong Game");
    }

    public void MainMenu()
    {
        if (NetworkClient.Instance != null && NetworkClient.Instance.IsConnected())
        {
            NetworkClient.Instance.Send("MAINMENU");
        }
        SceneManager.LoadScene("MainMenu");
    }

    public void MoveToScene(string sceneName)
    {
        if (sceneName == "Pong Game" && NetworkClient.Instance != null)
        {
            NetworkClient.Instance.ResetScores();
        }
        SceneManager.LoadScene(sceneName);
    }
}