using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DeadLine : MonoBehaviour
{
    [SerializeField] private float checkDelay = 3f;
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

    void OnTriggerStay2D(Collider2D collision)
    {
        if (isGameOver) return;

        Fruit fruit = collision.GetComponent<Fruit>();
        if (fruit != null && fruit.CanCheckGameOver())
        {
            timer += Time.deltaTime;

            if (timer >= checkDelay)
            {
                GameOver();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Fruit fruit = collision.GetComponent<Fruit>();
        if (fruit != null)
        {
            timer = 0f;
        }
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over! Final Score: " + GameManager.Instance.GetScore());
        Time.timeScale = 0f;
    }
}