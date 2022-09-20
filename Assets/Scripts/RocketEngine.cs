using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(Rigidbody))]
public class RocketEngine : MonoBehaviour
{
    [SerializeField] private float thrust;
    
    [SerializeField] private float massNoFuel;
    
    [SerializeField] private float initialMass;
    
    [SerializeField] private float burnTime;

    [SerializeField] private int recordingFreq;

    [SerializeField] private int stopTime;

    

    private Rigidbody _rb;
    private Stopwatch watch = new Stopwatch();
    private float _massBurnRate;
    public bool inWindZone = false;
    public GameObject windZone;
    private List<TransformData> _transformData = new List<TransformData>();
    private int _count;
    private int _localCount;
    private string _path;
    
    private void Start()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
        _massBurnRate = (initialMass - massNoFuel) / burnTime;
        _rb.mass = initialMass;
        watch.Start();
        _count = (int)(stopTime / recordingFreq);
        _path = Path.Combine(Application.dataPath + "/PositionData/", "data.json");
    }
    
    
    private void FixedUpdate()
    {
        if (_count == _localCount)
        {
            string parsedStr = JsonUtility.ToJson(new Serialization<TransformData>(_transformData));
            Debug.Log(parsedStr);
            File.WriteAllText(_path, parsedStr);
            _localCount++;

        }
        if (inWindZone)
        {
            _rb.AddRelativeTorque(windZone.GetComponent<WindArea>().direction * windZone.GetComponent<WindArea>().strength);
        }

        if (watch.ElapsedMilliseconds > recordingFreq)
        {
            _transformData.Add(new TransformData(_rb.position, _rb.rotation.eulerAngles, _rb.velocity));
            _localCount++;
            watch.Restart();
        }

        UpdateMass(Time.fixedDeltaTime);
        UpdateThrust();
        ApplyThrust();
    }
    
    /**
     * TODO: Add Calculation with experimental result
     */
    private void UpdateThrust()
    {
        if (_rb.mass <= massNoFuel) thrust = 0f;
    }
    
    /**
     * Decrease mass linearly
     * TODO: Add Calculation with experimental result 
     * @param0: deltaTime (default: 0.02)
     */
    private void UpdateMass(float dt)
    {
        if (_rb.mass <= massNoFuel) return;
        
        _rb.mass -= _massBurnRate * dt;
        _rb.mass = Mathf.Max(_rb.mass, massNoFuel);
    }

   
    /**
     * Apply force vector to object with thrust
     */
    
    private void ApplyThrust()
    {
        _rb.AddRelativeForce(_rb.transform.up * thrust);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("windArea"))
        {
            windZone = other.gameObject;
            inWindZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("windArea"))
        {
            windZone = other.gameObject;
            inWindZone = false;
        }
    }

    [Serializable]
    public class Serialization<T>
    {
        [SerializeField] private List<T> target;

        public List<T> ToList()
        {
            return target;
        }
        
        public Serialization(List<T> target)
        {
            this.target = target;
        }
    }
}
