using UnityEngine;
using System.Collections.Generic;

public class DeadLine : MonoBehaviour
{
    [SerializeField] private float gameOverDelay = 2f;
    [SerializeField] private float physicsSettleDelay = 1f;
    [SerializeField] private float blinkInterval = 0.15f;
    [SerializeField] private Color warningColor = Color.red;

    private float deadlineY;
    private bool isGameOver = false;
    private Dictionary<Fruit, float> aboveTimers = new Dictionary<Fruit, float>();
    private Dictionary<Fruit, float> blinkTimers = new Dictionary<Fruit, float>();
    private Dictionary<Fruit, bool> blinkStates = new Dictionary<Fruit, bool>();
    private Dictionary<Fruit, float> settleTimers = new Dictionary<Fruit, float>();

    void Awake()
    {
        deadlineY = transform.position.y;
    }

    void Update()
    {
        if (isGameOver) return;

        var fruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);

        CleanupDictionaries(fruits);

        foreach (var fruit in fruits)
        {
            if (!fruit.IsPhysicsEnabled()) continue;

            if (!settleTimers.ContainsKey(fruit))
            {
                settleTimers[fruit] = 0f;
            }

            settleTimers[fruit] += Time.deltaTime;
            if (settleTimers[fruit] < physicsSettleDelay) continue;

            float bottomY = fruit.transform.position.y - fruit.GetRadius();
            bool isAbove = bottomY > deadlineY;

            if (isAbove)
            {
                if (!aboveTimers.ContainsKey(fruit))
                {
                    aboveTimers[fruit] = 0f;
                }

                aboveTimers[fruit] += Time.deltaTime;
                HandleBlink(fruit, true);

                if (aboveTimers[fruit] >= gameOverDelay)
                {
                    isGameOver = true;
                    ResetAllColors(fruits);
                    GameManager.Instance.TriggerGameOver();
                    return;
                }
            }
            else
            {
                if (aboveTimers.ContainsKey(fruit))
                {
                    aboveTimers.Remove(fruit);
                    HandleBlink(fruit, false);
                }
            }
        }
    }

    void HandleBlink(Fruit fruit, bool enable)
    {
        SpriteRenderer sr = fruit.GetSpriteRenderer();
        if (sr == null) return;

        if (!enable)
        {
            sr.color = Color.white;
            blinkTimers.Remove(fruit);
            blinkStates.Remove(fruit);
            return;
        }

        if (!blinkTimers.ContainsKey(fruit))
        {
            blinkTimers[fruit] = 0f;
            blinkStates[fruit] = true;
        }

        blinkTimers[fruit] += Time.deltaTime;
        if (blinkTimers[fruit] >= blinkInterval)
        {
            blinkTimers[fruit] = 0f;
            blinkStates[fruit] = !blinkStates[fruit];
            sr.color = blinkStates[fruit] ? warningColor : Color.white;
        }
    }

    void CleanupDictionaries(Fruit[] activeFruits)
    {
        var activeSet = new HashSet<Fruit>(activeFruits);

        var keys = new List<Fruit>(aboveTimers.Keys);
        foreach (var key in keys)
        {
            if (!activeSet.Contains(key))
            {
                aboveTimers.Remove(key);
                blinkTimers.Remove(key);
                blinkStates.Remove(key);
                settleTimers.Remove(key);
            }
        }

        keys = new List<Fruit>(settleTimers.Keys);
        foreach (var key in keys)
        {
            if (!activeSet.Contains(key))
            {
                settleTimers.Remove(key);
            }
        }
    }

    void ResetAllColors(Fruit[] fruits)
    {
        foreach (var fruit in fruits)
        {
            SpriteRenderer sr = fruit.GetSpriteRenderer();
            if (sr != null) sr.color = Color.white;
        }
    }

    public void Reset()
    {
        isGameOver = false;
        aboveTimers.Clear();
        blinkTimers.Clear();
        blinkStates.Clear();
        settleTimers.Clear();
    }
}