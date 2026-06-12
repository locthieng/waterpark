using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProgress : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private TMPro.TextMeshProUGUI txtPercent;
    [SerializeField] private GameObject[] starsPass;

    public void SetProgress(float amount, int starPass = 0, float duration = 0f, Action callback = null)
    {
        if (duration > 0)
        {
            LeanTween.value(fill.fillAmount, amount, duration).setOnUpdate((float f) =>
            {
                if (fill != null)
                {
                    fill.fillAmount = f;
                }
                if (txtPercent != null)
                {
                    txtPercent.text = (int)(100 * f) + "%";
                }
            }).setOnComplete(callback);
        }
        else
        {
            fill.fillAmount = amount;
        }
        for (int i = 0; i < starPass; i++)
        {
            starsPass[i].SetActive(true);
        }
        for (int i = starPass; i < starsPass.Length; i++)
        {
            starsPass[i].SetActive(false);
        }
    }
}
