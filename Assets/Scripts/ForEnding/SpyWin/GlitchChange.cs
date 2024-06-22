using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GlitchChange : MonoBehaviour
{
    [SerializeField] private Volume _glitchVolume;
    [SerializeField] private VolumeProfile _glitchVolumeProfile1;
    [SerializeField] private VolumeProfile _glitchVolumeProfile2;
    [SerializeField] private VolumeProfile _glitchVolumeProfile3;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void InitGlitch()
    {
        _glitchVolume.profile = _glitchVolumeProfile1;
    }
    public void ChangeGlitch()
    {
        _glitchVolume.profile = _glitchVolumeProfile2;
    }
    public void EndGlitch()
    {
        _glitchVolume.profile = _glitchVolumeProfile3;
    }
}
