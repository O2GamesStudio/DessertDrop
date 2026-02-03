using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DeadLine : MonoBehaviour
{
    [SerializeField] private float checkDelay = 2f;
    [SerializeField] private Vector2 colliderSize = new Vector2(4f, 0.1f);

    private float timer = 0f;
    private bool isGameOver = false;
    private BoxCollider2D col;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = colliderSize;
    }

    void Update()
    {
        if (isGameOver) return;

        if (IsAllFruitsAboveDeadline())
        {
            timer += Time.deltaTime;
            if (timer >= checkDelay)
            {
                GameOver();
            }
        }
        else
        {
            timer = 0f;
        }
    }

    bool IsAllFruitsAboveDeadline()
    {
        var fruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);

        bool hasActiveFruit = false;
        foreach (var fruit in fruits)
        {
            if (!fruit.IsPhysicsEnabled()) continue;
            hasActiveFruit = true;

            float bottomY = fruit.transform.position.y - fruit.GetRadius();
            if (bottomY <= transform.position.y) return false;
        }

        return hasActiveFruit;
    }

    void GameOver()
    {
        isGameOver = true;
        GameManager.Instance.TriggerGameOver();
    }

    public void Reset()
    {
        isGameOver = false;
        timer = 0f;
    }
}