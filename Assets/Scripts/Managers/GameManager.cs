using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject[] fruitPrefabs;

    private int score;
    private int currentMaxLevel = 0;
    private int consecutiveNoMerge = 0;
    private bool isGameOver = false;
    private bool isPaused = false;

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
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayGameBGM();
        }
    }

    public GameObject GetFruitPrefab(FruitType type)
    {
        int index = (int)type;

        if (fruitPrefabs == null || index < 0 || index >= fruitPrefabs.Length)
        {
            Debug.LogError($"Fruit prefab not found for type: {type}");
            return null;
        }

        return fruitPrefabs[index];
    }

    public int GetFruitDataCount()
    {
        return fruitPrefabs != null ? fruitPrefabs.Length : 0;
    }

    public void AddScore(int points)
    {
        score += points;
        UIManager.Instance.UpdateScoreUI(score);
    }

    public int GetScore()
    {
        return score;
    }

    public void UpdateMaxLevel(int level)
    {
        if (level > currentMaxLevel)
        {
            currentMaxLevel = level;
        }
    }

    public int GetCurrentMaxLevel()
    {
        return currentMaxLevel;
    }

    public void IncrementNoMerge()
    {
        consecutiveNoMerge++;
    }

    public void ResetNoMerge()
    {
        consecutiveNoMerge = 0;
    }

    public int GetConsecutiveNoMerge()
    {
        return consecutiveNoMerge;
    }

    public int GetActiveFruitCount()
    {
        return FindObjectsByType<Fruit>(FindObjectsSortMode.None).Length;
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0f;
        UIManager.Instance.ShowGameOverUI(score);
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void PauseGame()
    {
        if (isGameOver) return;
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (isGameOver) return;
        isPaused = false;
        Time.timeScale = 1f;
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}