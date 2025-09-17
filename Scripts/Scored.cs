using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scored : MonoBehaviour
{
    public int homeScore = 0;
    public int awayScore = 0;
    private int scoreHome;
    private int scoreAway;
    

    GameManager game;

    private void Start()
    {
        game = FindObjectOfType<GameManager>();
        PlayerPrefs.SetInt("PlayerScore", homeScore);
        PlayerPrefs.SetInt("AIScore", awayScore);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("HomeGoal"))
        {
            homeScore++;
            SetHomeScore();
            PlayerPrefs.SetInt("PlayerScore", homeScore);
            //game.respawnPlayer();
            game.updateScore();
        }
        if (collision.gameObject.CompareTag("AwayGoal"))
        {
            awayScore++;
            SetAwayScore();
            PlayerPrefs.SetInt("AIScore", awayScore);
            //game.respawnPlayer();
            game.updateScore();
        }
    }
    public void SetHomeScore()
    {
        scoreHome = homeScore;
    }

    public int GetHomeScore()
    {
        return scoreHome;
    }

    public void SetAwayScore()
    {
        scoreAway = awayScore;
    }

    public int GetAwayScore()
    {
        return scoreAway;
    }
    
}
