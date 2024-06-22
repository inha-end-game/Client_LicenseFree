using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepSoundController : MonoBehaviour
{
    public void FootStep()
    {
        GetComponent<AudioSource>().Play();
    }
}
