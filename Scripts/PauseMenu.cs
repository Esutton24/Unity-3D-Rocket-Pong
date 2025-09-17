using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool gamePaused = false;
    public UIMenu pauseMenuUI;
    [SerializeField] InputManager _input;
    public bool gameisPaused = false;
    public void TogglePause() => TogglePause(!gamePaused);
    public void TogglePause(bool isPaused)
    {
        bool UIActive = GetComponentInChildren<UIMenu>();
        print("UI is active: " + UIActive);
        if (isPaused && UIActive)
        {
            print("Already UI Active. Cannot pause");
            return;
        }
        gamePaused = isPaused;
        print("Game Paused: " + gamePaused);
        Cursor.visible = gamePaused;
        pauseMenuUI.gameObject.SetActive(gamePaused);
        Time.timeScale = gamePaused ? 0 : 1;
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void LoadMenu()
    {
        TogglePause(false);
        SceneManager.LoadScene("MainMenu");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    private void OnEnable()
    {
        _input.PauseButton.performed += _ctx => TogglePause();
        pauseMenuUI.ExitMenu += PauseOff;
        TogglePause(false);
    }
    private void OnDisable()
    {
        _input.PauseButton.performed -= _ctx => TogglePause();
        pauseMenuUI.ExitMenu -= PauseOff;

    }
    void PauseOff() => TogglePause(false);
    private void Update()
    {
        gameisPaused = gamePaused;
    }
}
