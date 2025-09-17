using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System;

public class GameManager : MonoBehaviour
{
    public Scored setScore;
    public bool gamePlaying { get; private set; }
    [SerializeField] private GameObject wallPlayer;
    [SerializeField] private GameObject wallAI;
    [SerializeField] private Transform respawnPlayerPosition;
    [SerializeField] private Transform respawnAIPosition;
    private int homeScore;
    private int awayScore;
    public TMP_Text homeTeamScoreText;
    public TMP_Text awayTeamScoreText;
    
    public TMP_Text TeamScored;

    private int countdownTimer = 3;
    public TMP_Text countdownDisplay;

    public AudioClip startSound, mPointSound, mPS;
    SoundProfile startSFX, mPointSFX, mPSFX;

    public AudioClip mThemeSound;
    SoundProfile mThemeSFX;
    public bool ShowDebug;
    static List<GizmosDrawSettings> gizmosSettings = new List<GizmosDrawSettings>();
    List<GizmosDrawSettings> mostRecentSettings = new List<GizmosDrawSettings>();
    private void Start()
    {
        gamePlaying = false;

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        startSFX = new SoundProfile(gameObject.name + " - Start");
        startSFX.Volume = 0.1f;
        startSFX.Audio = startSound;

        mThemeSFX = new SoundProfile(gameObject.name + " - mTheme");
        mThemeSFX.Volume = 0.1f;
        mThemeSFX.Audio = mThemeSound;

        mPointSFX = new SoundProfile(gameObject.name + " - MatchPoint");
        mPointSFX.Volume = 1.5f;
        mPointSFX.Audio = mPointSound;

        mPSFX = new SoundProfile(gameObject.name + " - MatchPoint Voice");
        mPSFX.Volume = 0.3f;
        mPSFX.Audio = mPS;

        TeamScored.gameObject.SetActive(false);
        StartCoroutine(CountdownToStart());
       
    }
    public IEnumerator pointScored(string teamName)
    {
        Time.timeScale = 0.25f;
        TeamScored.gameObject.SetActive(true);
        if (teamName == "Home")
        {
            TeamScored.color = new Color(0, 0.5773268f, 1, 1);
            TeamScored.text = "HOME SCORED!";
            if (homeScore == 9)
            {
                TeamScored.text = "MATCH POINT!";
            }
        }
        if (teamName == "Away")
        {
            TeamScored.color = new Color(1, 0.01919135f, 0, 1);
            TeamScored.text = "AWAY SCORED!";
            if (awayScore == 9)
            {
                TeamScored.text = "MATCH POINT!";
            }
        }
        yield return new WaitForSecondsRealtime(1.5f);
        TeamScored.gameObject.SetActive(false);
        respawnPlayer();
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1f;
    }
    public void updateScore()
    {
        KeepScore();

        homeScore = setScore.GetHomeScore();
        awayScore = setScore.GetAwayScore();

        if (homeScore == 10 || awayScore == 10)
        {
            AudioManager.instance.StopSound(mThemeSFX);
            AudioManager.instance.PlaySound(mPSFX, Vector3.zero);
            if (!AudioManager.instance.HasSoundPlaying(mPointSFX))
                AudioManager.instance.PlayMusic(mPointSFX, true);

        }

        GameEnd();
    }
    void respawnPlayer() 
    {
        // Reset player to defaults
        wallPlayer.GetComponent<Rigidbody>().velocity = Vector3.zero;
        wallPlayer.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        wallPlayer.transform.position = respawnPlayerPosition.position;
        wallPlayer.transform.GetChild(0).transform.rotation = Quaternion.identity;
        wallPlayer.GetComponent<PlayerWall>().clearRockets();

        // Reset AI to defaults
        wallAI.transform.position = respawnAIPosition.position;
        wallAI.GetComponent<Rigidbody>().velocity = Vector3.zero;
        wallAI.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        wallAI.transform.GetChild(0).transform.rotation = Quaternion.identity;
    }
    public void KeepScore()
    {
        homeTeamScoreText.text = $"Home: {setScore.homeScore}";
        awayTeamScoreText.text = $"Away: { setScore.awayScore}";
    }
    public void BeginGame()
    {
        gamePlaying = true;
    }
    public void GameEnd()
    {
        gamePlaying = false;

        if (homeScore >= 11 || awayScore >= 11)
        {
            AudioManager.instance.PauseMusic(true);
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
    public IEnumerator CountdownToStart()
    {
        AudioManager.instance.PauseMusic(true);
        AudioManager.instance.PlaySound(startSFX, Vector3.zero);
        for(int i = 3; i > 0; i--)
        {
            countdownDisplay.text = i.ToString();
            float secondTimer = 1;
            while(secondTimer > 0)
            {
                Time.timeScale = 0;

                if (!PauseMenu.gamePaused)
                    secondTimer -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        countdownDisplay.text = "GO!";

        yield return new WaitForSecondsRealtime(0.5f);
        countdownDisplay.gameObject.SetActive(false);

        Time.timeScale = 1;
        AudioManager.instance.PlayMusic(mThemeSFX, true);
    }
    public static void DrawGizmos(GizmoDraw drawType, Vector3 startPosition, Color gColor, float length = 0, Vector3 secondVector3 = new Vector3())
    {
        gizmosSettings.Add(new GizmosDrawSettings(drawType, startPosition, secondVector3, length, gColor));
    }
    private void OnDrawGizmos()
    {
        if (!ShowDebug) return;
        if (!Application.isPlaying)
        {
            foreach (GizmosDrawSettings gs in mostRecentSettings)
            {
                Gizmos.color = gs.GColor;
                switch (gs.DrawMode)
                {
                    case GizmoDraw.Sphere: DrawSphere(gs); break;
                    case GizmoDraw.Line: DrawLine(gs); break;
                    case GizmoDraw.Cube: DrawCube(gs); break;
                }
            }
        }
        else
        {
            foreach (GizmosDrawSettings gs in gizmosSettings)
            {
                Gizmos.color = gs.GColor;
                switch (gs.DrawMode)
                {
                    case GizmoDraw.Sphere: DrawSphere(gs); break;
                    case GizmoDraw.Line: DrawLine(gs); break;
                    case GizmoDraw.Cube: DrawCube(gs); break;
                }
            }
            mostRecentSettings = gizmosSettings;
            gizmosSettings = new List<GizmosDrawSettings>();
        }
        void DrawSphere(GizmosDrawSettings gs)
        {
            Gizmos.DrawSphere(gs.StartPosition, gs.Length);
        }
        void DrawLine(GizmosDrawSettings gs)
        {
            Gizmos.DrawLine(gs.StartPosition, gs.secondVector3);
        }
        void DrawCube(GizmosDrawSettings gs)
        {
            Gizmos.DrawCube(gs.StartPosition, gs.secondVector3);
        }
    }
    public class GizmosDrawSettings
    {
        public GizmoDraw DrawMode;
        public Vector3 StartPosition, secondVector3;
        public float Length;
        public Color GColor;
        public GizmosDrawSettings(GizmoDraw drawMode, Vector3 startPosition, Vector3 secondVector3, float length, Color gColor)
        {
            DrawMode = drawMode;
            StartPosition = startPosition;
            this.secondVector3 = secondVector3;
            Length = length;
            GColor = gColor;
        }
    }
}
public enum GizmoDraw { Sphere, Line, Cube }