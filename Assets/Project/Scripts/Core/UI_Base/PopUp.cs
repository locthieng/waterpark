using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{
    [SerializeField] private Image bg;
    [SerializeField] public CanvasGroup content;
    [SerializeField] protected Image contentImage;
    [SerializeField] protected TMPro.TextMeshProUGUI txtTitle;
    [SerializeField] protected TMPro.TextMeshProUGUI txtContent;
    [SerializeField] private UnityEvent onShown;
    protected Action onYesCallback;
    protected Action onHidden;
    protected Action onShow;

    protected string location = "home_icon";
    protected string showType = "pack";
    protected string productId;

    public void SetLocationForLogEvent(string location)
    {
        this.location = location;
    }

    public virtual void Show()
    {
        bg.gameObject.SetActive(true);
        bg.color = new Color(0, 0, 0, 0.9f);
        content.blocksRaycasts = true;
        content.transform.localScale = Vector3.one / 1.1f;
        if (onShown != null)
        {
            onShown.Invoke();
        }
        onShow?.Invoke();
        LeanTween.scale(content.gameObject, Vector3.one, 0.2f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
        {
        }).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(content, 1, 0.1f).setIgnoreTimeScale(true);
        //transform.SetAsLastSibling();
        //GlobalController.Instance.HideBanner();
    }
    public void SetOnHidden(Action onHidden)
    {
        this.onHidden = onHidden;
    }

    public void SetContent(string content)
    {
        txtContent.text = content;
    }

    public virtual void Hide()
    {
        LeanTween.scale(content.gameObject, Vector3.one / 1.1f, 0.1f).setEase(LeanTweenType.easeOutQuad).setIgnoreTimeScale(true);
        content.blocksRaycasts = false;
        bg.color = new Color(0, 0, 0, 0);
        LeanTween.alphaCanvas(content, 0, 0.1f).setOnComplete(() =>
        {
            GameUIController.Instance.SetInteractBoosterUI(true);
            bg.gameObject.SetActive(false);
            //GlobalController.Instance.ShowBanner();
            onHidden?.Invoke();
            onHidden = null;
        }).setIgnoreTimeScale(true);
    }

    public void SetCallback(Action callback)
    {
        onYesCallback = callback;
    }

    public void OnCallback()
    {
        if (onYesCallback != null)
        {
            onYesCallback();
            onYesCallback = null;
        }
    }
}
