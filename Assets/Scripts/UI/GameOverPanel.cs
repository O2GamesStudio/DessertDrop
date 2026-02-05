using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    private const string HIGH_SCORE_KEY = "HighScore";

    [SerializeField] private Button retryBtn;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    private void Awake()
    {
        retryBtn.onClick.AddListener(OnRetryClicked);
    }

    public void Show(int currentScore)
    {
        gameObject.SetActive(true);

        int highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
        }

        scoreText.text = currentScore.ToString();
        highScoreText.text = highScore.ToString();
    }

    private void OnRetryClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        retryBtn.onClick.RemoveListener(OnRetryClicked);
    }
}