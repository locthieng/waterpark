using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class UIDigitalClock : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI txtTimer;
    [SerializeField] private Animator animWarning;
    [SerializeField] private bool showTimeUnit;
    public Action OnEnd { get; set; }
    public UnityAction<int> OnMilestone;
    private int hours;
    private int minutes;
    private int seconds;
    private int digit1;
    private int digit2;
    private int digit3;
    private string unit1 = "h";
    private string unit2 = "m";
    private string unit3 = "s";
    private string timeUnit1;
    private string timeUnit2;
    private float miliseconds;
    private int totalSeconds;
    private float totalMiliseconds;
    private bool isTimeRunning;

    public void SetTimerText(string text)
    {
        txtTimer.text = text;
    }

    public void SetWarningActive(bool isActive)
    {
        if (isActive)
        {
            animWarning.enabled = true;
            animWarning.Play("Warning", 0, 0);
        }
        else
        {
            animWarning.enabled = false;
            txtTimer.color = Color.white;
        }
    }

    public void SetUpTimer(int time)
    {
        totalSeconds = time;
        totalMiliseconds = time * 100f;
        CalculateTime();
        UpdateTimeText();
    }

    public void StartTimer(float delay = 0)
    {
        Invoke(nameof(StartTimerDelay), delay);
    }

    private void StartTimerDelay()
    {
        isTimeRunning = true;
    }

    void Update()
    {
        if (!isTimeRunning) return;
        if (miliseconds <= 0)
        {
            miliseconds = 0;
            if (totalSeconds <= 0)
            {
                totalSeconds = 0;
                isTimeRunning = false;
                //asTick.Stop();
                UpdateTimeText();
                OnEnd?.Invoke();
                return;
            }
            else if (totalSeconds > 0)
            {
                totalSeconds--;
                OnMilestone?.Invoke(totalSeconds);
                CalculateTime();
            }
            miliseconds = 100;
        }
        UpdateTimeText();
        miliseconds -= Time.deltaTime * 100;
        totalMiliseconds -= Time.deltaTime * 100;
    }

    private void UpdateTimeText()
    {
        if (hours > 0)
        {
            if (showTimeUnit)
            {
                txtTimer.text = string.Format("{0:00}h{1:00}m", hours, minutes);
            }
            else
            {
                txtTimer.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            }
        }
        else
        {
            if (showTimeUnit)
            {
                txtTimer.text = string.Format("{0:00}m{1:00}s", minutes, seconds);
            }
            else
            {
                txtTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    private void CalculateTime()
    {
        hours = totalSeconds / 3600;
        minutes = (totalSeconds - hours * 3600) / 60;
        seconds = totalSeconds - hours * 3600 - minutes * 60;
    }

    public void Stop()
    {
        isTimeRunning = false;
    }

    public void Resume()
    {
        isTimeRunning = true;
    }
}
