using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditSlider : MonoBehaviour
{
    public int speed = 10;
    private Vector3 startPos;

    private void Awake()
    {
        this.startPos = this.transform.position;
    }

    private void OnEnable()
    {
        this.transform.position = this.startPos;
    }

    void Update()
    {
        this.transform.Translate(new Vector2(0, speed * Time.deltaTime));
    }
}
