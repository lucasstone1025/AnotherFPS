using UnityEngine;
using TMPro;

public class PointsTracker : MonoBehaviour
{
    public static PointsTracker instance;

    public TMP_Text scoreText;
    public TMP_Text highscoreText;

    int score = 0;
    int highscore = 0;
    int numKillsInRow = 0;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreText.text = "POINTS: " + score.ToString();
        highscoreText.text = "HIGHSCORE: " + highscore.ToString();
    }

    public void AddKillPoints()
    {
        numKillsInRow++;
        score += (100 * numKillsInRow);
        scoreText.text = "POINTS: " + score.ToString();
    }

    public void AddWavePoints()
    {
        score += 500;
        scoreText.text = "POINTS: " + score.ToString();
    }

    public void MissedEnemy()
    {
        numKillsInRow = 0;
    }
}
