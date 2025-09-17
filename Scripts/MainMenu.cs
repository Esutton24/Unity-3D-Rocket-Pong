using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioClip mainMenuTheme;
    SoundProfile MMSFX;
    public UIMenu startMenu;
    private void Update()
    {
        if (!AudioManager.instance.HasSoundPlaying(MMSFX))
            AudioManager.instance.PlayMusic(MMSFX, true);
    }
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    private void Awake()
    {
        MMSFX = new SoundProfile("MMSFX", mainMenuTheme, .5f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void Start()
    {
        startMenu.SetActive(null);
    }
}
