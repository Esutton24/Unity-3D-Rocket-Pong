using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameSettings : MonoBehaviour
{
    ParticleSystem[] ballEffects;

    public void SetBallTrail (bool isActive)
    {
        
    }

    public void SetBounceMarker (bool isActive)
    {

    }

    public void SetGoalExplosions(bool isActive)
    {
        ballEffects = FindObjectsOfType<ParticleSystem>();

        if (isActive == false)
        {
            foreach (ParticleSystem effect in ballEffects)
            {
                effect.Stop();
            }
        }
    }
}
