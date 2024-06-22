using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaughController : MonoBehaviour
{
    public void PlayLaugh()
    {
        GetComponent<AudioSource>().Play();
    }
}
