using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSoundController : MonoBehaviour
{
    //Sounds
    [SerializeField] private AudioClip _gunShotSound;
    [SerializeField] private AudioClip _gunReload;
    [SerializeField] private GameObject _shotEffect;
    private ParticleSystem _shotParticle;

    // Start is called before the first frame update
    void Start()
    {
        _shotParticle = _shotEffect.GetComponent<ParticleSystem>();
    }
    // Update is called once per frame
    void Update()
    {

    }
    private void Shot()
    {
        AudioSource.PlayClipAtPoint(_gunShotSound, transform.position);

        if (_shotParticle.isPlaying)
        {
            _shotParticle.Stop();
        }

        _shotParticle.Play();
    }
        void Reload()
    {
        AudioSource.PlayClipAtPoint(_gunReload, transform.position);
    }
}
