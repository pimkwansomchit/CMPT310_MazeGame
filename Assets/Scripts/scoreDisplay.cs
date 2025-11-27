using UnityEngine;
using TMPro; // Use this if using TextMeshPro
// using UnityEngine.UI; // Use this if using legacy Text

public class ScoreDisplay : MonoBehaviour
{
    public GenerateMaze mazeGenerator;
    public TextMeshProUGUI scoreText; // Or: public Text scoreText; for legacy

    void Update()
    {
        if (mazeGenerator != null && scoreText != null)
        {
            scoreText.text = "Player: " + mazeGenerator.playerScore + "\nAI: " + mazeGenerator.aiScore;
        }
    }
}