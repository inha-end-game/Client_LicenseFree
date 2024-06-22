using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheerSoundController : MonoBehaviour
{
    public void Cheer()
    {
        GetComponent<AudioSource>().Play();
    }
}
