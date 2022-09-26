using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DefaultNamespace;
using DefaultNamespace.InfoClass;
using DefaultNamespace.utils;
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
    
    public bool recording = false;

    public bool withRandomField = false;

    public bool isDrag = true;

    public bool withExperiment = true;


    private Drag drag;
    private Rigidbody _rb;
    private Stopwatch watch = new Stopwatch();
    private float _massBurnRate;
    private bool inWindZone = false;
    public GameObject windZone;
    private List<TransformData> _transformData = new List<TransformData>();
    private int _count;
    private int _localCount;
    private string _path;
    private List<double> _TMSForce;
    private List<double> _grainMass;
    private int _timestamp = 0;
    
    private void Start()
    {                        
        _rb = gameObject.GetComponent<Rigidbody>();
        _massBurnRate = (initialMass - massNoFuel) / burnTime;
        _rb.mass = initialMass;
        watch.Start();
        _count = (int)(stopTime / recordingFreq);
        _path = Path.Combine(Application.dataPath + "/PositionData/", "data.json");
        LoadCSV TMS_csv = new LoadCSV("TMS");
        LoadCSV grainMass_csv = new LoadCSV("Grain");
        drag = new Drag();
        TMS_csv.Read("F(N)");
        grainMass_csv.Read("grainMass");

        _TMSForce = TMS_csv.data;
        _grainMass = grainMass_csv.data;

        if (_TMSForce.Count < _grainMass.Count)
        {
            _grainMass = LoadCSV.SyncTime(_TMSForce.Count, _grainMass);
        }
        
        else
        {
            _TMSForce = LoadCSV.SyncTime(_grainMass.Count, _TMSForce);
        }

    }
    
    
    private void FixedUpdate()
    {
        double currentForce;
        double currentMass;
        
        if (_count == _localCount)
        {
            string parsedStr = JsonUtility.ToJson(new Serialization<TransformData>(_transformData));
            Debug.Log(parsedStr);
            if(recording)
                File.WriteAllText(_path, parsedStr); 
            _localCount++;

        }
        if (withRandomField)
        {
            _rb.AddRelativeTorque(windZone.GetComponent<WindArea>().direction * windZone.GetComponent<WindArea>().strength);
        }

        if (watch.ElapsedMilliseconds > recordingFreq)
        {
            _transformData.Add(new TransformData(_rb.position, _rb.rotation.eulerAngles, _rb.velocity));
            _localCount++;
            
            watch.Restart();
        }

        if (withExperiment && _timestamp < _grainMass.Count)
        {
            currentForce = _TMSForce[_timestamp];
            currentMass = _grainMass[_timestamp];
            _timestamp++;
            
            drag.calDragLoop(_rb.velocity.y);
            UpdateMass(Time.fixedDeltaTime, currentMass);
            ApplyThrust(currentForce);
        }

        else
        {
            UpdateMass(Time.fixedDeltaTime, 0);
            UpdateThrust();
            drag.calDragLoop(_rb.velocity.y);
            ApplyThrust(0);
        }
        
        
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
    private void UpdateMass(float dt, double currentMass)
    {
        if (withExperiment)
        {
            _rb.mass = (float)currentMass * (float)0.01 + massNoFuel ;
        }
        else
        {
            if (_rb.mass <= massNoFuel) return;
        
            _rb.mass -= _massBurnRate * dt;
            _rb.mass = Mathf.Max(_rb.mass, massNoFuel);
        }
        
    }

    private void UpdateMassByExperimentalResult()
    {
        
    }

   
    /**
     * Apply force vector to object with thrust
     */
    
    private void ApplyThrust(double currentThrust)
    {
        Debug.Log(currentThrust);
        if (withExperiment)
        {
            if (isDrag)
            {
                _rb.AddForce(_rb.transform.up * ((float)currentThrust - (float)drag.dragForce));
            }
            
            else
            {
                _rb.AddForce(_rb.transform.up * (float)currentThrust );
            }
        }

        else
        {
            if (isDrag)
            {
                _rb.AddForce(_rb.transform.up * (thrust - (float)drag.dragForce));
            }
            else
            {
                _rb.AddForce(_rb.transform.up * thrust );
            }
        }
        
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("windArea"))
        {
            Debug.Log("Im in");
            windZone = other.gameObject;
            inWindZone = true;
        }
        Debug.Log("Im in");
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
