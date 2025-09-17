using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRocket : MonoBehaviour
{
    [SerializeField] ParticleSystem rocketFireParticles, rocketSmokeParticles;
    [SerializeField] float fireEmission;
    public AudioClip rocketSound;
    SoundProfile rocketSFX;
  
    //public void ToggleRocketParticles(bool on) => rocketFireParticles.gameObject.SetActive(on);
    public void ToggleRocketParticles(bool on)
    {
        var emission = rocketFireParticles.emission;
        if (on)
        {
            emission.rateOverTime = fireEmission;
            if(!AudioManager.instance.HasSoundPlaying(rocketSFX))
                AudioManager.instance.PlaySound(rocketSFX, transform);
        }
        else
        {
            emission.rateOverTime = 0;
            AudioManager.instance.StopSound(rocketSFX);
        }
    }
    public void clearParticles()
    {
        rocketFireParticles.Clear();
    }
    private void Update()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }
    private void Awake()
    {
        rocketSFX = new SoundProfile(transform.parent.gameObject.name + " - " + gameObject.name);
        rocketSFX.Volume = 1;
        rocketSFX.Audio = rocketSound;
    }
}
