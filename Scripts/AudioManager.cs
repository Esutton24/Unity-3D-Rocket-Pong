using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Read Me//
    /*
    This script is used to manage every sound that happens in the game

    Object Pooling:
    Creating a pool of objects that can be used many times instead of 
    instantiating the same object over and over again.
    This can help the game save resources and reduces lag.

    Not using object pooling is the eqivalent of buying a plate every time you want to eat
    and then smashing it when you're done using it.
    Using object pooling is like going to the store and buying a set of plates and then
    washing them when you're done using them.

    *TLDR* Creating and destroying game objects makes lag.
    Create all that you will need at once and use them when you need them and reuse them.
    */
    #region SelfExplanitoryVariables
    //Instance for other scripts to find this one at
    public static AudioManager instance;
    public bool LoadedSoundSettings = false;

    [SerializeField] private AudioSource _musicSource; //Source that music comes from
    [SerializeField] private AudioSource SFXSourcePrefab; //Prefab for sound effects
    [SerializeField] AudioClip UIButtonPressSound, UIButtonHoverSound, UIPageSwapSound;//SFX for UI
    #endregion
    #region VolumeControlVariables
    //Volume types
    public float MasterVolume => GameSettings? GameSettings.MasterVolume : 0; public float MusicVolume => GameSettings ? GameSettings.MusicVolume : 0; public float SFXVolume => GameSettings ? GameSettings.SFXVolume : 0;
    public SoundProfile CurrentMusicProfile;
    public Settings GameSettings;
    #endregion
    #region SourceRecyclingVariables
    [SerializeField] int NumOfSFXSources = 10;
    //Sources that are currently being used
    [SerializeField] private List<AudioSource> UsedSources = new List<AudioSource>();
    //Sources that are not being Used
    [SerializeField] private List<AudioSource> UnusedSources = new List<AudioSource>();
    //List containing sources that still need to be recycled
    [SerializeField] List<AudioSource> soundsToRecycle = new List<AudioSource>();
    //List containing profiles of the sources that need to be recycled
    [SerializeField] List<SoundProfile> profilesToClean = new List<SoundProfile>();
    //A list of all currently registered sound profiles
    [SerializeField] List<SoundProfile> ProfilesThatExist = new List<SoundProfile>();
    //Put in string for profile, output saved profile for that sound effect
    Dictionary<string, SoundProfile> SoundsDict = new Dictionary<string, SoundProfile>();
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
        SpawnAudioSources();
    }
    void SpawnAudioSources()
    {
        for (int i = 0; i < NumOfSFXSources; i++)
        {
            AudioSource sfx = Instantiate(SFXSourcePrefab, transform);
            sfx.gameObject.name = i.ToString();
            UnusedSources.Add(sfx);
        }
        _musicSource.spatialBlend = 0;
        _musicSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }
    private void Update()
    {
        foreach (SoundProfile profile in SoundsDict.Values)
        {
            foreach (AudioSource sfx in profile.currentUsedSourcesForSounds)
            {
                //If sfx is null, Recycle that source.
                //This usually happens when the gameobject holding the Source gets destroyed before the sound is finished playing
                if (sfx == null)
                {
                    //print("Missing AudioSource.. Making new one");
                    soundsToRecycle.Add(sfx);
                    profilesToClean.Add(profile);
                }
                else
                {
                    sfx.volume = SFXVolume * profile.Volume;
                    //if the sfx is done playing, recycle it
                    if (!sfx.isPlaying)
                    {
                        //print(sfx.name + "is finished.. Recycling");
                        soundsToRecycle.Add(sfx);
                        profilesToClean.Add(profile);
                    }
                }
            }

        }
        //Recycling all sounds that arent currently being used
        foreach (AudioSource sfx in soundsToRecycle)
        {
            if (sfx == null)
                UsedSources.Remove(sfx);
            RecycleAudioSource(sfx);
        }
        //All sounds that need to be recycled are recycled so clear out lists.
        soundsToRecycle.Clear();
        profilesToClean.Clear();
        CheckSourceAmount();

        _musicSource.volume = MusicVolume * (CurrentMusicProfile != null? CurrentMusicProfile.Volume : 1);
        AudioListener.volume = GameSettings.MasterVolume;
    }
    void CheckSourceAmount()
    {
        while (UsedSources.Count + UnusedSources.Count < NumOfSFXSources)
        {
            AudioSource sfx = Instantiate(SFXSourcePrefab, transform);
            sfx.gameObject.name = "ExtraSFX" + UnusedSources.Count;
            UnusedSources.Add(sfx);
        }
    }
    public void RegisterSound(SoundProfile newSound)
    {

        if (SoundsDict.ContainsKey(newSound.SoundName)) return;
        SoundsDict.Add(newSound.SoundName, newSound);
        ProfilesThatExist.Add(newSound);
    }
    public bool HasSoundPlaying(SoundProfile newSound)
    {
        if(newSound == CurrentMusicProfile) return true;
        if (!SoundsDict.ContainsKey(newSound.SoundName)) return false;
        return SoundsDict[newSound.SoundName].currentUsedSourcesForSounds.Count != 0;
    }
    public void StopSound(SoundProfile Sound)
    {
        if (!HasSoundPlaying(Sound)) return;
        if (SoundsDict.TryGetValue(Sound.SoundName, out SoundProfile newSound))
        {
            foreach (AudioSource sfx in newSound.currentUsedSourcesForSounds)
            {
                soundsToRecycle.Add(sfx);
                profilesToClean.Add(newSound);
            }
        }
        else
            RegisterSound(Sound);

    }
    public void SetAudio(AudioSettings newSettings)
    {
        LoadedSoundSettings = true;
        SetVolume(AudioType.Master, newSettings.MasterVolume);
        SetVolume(AudioType.Music, newSettings.MusicVolume);
        SetVolume(AudioType.SFX, newSettings.SFXVolume);
    }
    //Use this method to play any overarching sounds. only 1 can be played at a time
    public void PlayMusic(SoundProfile newProfile, bool loop = false)
    {
        _musicSource.Stop();
        _musicSource.clip = newProfile.Audio;
        _musicSource.loop = loop;
        CurrentMusicProfile = newProfile;
        _musicSource.Play();
    }
    public void PauseMusic(bool Pause)
    {
        if (Pause) _musicSource.Stop();
        else _musicSource.Play();
    }
    public void PlaySound(SoundProfile newProfile, Transform parentTransform, bool RandomizePitch = false, bool loopSound = false)
    {
        AudioSource sfx = GetFreeSource();
        sfx.clip = newProfile.Audio;

        RegisterSound(newProfile);
        SoundProfile soundProfile = SoundsDict[newProfile.SoundName];
        soundProfile.currentUsedSourcesForSounds.Add(sfx);

        sfx.transform.parent = parentTransform;
        sfx.transform.position = parentTransform.position;

        UsedSources.Add(sfx);
        UnusedSources.Remove(sfx);

        sfx.spatialBlend = 1;
        sfx.pitch = RandomizePitch ? Random.Range(.75f, 1.25f) : 1;
        sfx.loop = loopSound;
        if(soundProfile.SoundCurve != null)
            sfx.SetCustomCurve(AudioSourceCurveType.CustomRolloff, soundProfile.SoundCurve);

        sfx.Play();
    }
    public void PlaySound(SoundProfile newProfile, Vector3 Position, bool RandomizePitch = false, bool loopSound = false)
    {
        AudioSource sfx = GetFreeSource();
        sfx.clip = newProfile.Audio;

        RegisterSound(newProfile);
        SoundProfile soundProfile = SoundsDict[newProfile.SoundName];
        soundProfile.currentUsedSourcesForSounds.Add(sfx);

        sfx.transform.parent = null;
        sfx.transform.position = Position;

        UsedSources.Add(sfx);
        UnusedSources.Remove(sfx);

        if (Position == Vector3.zero)
            sfx.spatialBlend = 0;
        else
            sfx.spatialBlend = 1;
        sfx.pitch = RandomizePitch ? Random.Range(.75f, 1.25f) : 1;
        sfx.loop = loopSound;

        if (soundProfile.SoundCurve != null)
            sfx.SetCustomCurve(AudioSourceCurveType.CustomRolloff, soundProfile.SoundCurve);

        sfx.Play();
    }
    public void PlaySound(string clipName, Vector2 positon, float soundLevel = -1,bool RandomizePitch = false, bool loopSound = false)
    {
        AudioSource sfx = GetFreeSource();
        SoundProfile newProfile = new SoundProfile(clipName);
        RegisterSound(newProfile);
        SoundProfile soundProfile = SoundsDict[newProfile.SoundName];
        AudioClip clip;
        if(soundProfile.Audio == null)
        {
            clip = Resources.Load<AudioClip>(clipName);
            if (clip == null)
            {
                print("Sound " + clipName + " not found");
                return;
            }
        }
        else
            clip = soundProfile.Audio;
        
        sfx.clip = clip;

        soundProfile.currentUsedSourcesForSounds.Add(sfx);

        sfx.transform.parent = null;
        sfx.transform.position = positon;

        UsedSources.Add(sfx);
        UnusedSources.Remove(sfx);

        if (positon == Vector2.zero)
            sfx.spatialBlend = 0;
        else
            sfx.spatialBlend = 1;

        if (soundLevel == -1)
            newProfile.Volume = 1;
        else
            newProfile.Volume = soundLevel;

        sfx.pitch = RandomizePitch ? Random.Range(.75f, 1.25f) : 1;
        sfx.loop = loopSound;

        if (soundProfile.SoundCurve != null)
            sfx.SetCustomCurve(AudioSourceCurveType.CustomRolloff, soundProfile.SoundCurve);

        sfx.Play();
    }
    private AudioSource GetFreeSource()
    {
        //print("Getting new sources");
        if (UnusedSources.Count == 0)
        {
            if (UsedSources.Count != 0)
                RecycleAudioSource(UsedSources[UsedSources.Count - 1]);
            else
                RecycleAudioSource(null);
        }
        return UnusedSources[0];
    }
    private void RecycleAudioSource(AudioSource sfx)
    {
        //This method will recycle and clean any Audio Source so it can be used for the next
        //sound needed.
        if (UsedSources.Contains(sfx))
        {
            //If the source doesnt exist, get rid of it and make a new one to replace
            if (sfx == null)
            {
                print("Source is null");
                UsedSources.Remove(sfx);
            }
            sfx.Stop();

            //Making sure to remove sfx if its attached to a sound profile
            foreach (SoundProfile profile in profilesToClean)
                if (profile.currentUsedSourcesForSounds.Contains(sfx))
                    profile.currentUsedSourcesForSounds.Remove(sfx);
            //Resetting all modified values in the Audio Source
            sfx.pitch = 1;
            sfx.spatialBlend = 0f;
            if (UsedSources.Contains(sfx))
                UsedSources.Remove(sfx);
            sfx.transform.parent = transform;
            sfx.transform.position = transform.position;
            sfx.clip = null;
            sfx.rolloffMode = AudioRolloffMode.Logarithmic;
            UnusedSources.Add(sfx);
        }
    }
    public void SetVolume(AudioType type, float value)
    {
        if (!GameSettings) throw new System.Exception("Attach Game Settings");
        switch (type)
        {
            case AudioType.Master:
                SetMasterVolume(value);
                break;
            case AudioType.Music:
                SetMusicVolume(value);
                break;
            case AudioType.SFX:
                SetSFXVolume(value);
                break;
            default:break;
        }
    }
    private void SetMasterVolume(float value)
    {
        if (MasterVolume == value) return;
        GameSettings.MasterVolume = value;
    }
    private void SetMusicVolume(float value)
    {
        if (MusicVolume == value) return;
        GameSettings.MusicVolume = value;
    }
    private void SetSFXVolume(float value)
    {
        if (SFXVolume == value) return;
        GameSettings.SFXVolume = value;
    }
    public void PlayUISound(UISoundEffects soundType)
    {
        switch (soundType)
        {
            case UISoundEffects.ButtonPress:
                PlaySound(new SoundProfile("UIButtonClick"), Vector3.zero, false);
                break;
            case UISoundEffects.ButtonHover:
                PlaySound(new SoundProfile("UIButtonHover"), Vector3.zero, false);
                break;
            case UISoundEffects.PageSwap:
                PlaySound(new SoundProfile("UIPageSwap"), Vector3.zero, false);
                break;
        }
    }
    public enum AudioType { Master, Music, SFX }
    public enum UISoundEffects { ButtonPress, ButtonHover, PageSwap }
    public struct AudioSettings
    {
        public float MasterVolume;
        public float MusicVolume;
        public float SFXVolume;
    }
    private void OnEnable()
    {
        GameSettings.LoadSettings();
    }
    private void OnDisable()
    {
        GameSettings.SaveSettings();
    }
}

[System.Serializable]
public class SoundProfile
{
    public string SoundName;
    public float Volume;
    public AudioClip Audio;
    public List<AudioSource> currentUsedSourcesForSounds = new List<AudioSource>();
    public AnimationCurve SoundCurve;
    public SoundProfile(string Name)
    {
        SoundName = Name;
        Volume = 1;
        currentUsedSourcesForSounds = new List<AudioSource>();
        SoundCurve = null;
    }
    public SoundProfile(string Name, AudioClip Audio, float volume = 1)
    {
        SoundName = Name;
        Volume = volume;
        currentUsedSourcesForSounds = new List<AudioSource>();
        this.Audio = Audio;
        SoundCurve = null;
    }
    public void ClearSources()
    {
        currentUsedSourcesForSounds.Clear();
    }
}
