using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnProbabilityConfig
{
    [Serializable]
    public class ProbabilitySet
    {
        public int maxLevelThreshold;
        public float[] probabilities;
    }

    public List<ProbabilitySet> probabilitySets;
    public float emergencyBoost = 0.15f;
    public int emergencyFruitCount = 20;
    public float noMergeBoost = 0.20f;
    public int noMergeThreshold = 5;
}