using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image nextObjectImage;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private float countUpSpeed = 100f;

    private int currentDisplayScore = 0;
    private int targetScore = 0;
    private Coroutine countUpCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreText(0);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void UpdateScoreUI(int newScore)
    {
        targetScore = newScore;

        if (countUpCoroutine == null)
        {
            countUpCoroutine = StartCoroutine(CountUpScore());
        }
    }

    public void UpdateNextFruitUI(FruitType nextFruitType)
    {
        if (nextObjectImage != null)
        {
            GameObject prefab = GameManager.Instance.GetFruitPrefab(nextFruitType);
            if (prefab != null)
            {
                SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    nextObjectImage.sprite = sr.sprite;
                    nextObjectImage.color = Color.white;
                }
            }
        }
    }

    public void ShowGameOverUI(int finalScore)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverScoreText != null) gameOverScoreText.text = finalScore.ToString();
    }

    IEnumerator CountUpScore()
    {
        while (currentDisplayScore < targetScore)
        {
            int difference = targetScore - currentDisplayScore;
            int increment = Mathf.Max(1, Mathf.CeilToInt(difference * Time.deltaTime * countUpSpeed / 10f));

            currentDisplayScore = Mathf.Min(currentDisplayScore + increment, targetScore);
            UpdateScoreText(currentDisplayScore);

            yield return null;
        }

        countUpCoroutine = null;
    }

    void UpdateScoreText(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }
}