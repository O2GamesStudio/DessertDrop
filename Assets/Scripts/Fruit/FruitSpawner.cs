using UnityEngine;
using UnityEngine.InputSystem;

public class FruitSpawner : MonoBehaviour
{
    public static FruitSpawner Instance { get; private set; }

    [SerializeField] private GameObject fruitPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnHeight = 4f;
    [SerializeField] private float containerHalfWidth = 2f;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 10;
    [SerializeField] private float trajectoryTimeStep = 0.15f;
    [SerializeField] private TextAsset probabilityConfigJson;

    private Fruit currentFruit;
    private FruitType nextFruitType;
    private Camera mainCamera;
    private bool isDragging = false;
    private float currentFruitRadius = 0f;
    private SpawnProbabilityConfig probabilityConfig;

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

        mainCamera = Camera.main;

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }

        LoadProbabilityConfig();
    }

    void LoadProbabilityConfig()
    {
        if (probabilityConfigJson != null)
        {
            probabilityConfig = JsonUtility.FromJson<SpawnProbabilityConfig>(probabilityConfigJson.text);
        }
        else
        {
            Debug.LogError("Probability config JSON not assigned!");
        }
    }

    void Start()
    {
        nextFruitType = GetRandomFruitType();
        SpawnNextFruit();
    }

    void Update()
    {
        if (currentFruit == null) return;

        Vector2 touchPosition = GetInputPosition();
        if (touchPosition != Vector2.zero)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 10f));
            float halfSize = currentFruitRadius;
            float clampedX = Mathf.Clamp(worldPos.x, -containerHalfWidth + halfSize, containerHalfWidth - halfSize);
            currentFruit.transform.position = new Vector3(clampedX, spawnHeight, 0f);

            if (!isDragging)
            {
                isDragging = true;
                if (trajectoryLine != null)
                {
                    trajectoryLine.enabled = true;
                }
            }

            UpdateTrajectory(currentFruit.transform.position);
        }

        if (isDragging && !IsInputActive())
        {
            isDragging = false;
            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = false;
            }
            DropFruit();
        }
    }

    void UpdateTrajectory(Vector3 startPosition)
    {
        if (trajectoryLine == null) return;

        trajectoryLine.positionCount = trajectoryPoints;
        Vector3 velocity = Vector3.zero;
        Vector3 gravity = Physics2D.gravity;
        Vector3 position = startPosition;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            trajectoryLine.SetPosition(i, position);
            velocity += gravity * trajectoryTimeStep;
            position += velocity * trajectoryTimeStep;

            if (position.y < -3f)
            {
                trajectoryLine.positionCount = i + 1;
                break;
            }
        }
    }

    Vector2 GetInputPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            return Mouse.current.position.ReadValue();
        }

        return Vector2.zero;
    }

    bool IsInputActive()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            return true;
        }

        return false;
    }

    void SpawnNextFruit()
    {
        GameObject fruitObj = Instantiate(fruitPrefab, spawnPoint.position, Quaternion.identity);
        currentFruit = fruitObj.GetComponent<Fruit>();
        currentFruit.Initialize(nextFruitType);
        currentFruit.DisablePhysics();

        FruitData data = GameManager.Instance.GetFruitData(nextFruitType);
        currentFruitRadius = data.radius;

        nextFruitType = GetRandomFruitType();
    }

    void DropFruit()
    {
        currentFruit.EnablePhysics();
        currentFruit = null;
        currentFruitRadius = 0f;
        GameManager.Instance.IncrementNoMerge();
        Invoke(nameof(SpawnNextFruit), 1f);
    }

    public void SpawnMergedFruit(FruitType type, Vector3 position)
    {
        GameObject fruitObj = Instantiate(fruitPrefab, position, Quaternion.identity);
        Fruit fruit = fruitObj.GetComponent<Fruit>();
        fruit.Initialize(type);
        fruit.EnablePhysics();
    }

    FruitType GetRandomFruitType()
    {
        int maxLevel = GameManager.Instance.GetCurrentMaxLevel();
        int activeFruits = GameManager.Instance.GetActiveFruitCount();
        int noMergeCount = GameManager.Instance.GetConsecutiveNoMerge();

        int spawnMax = Mathf.Min(maxLevel > 0 ? maxLevel - 1 : 0, 4);
        spawnMax = Mathf.Max(spawnMax, 0);

        float[] probabilities = GetProbabilities(maxLevel, activeFruits, noMergeCount);

        float randomValue = Random.Range(0f, 1f);
        float cumulative = 0f;

        for (int i = 0; i <= spawnMax; i++)
        {
            cumulative += probabilities[i];
            if (randomValue <= cumulative)
            {
                return (FruitType)i;
            }
        }

        return (FruitType)0;
    }

    float[] GetProbabilities(int maxLevel, int activeFruits, int noMergeCount)
    {
        if (probabilityConfig == null) return new float[] { 1f, 0f, 0f, 0f, 0f };

        float[] probs = new float[5];

        foreach (var set in probabilityConfig.probabilitySets)
        {
            if (maxLevel <= set.maxLevelThreshold)
            {
                for (int i = 0; i < Mathf.Min(probs.Length, set.probabilities.Length); i++)
                {
                    probs[i] = set.probabilities[i];
                }
                break;
            }
        }

        if (activeFruits >= probabilityConfig.emergencyFruitCount)
        {
            float boost = probabilityConfig.emergencyBoost;
            probs[0] += boost * 0.7f;
            probs[1] += boost * 0.3f;
            for (int i = 2; i < probs.Length; i++)
            {
                probs[i] *= 0.85f;
            }
        }

        if (noMergeCount >= probabilityConfig.noMergeThreshold)
        {
            probs[0] += probabilityConfig.noMergeBoost;
            for (int i = 1; i < probs.Length; i++)
            {
                probs[i] *= 0.8f;
            }
        }

        float sum = 0f;
        for (int i = 0; i < probs.Length; i++)
        {
            sum += probs[i];
        }
        for (int i = 0; i < probs.Length; i++)
        {
            probs[i] /= sum;
        }

        return probs;
    }
}