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

    private Rigidbody _rb;
    private float _massBurnRate;
    public bool inWindZone = false;
    public GameObject windZone;


    private void Start()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
        _massBurnRate = (initialMass - massNoFuel) / burnTime;
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
}
