using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int player1Score = 0;
    public int player2Score = 0;
    
    public Text player1ScoreText;
    public Text player2ScoreText;

    public int maxScore = 11;

    void Update()
    {
        if (player1Score >= maxScore || player2Score >= maxScore)
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    public void AddScore(int player)
    {
        if (player == 1)
        {
            player1Score++;
            
        }

        else
        { 
            player2Score++;
            
        }
            
    }

    public void ResetScores()
    {
        player1Score = 0;
        player2Score = 0;
    }
}
