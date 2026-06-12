using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILife : MonoBehaviour
{
    [SerializeField] private UIDigitalClock lifeClock;
    [SerializeField] private GameObject imageInfinite;
    [SerializeField] private TMPro.TextMeshProUGUI txtLife;
    [SerializeField] private Image imageLifePlus;
    [SerializeField] private UIButton buttonLifeField;

    [Header("TimeInfinite > 60ph")]
    [SerializeField] private RectTransform rectLife;
    [SerializeField] private RectTransform rectBg;
    [SerializeField] private RectTransform rectTime;
    [SerializeField] private float[] rectLifes;
    [SerializeField] private Vector2[] rectBgs;
    [SerializeField] private float[] rectTimes;

    LifeSystem lifeSystem => LifeSystem.Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetUp();
    }

    public void SetUp()
    {
        lifeClock.OnEnd = OnLifeClockEnd;
        lifeSystem.OnLifeChanged = RefreshUI;
        lifeSystem.CheckLifeFrozenStatus();
        RefreshUILife();
    }

    //private void OnDestroy()
    //{
    //    lifeSystem.OnLifeChanged -= RefreshUI;
    //}

    public void RefreshUILife()
    {
        if (lifeSystem.IsInfinite)
        {
            buttonLifeField.enabled = false;
            imageLifePlus.color = new Color(1, 1, 1, 0f);
            SetUpUILifeInfinite();
        }
        else
        {
            lifeClock.OnMilestone = null;
            AdjustUILayout(false);
            if (lifeSystem.MaxLife > lifeSystem.CurrentLife)
            {
                buttonLifeField.enabled = true;
                imageLifePlus.color = Color.white;
                if (!string.IsNullOrEmpty(lifeSystem.lastLifeFillTime.ToString()))
                {
                    int lifeRefillProgressTime = (int)(DateTime.Now - DateTime.Parse(lifeSystem.lastLifeFillTime.ToString())).TotalSeconds;
                    int remainLifeRefillTime = lifeSystem.refillSeconds - lifeRefillProgressTime;
                    if (remainLifeRefillTime <= 0)
                    {
                        int refilledLives = lifeRefillProgressTime / lifeSystem.refillSeconds;
                        if (refilledLives > lifeSystem.MaxLife - lifeSystem.CurrentLife)
                        {
                            refilledLives = lifeSystem.MaxLife - lifeSystem.CurrentLife;
                        }
                        remainLifeRefillTime = lifeRefillProgressTime - lifeSystem.refillSeconds * refilledLives;
                        lifeSystem.lastLifeFillTime = DateTime.Now.AddSeconds(-remainLifeRefillTime);
                        lifeSystem.AddLife(refilledLives);

                    }
                    if (lifeSystem.MaxLife > lifeSystem.CurrentLife && remainLifeRefillTime > 0)
                    {
                        lifeClock.OnEnd = OnLifeClockEnd;
                        lifeClock.SetUpTimer(remainLifeRefillTime);
                        lifeClock.StartTimer();

                    }
                    else if (lifeSystem.MaxLife == lifeSystem.CurrentLife)
                    {
                        buttonLifeField.enabled = false;
                        imageLifePlus.color = new Color(1, 1, 1, 0.0f);
                        lifeClock.SetTimerText("Full");
                    }
                }
            }
            else
            {
                lifeClock.Stop();
                buttonLifeField.enabled = false;
                imageLifePlus.color = new Color(1, 1, 1, 0.0f);
                lifeClock.SetTimerText("Full");
            }
        }
        imageInfinite.SetActive(lifeSystem.IsInfinite);
        txtLife.gameObject.SetActive(!lifeSystem.IsInfinite);
        txtLife.text = lifeSystem.CurrentLife.ToString();
        lifeSystem.Save();
    }    

    // nếu hết giờ 
    private void OnLifeClockEnd()
    {
        lifeSystem.AddLife(1,
            ()=>
            {
                // max 
                txtLife.gameObject.SetActive(!lifeSystem.IsInfinite);
                imageInfinite.SetActive(lifeSystem.IsInfinite);
                lifeClock.SetTimerText("Full");
                imageLifePlus.color = new Color(1, 1, 1, 0.0f);
                buttonLifeField.enabled = false;
                lifeClock.Stop();
            },
            () =>
            {
                // chưa max
                txtLife.gameObject.SetActive(true);
                imageInfinite.SetActive(false);
                lifeClock.SetUpTimer(lifeSystem.RefillSeconds);
                lifeClock.StartTimer();
            });
    }

    public void RefreshUI(int currenLife)
    {
        txtLife.text = currenLife.ToString();
        if (currenLife == lifeSystem.MaxLife)
        {
            //lifeClock.SetTimerText("MAX"); 
            SetUIHeartMax();
        }
    }

    public void SetUIHeartMax()
    {
        txtLife.gameObject.SetActive(!lifeSystem.IsInfinite);
        imageInfinite.SetActive(lifeSystem.IsInfinite);
        lifeClock.SetTimerText("Full");
        imageLifePlus.color = new Color(1, 1, 1, 0.0f);
        buttonLifeField.enabled = false;
        lifeClock.Stop();
    }

    public void SetUpUILifeInfinite()
    {
        if (lifeSystem.IsInfinite)
        {
            int remainingSeconds = (int)(lifeSystem.infiniteExpireTime - DateTime.Now).TotalSeconds;
            if (remainingSeconds > 0)
            {
                AdjustUILayout(remainingSeconds > 3600);
                lifeClock.SetUpTimer(remainingSeconds);
                lifeClock.StartTimer();
                lifeClock.OnMilestone = (sec) =>
                {
                    AdjustUILayout(sec > 3600);
                };
                lifeClock.OnEnd = () =>
                {
                    lifeSystem.CheckLifeFrozenStatus();
                    RefreshUILife();
                };
            }
            else
            {
                AdjustUILayout(false);
                lifeClock.Stop();
                lifeClock.SetTimerText("Full");
            }
        }
        else
        {
            AdjustUILayout(false);
            lifeClock.Stop();
            lifeClock.SetTimerText("Full");
        }
    }    

    private void AdjustUILayout(bool isGreaterThanOneHour)
    {
        if (rectLife == null || rectBg == null || rectTime == null) return;
        if (rectLifes == null || rectLifes.Length < 2) return;
        if (rectTimes == null || rectTimes.Length < 2) return;
        if (rectBgs == null || rectBgs.Length < 2) return;

        int index = isGreaterThanOneHour ? 1 : 0;

        var lifePos = rectLife.anchoredPosition;
        lifePos.x = rectLifes[index];
        rectLife.anchoredPosition = lifePos;

        var timePos = rectTime.anchoredPosition;
        timePos.x = rectTimes[index];
        rectTime.anchoredPosition = timePos;

        var bgPos = rectBg.anchoredPosition;
        bgPos.x = rectBgs[index].x;
        rectBg.anchoredPosition = bgPos;

        var bgSize = rectBg.sizeDelta;
        bgSize.x = rectBgs[index].y;
        rectBg.sizeDelta = bgSize;
    }
}
