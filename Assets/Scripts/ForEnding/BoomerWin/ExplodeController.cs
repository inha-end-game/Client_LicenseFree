using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explode;
    [SerializeField] private GameObject _map;
    [SerializeField] private GameObject _building1;
    [SerializeField] private GameObject _building2;
    [SerializeField] private GameObject _building3;
    [SerializeField] private GameObject _destroy_building1;
    [SerializeField] private GameObject _destroy_building2;
    [SerializeField] private GameObject _destroy_building3;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void beep()
    {
        GetComponent<AudioSource>().Play();
    }
    public void Explode1()
    {
        Instantiate(_explode, new Vector3(108, 4, 29), Quaternion.Euler(new Vector3(-16, -58, 14)));
        Destroy(_building1);
        Instantiate(_destroy_building1, _map.transform);
    }
    public void Explode2()
    {
        Instantiate(_explode, new Vector3(105, 13.7f, 41), Quaternion.Euler(new Vector3(-57.5f, -134.4f, 43)));
        Destroy(_building2);
        Instantiate(_destroy_building2, _map.transform);
    }
    public void Explode3()
    {
        Instantiate(_explode, new Vector3(109.75f, 9.2f, 45.8f), Quaternion.Euler(new Vector3(-8.912f, -58.445f, -5.213f)));
        Destroy(_building3);
        Instantiate(_destroy_building3, _map.transform);
    }
    public void Explode4()
    {
        Instantiate(_explode, new Vector3(111.89f, 5.55f, 39.25f), Quaternion.Euler(new Vector3(-16, 0, 0)));
        Destroy(_building3);
        Instantiate(_destroy_building3, _map.transform);
    }
    public void Explode5()
    {
        Instantiate(_explode, new Vector3(102.6f, 7.48f, 45.29f), Quaternion.Euler(new Vector3(25.6f, 11.52f, -32.98f)));
        Destroy(_building3);
        Instantiate(_destroy_building3, _map.transform);
    }
}
