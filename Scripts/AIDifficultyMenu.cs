using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDifficultyMenu : UIMenu
{
    [SerializeField] GameObject[] ButtonSelectionBackgrounds;
    [SerializeField] Settings settings;
    int currentDifficulty => settings? settings.AIDifficulty : 1;
    public void SetDifficulty(int difficulty)
    {
        settings.AIDifficulty = difficulty;
        UpdateDifficultyVisual();
    }
    void UpdateDifficultyVisual()
    {
        for (int i = 0; i < ButtonSelectionBackgrounds.Length; i++)
            ButtonSelectionBackgrounds[i].SetActive(i == currentDifficulty);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        settings.LoadSettings();
        UpdateDifficultyVisual();
    }
    private void OnDisable()
    {
        base.OnDisable();
        settings.SaveSettings();
    }
}
