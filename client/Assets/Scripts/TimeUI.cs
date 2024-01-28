using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeUI : MonoBehaviour
{
    public TimeScore timeScore;
    private TMPro.TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMPro.TMP_Text>();    
    }

    void Update()
    {
        int minutes = Mathf.FloorToInt(timeScore.currentTime / 60.0f);

        string minutesText = minutes < 10? "0" + minutes : minutes.ToString();

        int seconds = Mathf.FloorToInt(timeScore.currentTime - (minutes * 60));

        string secondsText = seconds < 10 ? "0" + seconds : seconds.ToString();

        int microseconds = Mathf.FloorToInt((timeScore.currentTime - (seconds + (minutes * 60))) * 100);

        string microsecondsText = microseconds < 10 ? "0" + microseconds : microseconds.ToString();

        text.text = minutesText + ":" + secondsText + "." + microsecondsText;
    }
}
