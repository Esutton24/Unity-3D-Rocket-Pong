using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public TMP_Text Condition;

    public AudioClip GOTheme;
    SoundProfile GOSFX;

    public bool winLose = false;
    private void Start()
    {
        GOSFX = new SoundProfile("GOSFX", GOTheme, .2f);
        print(PlayerPrefs.GetInt("PlayerScore"));
        if (PlayerPrefs.GetInt("PlayerScore") == 11)
        {
            Condition.text = "YOU WIN!";
            Condition.color = new Color(0, 0.5773268f, 1, 1);
            if (!AudioManager.instance.HasSoundPlaying(GOSFX))
                AudioManager.instance.PlayMusic(GOSFX, false);
        }
        else if(PlayerPrefs.GetInt("AIScore") == 11)
        {
            Condition.text = "You Lose...";
            Condition.color = new Color(1, 0.01919135f, 0, 1);
            if (!AudioManager.instance.HasSoundPlaying(GOSFX))
                AudioManager.instance.PlayMusic(GOSFX, false);
        }
    }
    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
