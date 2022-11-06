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

    public bool withRollRotation = false;

    public float k;
    
    public GameObject top;
    
    public GameObject cp;
    
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

        if (withRollRotation)
        {
            _rb.AddRelativeTorque(new Vector3(0, (float)0.1, 0) * _timestamp);
        }
        
        
        if (_count == _localCount)
        {
            string parsedStr = JsonUtility.ToJson(new Serialization<TransformData>(_transformData));
            if(recording)
                File.WriteAllText(_path, parsedStr); 
            _localCount++;

        }
        if (withRandomField)
        {
            _rb.AddRelativeTorque(windZone.GetComponent<WindArea>().direction * windZone.GetComponent<WindArea>().strength);
            Vector3 _recoveryTorque = calcRecoveryForce();

            Vector3 temp = (float)0.95 * top.transform.position -  cp.transform.position;
            Debug.Log("top: " + top.transform.position.ToString());
            Debug.Log("cp: " + cp.transform.position.ToString());
            temp = Vector3.Normalize(temp);
            Debug.DrawLine(temp, temp, Color.cyan);
            Debug.Log("tip: " + temp.ToString());
            
            
            
            
            Vector3 ttemp = new Vector3(30 * temp.x * _recoveryTorque.x, 
                0,
                 temp.z * _recoveryTorque.z);

            Vector3 r = _rb.centerOfMass - cp.transform.position;
            Vector3 torque = Vector3.Cross(r, ttemp);
            Debug.Log("Temp: " + ttemp.ToString());
            _rb.AddTorque(torque);
            
            
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

    private Vector3 calcRecoveryForce()
    {
        Vector3 _rot = _rb.rotation.eulerAngles;
       
        double xForce = Math.Cos(_rot.x * Mathf.Deg2Rad);
        double zForce = Math.Sin(_rot.z * Mathf.Deg2Rad);

        double xSpring = Math.Tan(_rot.x * Mathf.Deg2Rad);
        double zSpring = Math.Tan(_rot.z * Mathf.Deg2Rad);

        
        
        xForce =  -(k * Rocket.RocketLength * xSpring) / xForce;
        
        
        zForce =  -(k * Rocket.RocketLength * zSpring) / zForce;

        Vector3 ret = new Vector3((float)xForce, 0, (float)zForce);

        Debug.Log("restore:" + ret.ToString());
        
        return ret;
        
    }

   
    /**
     * Apply force vector to object with thrust
     */
    
    private void ApplyThrust(double currentThrust)
    {
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
