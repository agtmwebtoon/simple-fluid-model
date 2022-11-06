using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class TorqueTest : MonoBehaviour
{
    private Rigidbody _rb;
    
    // Start is called before the first frame update
    void Start()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 force = new Vector3(1, 1, 1);
        Vector3 torque = Vector3.Cross(Vector3.up, force * 3);
        _rb.AddTorque(torque);
    }
}
