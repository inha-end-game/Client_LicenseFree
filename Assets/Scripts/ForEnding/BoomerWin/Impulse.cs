using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Impulse : MonoBehaviour
{
    public CinemachineImpulseSource _impulse;
    // Start is called before the first frame update
    void Start()
    {
        _impulse = transform.GetComponent<CinemachineImpulseSource>();
        _impulse.m_DefaultVelocity = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));
        _impulse.GenerateImpulse();
    }

    // Update is called once per frame
    void Update()
    {

    }

}
