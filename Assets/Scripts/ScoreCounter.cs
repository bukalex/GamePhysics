using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreCounter : MonoBehaviour
{
    private TMP_Text scoreUI;
    private int score = 0;

    private void Awake()
    {
        scoreUI = GetComponent<TMP_Text>();
    }

    public void ChangeScore(int value)
    {
        score += value;
        scoreUI.text = score.ToString();
    }
}
