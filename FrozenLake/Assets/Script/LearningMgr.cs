using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearningMgr
{
    // UI
    public bool bIsWindyFrozen = true;

    // Learning parameters
    public int tryTotal = 1;            // The value of attempts.

    public float exploration = 0.5f;
    public float randomNoise = 0.1f;

    public float timeLeft = 0.05f;
    public float nextTime = 0.0f;

    public float discount = 0.9f;

    public float alpha = 0.1f;
    public float windyFrozen = 0.1f;
}
