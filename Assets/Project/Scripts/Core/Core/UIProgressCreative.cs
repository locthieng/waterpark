using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressCreative : MonoBehaviour
{
    [SerializeField] public Image fill;
    [SerializeField] private TMPro.TextMeshProUGUI txtPercent;

    public void SetProgress(float amount, float duration = 0f, Action callback = null)
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
    }
}
