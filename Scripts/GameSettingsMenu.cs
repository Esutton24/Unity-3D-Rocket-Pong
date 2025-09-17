using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsMenu : UIMenu
{
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Toggle Trail, Marker, Explosion;
    [SerializeField] private Settings gameSettings;
    [SerializeField] private TMPro.TMP_Text sensText;
    protected override void OnEnable()
    {
        base.OnEnable();
        sensitivitySlider.value = gameSettings.Sensitivity;
        Trail.isOn = gameSettings.BallTrailEnabled;
        Marker.isOn = gameSettings.BounceMarkerEnabled;
        Explosion.isOn = gameSettings.GoalExplosionEnabled;
        sensitivitySlider.onValueChanged.AddListener(SetSensText);
        SetSensText(sensitivitySlider.value);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        sensitivitySlider.onValueChanged.RemoveListener(SetSensText);
    }
    void SetSensText(float val) => sensText.SetText( Mathf.Round(val * 100) + "");
}
