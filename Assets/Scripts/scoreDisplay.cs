using UnityEngine;
using TMPro; 

public class ScoreDisplay : MonoBehaviour
{
    public GenerateMaze mazeGenerator;
    public TextMeshProUGUI scoreText; 

    void Update()
    {
        if (mazeGenerator != null && scoreText != null)
        {
            scoreText.text = "Player: " + mazeGenerator.playerScore + "\nAI: " + mazeGenerator.aiScore;
        }
    }
}