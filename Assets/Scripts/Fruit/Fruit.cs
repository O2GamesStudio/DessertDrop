using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class Fruit : MonoBehaviour
{
    [SerializeField] private FruitType fruitType;
    [SerializeField] private float radius;
    [SerializeField] private int score;
    [SerializeField] private float pushStrength = 3f;
    [SerializeField] private float explosionRadiusMultiplier = 2f;
    [SerializeField] private float explosionForce = 5f;
    [SerializeField] private float mass = 1f;

    private Rigidbody2D rb;
    private CircleCollider2D col;
    private SpriteRenderer sr;
    private bool canMerge = false;
    private bool isPhysicsEnabled = false;
    private bool isMerging = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        rb.mass = mass;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;
        rb.gravityScale = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public void SetFruitType(FruitType type)
    {
        fruitType = type;
    }

    public FruitType GetFruitType()
    {
        return fruitType;
    }

    public float GetRadius()
    {
        return radius;
    }

    public int GetScore()
    {
        return score;
    }

    public float GetPushStrength()
    {
        return pushStrength;
    }

    public float GetExplosionRadiusMultiplier()
    {
        return explosionRadiusMultiplier;
    }

    public float GetExplosionForce()
    {
        return explosionForce;
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return sr;
    }

    public void EnablePhysics()
    {
        col.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        canMerge = true;
        isPhysicsEnabled = true;
    }

    public void DisablePhysics()
    {
        col.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        canMerge = false;
        isPhysicsEnabled = false;
    }

    public bool IsPhysicsEnabled()
    {
        return isPhysicsEnabled;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canMerge || isMerging) return;

        Fruit otherFruit = collision.gameObject.GetComponent<Fruit>();
        if (otherFruit != null && otherFruit.canMerge && !otherFruit.isMerging && otherFruit.fruitType == fruitType)
        {
            int nextIndex = (int)fruitType + 1;
            if (nextIndex < GameManager.Instance.GetFruitDataCount())
            {
                Merge(otherFruit);
            }
        }
    }

    void Merge(Fruit other)
    {
        if (GetInstanceID() < other.GetInstanceID()) return;

        isMerging = true;
        other.isMerging = true;

        Vector3 mergePosition = (transform.position + other.transform.position) / 2f;
        FruitType nextType = (FruitType)((int)fruitType + 1);

        GameManager.Instance.AddScore(score);
        GameManager.Instance.UpdateMaxLevel((int)nextType);
        GameManager.Instance.ResetNoMerge();

        ApplyPushForce(mergePosition, nextType, other);

        FruitSpawner.Instance.SpawnMergedFruit(nextType, mergePosition);

        Destroy(other.gameObject);
        Destroy(gameObject);
    }

    void ApplyPushForce(Vector3 explosionPosition, FruitType nextType, Fruit other)
    {
        GameObject nextPrefab = GameManager.Instance.GetFruitPrefab(nextType);
        if (nextPrefab == null) return;

        Fruit nextFruit = nextPrefab.GetComponent<Fruit>();
        float newRadius = nextFruit.GetRadius();
        float explosionRadius = newRadius * nextFruit.GetExplosionRadiusMultiplier();
        float nextExplosionForce = nextFruit.GetExplosionForce();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(explosionPosition, explosionRadius);

        foreach (Collider2D col in colliders)
        {
            if (col.gameObject == gameObject || col.gameObject == other.gameObject) continue;

            Rigidbody2D targetRb = col.GetComponent<Rigidbody2D>();
            if (targetRb == null) continue;

            float distance = Vector2.Distance(col.transform.position, explosionPosition);
            float explosionRatio = 1f - (distance / explosionRadius);

            Vector2 pushDirection = ((Vector2)col.transform.position - (Vector2)explosionPosition).normalized;
            pushDirection.x *= 3.0f;
            pushDirection.y *= 0.2f;
            pushDirection = pushDirection.normalized;

            targetRb.AddForce(pushDirection * nextExplosionForce * explosionRatio, ForceMode2D.Impulse);
        }
    }
}