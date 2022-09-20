using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    
    [SerializeField] private GameObject nozzle;

    private Rigidbody _rb;

    private Vector3 _origin;
    private float _massBurnRate;
    public float lifeTime = 10f;
    public bool inWindZone = false;
    public GameObject windZone;

    private void Awake()
    {
        

    }

    private void Start()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
        _massBurnRate = (initialMass - massNoFuel) / burnTime;
        _origin = _rb.position;
        _rb.mass = initialMass;
    }
    
    
    private void FixedUpdate()
    {
        if (inWindZone)
        {
            _rb.AddRelativeTorque(windZone.GetComponent<WindArea>().direction * windZone.GetComponent<WindArea>().strength);
        }


        UpdateMass(Time.fixedDeltaTime);
        UpdateThrust();
        ApplyThrust();
    }
    
    private void UpdateThrust()
    {
        if (_rb.mass <= massNoFuel) thrust = 0f;
    }
    
    private void UpdateMass(float dt)
    {
        if (_rb.mass <= massNoFuel) return;
        
        _rb.mass -= _massBurnRate * dt;
        _rb.mass = Mathf.Max(_rb.mass, massNoFuel);
    }

    private void _keyboardControl()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 v3 = new Vector3(5, 0, 0);
            
            _rb.AddRelativeTorque(thrust * 0.1f * v3);
        }
        
        if (Input.GetKey(KeyCode.S))
        {
            Vector3 v3= new Vector3(-5, 0, 0);
           
            _rb.AddRelativeTorque(thrust * 0.1f * v3);
            
        }
        
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 v3= new Vector3(0, 0, -5);
           
            _rb.AddRelativeTorque(thrust * 0.1f * v3);
        }
        if (Input.GetKey(KeyCode.D))
        {
            Vector3 v3= new Vector3(0, 0, 5);
            
            _rb.AddRelativeTorque(thrust * 0.1f * v3);
            
        }
    }
    
    private void ApplyThrust()
    {
        _rb.AddRelativeForce(_rb.transform.up* thrust);
        
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
}
