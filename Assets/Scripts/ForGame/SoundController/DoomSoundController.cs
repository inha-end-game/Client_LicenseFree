using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoomSoundController : MonoBehaviour
{
    public void Doom()
    {
        GetComponent<AudioSource>().Play();
    }
}
