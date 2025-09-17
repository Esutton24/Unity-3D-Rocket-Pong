using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioMenu : UIMenu
{
    [SerializeField] Settings settings;
    Slider masterS, sfxS, musicS;
    private void Awake()
    {
        masterS = transform.GetChild(1).GetComponent<Slider>();
        musicS = transform.GetChild(2).GetComponent<Slider>();
        sfxS = transform.GetChild(3).GetComponent<Slider>();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        settings.LoadSettings();
        SetSliders();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        settings.SaveSettings();
    }
    private void SetSliders()
    {
        masterS.value = settings.MasterVolume;
        musicS.value = settings.MusicVolume;
        sfxS.value = settings.SFXVolume;
    }
}
