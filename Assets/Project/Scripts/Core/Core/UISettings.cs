using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISettings : MonoBehaviour
{
    // Thay đổi từ Toggle sang Slider
    [SerializeField] private Slider sliderSound;
    [SerializeField] private Slider sliderBgm;
    [SerializeField] private UIToggleButton tgHaptic;

    [SerializeField] private CanvasGroup canvas;
    [SerializeField] private GameObject content;

    private void Start()
    {
        sliderSound.onValueChanged.AddListener(OnSliderSoundChanged);
        sliderBgm.onValueChanged.AddListener(OnSliderBgmChanged);
        tgHaptic.OnValueChange.AddListener(OnToggleHaptic);
    }

    private void OnToggleHaptic(bool isOn)
    {
        PlayerPrefs.SetInt("IsHapticOn", isOn ? 1 : 0);
        GlobalController.IsHapticOn = isOn;
    }

    private void OnSliderBgmChanged(float value)
    {
        PlayerPrefs.SetFloat("BgmVolume", value);
        GlobalController.BgmVolume = value;

        bool isOn = value > 0;
        GlobalController.IsBgmOn = isOn;
        PlayerPrefs.SetInt("IsBgmOn", isOn ? 1 : 0);


        if (isOn)
        {
            SoundController.Instance.Unmute();
        }
        else
        {
            SoundController.Instance.Mute();
        }
    }

    private void OnSliderSoundChanged(float value)
    {
        PlayerPrefs.SetFloat("SoundVolume", value);
        GlobalController.SoundVolume = value;

        bool isOn = value > 0;
        GlobalController.IsSoundOn = isOn;
        PlayerPrefs.SetInt("IsSoundOn", isOn ? 1 : 0);

        // Gọi hàm chỉnh âm lượng hiệu ứng âm thanh
        // SoundController.Instance.SetSoundVolume(value);
    }

    public void Show()
    {
        if (StageController.Instance.IsOver) return;
        gameObject.SetActive(true);

        content.transform.localScale = Vector3.one * 0.6f;

        LeanTween.alphaCanvas(canvas, 1, 0.2f).setOnComplete(() =>
        {
            canvas.blocksRaycasts = true;
        });

        LeanTween.scale(content.gameObject, Vector3.one, 0.2f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
        {
        });

        sliderSound.value = GlobalController.SoundVolume;
        sliderBgm.value = GlobalController.BgmVolume;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        LeanTween.alphaCanvas(canvas, 0, 0.1f).setOnComplete(() =>
        {
            canvas.blocksRaycasts = false;
        });
    }
}