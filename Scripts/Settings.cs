using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using static UnityEngine.Mathf;
[CreateAssetMenu(fileName = "New Settings", menuName = "MySOs/Settings")]
public class Settings : ScriptableObject
{
    [SerializeField] private float masterVolume, musicVolume, sfxVolume;
    public float MasterVolume { 
        get { return masterVolume; } 
        set {
            if (RoundToInt(masterVolume * 10) != RoundToInt(value * 10))
                AudioManager.instance.PlaySound("Tick", Vector3.zero, 1f);
            masterVolume = Clamp01(value); 
        } 
    }
    public float MusicVolume { get { return musicVolume; } set { musicVolume = Clamp01(value); } }
    public float SFXVolume { get { return sfxVolume; } 
        set {
            if (RoundToInt(SFXVolume * 10) != RoundToInt(value * 10))
            {
                Debug.Log("Tick: " + value);
                AudioManager.instance.PlaySound("Tick", Vector3.zero, 1f);
            }
            sfxVolume = Clamp01(value); 
        } 
    }

    [SerializeField] private int aiDifficulty = 1;
    public int AIDifficulty { get { return aiDifficulty; } set { aiDifficulty = Clamp(value, 0, 2); } }
    [SerializeField] private float sensitivity;
    public float Sensitivity { get { return sensitivity; } set { sensitivity = Clamp01(value); } }

    public bool BallTrailEnabled, BounceMarkerEnabled, GoalExplosionEnabled;

    public void LoadSettings()
    {
        Debug.Log("Load");
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        AIDifficulty = PlayerPrefs.GetInt("AIDifficulty", 1);
        Sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1);

        BallTrailEnabled = PlayerPrefs.GetInt("Trail", 1) == 1;
        BounceMarkerEnabled = PlayerPrefs.GetInt("Marker", 1) == 1;
        GoalExplosionEnabled = PlayerPrefs.GetInt("Explosion", 1) == 1;
    }
    public void SaveSettings()
    {
        Debug.Log("Save");

        PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        PlayerPrefs.SetFloat("SFXVolume", SFXVolume);

        PlayerPrefs.SetInt("AIDifficulty", AIDifficulty);
        PlayerPrefs.SetFloat("Sensitivity", Sensitivity);

        PlayerPrefs.SetInt("Trail", BallTrailEnabled.ToInt());
        PlayerPrefs.SetInt("Marker", BounceMarkerEnabled.ToInt());
        PlayerPrefs.SetInt("Explosion", GoalExplosionEnabled.ToInt());
    }
    public void SetDefault()
    {
        MasterVolume = MusicVolume = SFXVolume = 0.5f;
        AIDifficulty = 1;
        Sensitivity = 1;
        BallTrailEnabled = BounceMarkerEnabled = GoalExplosionEnabled = true;
    }
}
