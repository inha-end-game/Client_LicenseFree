using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopSoundController : MonoBehaviour
{
    //Sounds
    [SerializeField] private AudioClip _footStepWalk;
    [SerializeField] private AudioClip _footStepRun;
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {

    }
    void FootStep_Walk()
    {
        AudioSource.PlayClipAtPoint(_footStepWalk, transform.position);
    }
    void FootStep_Run()
    {
        AudioSource.PlayClipAtPoint(_footStepRun, transform.position);
    }
}
