using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class WindArea : MonoBehaviour
{
    public float strength;
    public Vector3 direction;
    [SerializeField] private float timeFrequency;
    private Stopwatch watch;
    private void Start()
    {
        watch = new Stopwatch();
        direction = new Vector3(UnityEngine.Random.Range(0f, 1f), 
            0,
            UnityEngine.Random.Range(0f, 1f));
        watch.Start();
        

    }

    private void Update()
    {
        if (watch.ElapsedMilliseconds > timeFrequency * 100)
        {
            createRandomVector();
            watch.Restart();
        }
    }

    private void createRandomVector()
    {
        direction = new Vector3(UnityEngine.Random.Range(0f, 1f), 
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f));
        
        direction = Vector3.Normalize(direction);
    }
}