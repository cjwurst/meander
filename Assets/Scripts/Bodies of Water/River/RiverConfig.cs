using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/River Config")]
public class RiverConfig : ScriptableObject
{
    public bool split = false;

    public float minSampleInterval = 0.5f;
    public float maxSampleInterval = 1.1f;

    public float cutThreshold = 0.5f;

    public float tangentCoeff = 5f;
    public float binormalCoeff = -0.2f;
    public float normalCoeff = 0.1f;
    public float totalCoeff = 0.001f;
    public float momentumBase = 0.5f;
    public int momentumDepth = 5;

    public bool debug = false;
    public bool debugDraw = false;
}
