using Ranged;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UICoin : MonoBehaviour
{
    public static UICoin Instance { get; set; }

    [SerializeField] public TMPro.TextMeshProUGUI[] txtCoin;
    [SerializeField] private Transform coinIconPrefab;
    [SerializeField] private Transform[] boosterHeartIconPrefab;
    [SerializeField] private Transform coinStartIcon;
    [SerializeField] private Transform coinEndIcon;
    [SerializeField] private AnimationCurve flyCurve;
    [SerializeField] private rfloat randMoneyMoveRadius = new rfloat(3f, 4f);
    private Vector3 movePos;
    private float angle;

    private void Start()
    {
        Instance = this;
    }

    public void ShowFlyingClaim(int amount, Action callback, bool isShop = false, Transform containerCoin = null)
    {
        if (containerCoin == null) { containerCoin = flyingCoinsContainer; }
        ShowFlyingCoinsFrom(containerCoin, amount, callback, isShop);
    }    

    public void ShowFlyingCoinsFrom(Transform coinStart, int total, Action callback, bool isShop = false)
    {
        coinStartIcon = coinStart;
        coinEndIcon = GlobalController.CurrentStage == StageScreen.Home ? coinEndHome : coinEndInGame;
        ShowFlyingCoins(total, callback);
    }

    [SerializeField] private Transform coinEndHome;
    [SerializeField] private Transform coinEndInGame;
    [SerializeField] public Transform flyingCoinsContainer;
    [SerializeField] private int minFlyingCoins = 10;
    [SerializeField] private int bundleCoinFlyCount = 5;
    [SerializeField] private AudioClip sfxCoin;

    public void ShowFlyingCoins(int total, Action callback)
    {
        //Debug.Log("updateCoin");
        if (total > 10) total = 18;
        //if (total < minFlyingCoins) total = minFlyingCoins;
        float gapAngle = Mathf.PI * 2 / total;
        for (int i = 0; i < total; i++)
        {
            Transform m = Instantiate(coinIconPrefab, flyingCoinsContainer);
            m.position = coinStartIcon.position;
            angle = gapAngle * i;
            float moneyMoveDelay = 0.1f + 0.05f * i;
            bool callBackActive = i == 0;
            movePos = m.position;
            movePos.x += Mathf.Sin(angle) * randMoneyMoveRadius.RandomValue;
            //movePos.z += Mathf.Cos(angle) * randMoneyMoveRadius.RandomValue;
            movePos.y += Mathf.Cos(angle) * randMoneyMoveRadius.RandomValue;
            m.LeanMove(movePos, UnityEngine.Random.Range(0.5f, 0.8f)).setDelay(UnityEngine.Random.Range(0.0f, 0.1f)).setEase(LeanTweenType.easeOutBack).setOnComplete(() =>
            {
                //m.LeanScale(Vector3.one * 0.5f, 0.3f)/*.setDelay(moneyMoveDelay - 0.1f)*/;
                SoundController.Instance.PlaySingle(SoundController.Instance.Coin);
                m.LeanMove(coinEndIcon.position, UnityEngine.Random.Range(0.7f, 1.2f))/*.setDelay(moneyMoveDelay)*/.setEase(flyCurve).setOnComplete(() =>
                {
                    // Update total money on first item arrival
                    if (callBackActive)
                    {
                        callback?.Invoke();
                    }
                    Destroy(m.gameObject);
                    LeanTween.scale(coinEndHome.gameObject, Vector3.one * 1.2f, 0f);
                    LeanTween.cancel(coinEndHome.gameObject);
                    LeanTween.scale(coinEndHome.gameObject, Vector3.one, 0.1f);
                });
            });
        }

    }
    public void ShowFlyingBundlesFrom(Action callback, Transform container = null)
    {
        if (container == null) { container = flyingCoinsContainer; }
        coinStartIcon = container;
        if (GameUIController.Instance != null)
            //coinEndIcon = GameUIController.Instance.flyTargetCoin;
        ShowFlyingBundles(callback);
    }

    /// <summary>1 icon mỗi loại booster + bundleCoinFlyCount coin bay về coinEndIcon.</summary>
    public void ShowFlyingBundles(Action callback)
    {
        if (coinStartIcon == null || coinEndIcon == null || flyingCoinsContainer == null
            || coinIconPrefab == null)
        {
            callback?.Invoke();
            return;
        }

        int totalIcons = bundleCoinFlyCount;
        if (boosterHeartIconPrefab != null)
        {
            for (int t = 0; t < boosterHeartIconPrefab.Length; t++)
            {
                if (boosterHeartIconPrefab[t] != null)
                    totalIcons++;
            }
        }

        if (totalIcons == 0)
        {
            callback?.Invoke();
            return;
        }

        int[] arrived = { 0 };
        int spawnIndex = 0;
        int slotCount = totalIcons;

        if (boosterHeartIconPrefab != null)
        {
            int typeCount = boosterHeartIconPrefab.Length;
            for (int typeIndex = 0; typeIndex < typeCount; typeIndex++)
            {
                Transform prefab = boosterHeartIconPrefab[typeIndex];
                if (prefab == null) continue;

                float burstAngle = (Mathf.PI * 2f / slotCount) * spawnIndex;
                FlyIconToEnd(prefab, burstAngle, spawnIndex, arrived, totalIcons, callback, playCoinSfx: false);
                spawnIndex++;
            }
        }

        for (int i = 0; i < bundleCoinFlyCount; i++)
        {
            int idx = spawnIndex + i;
            float burstAngle = (Mathf.PI * 2f / slotCount) * idx;
            FlyIconToEnd(coinIconPrefab, burstAngle, idx, arrived, totalIcons, callback, playCoinSfx: true);
        }
    }

    private void FlyIconToEnd(Transform prefab, float burstAngle, int delayIndex, int[] arrived, int totalIcons, Action callback, bool playCoinSfx)
    {
        Transform icon = Instantiate(prefab, flyingCoinsContainer);
        icon.position = coinStartIcon.position;

        Vector3 burstPos = icon.position;
        burstPos.x += Mathf.Sin(burstAngle) * randMoneyMoveRadius.RandomValue;
        burstPos.y += Mathf.Cos(burstAngle) * randMoneyMoveRadius.RandomValue;

        float burstDelay = 0.05f * delayIndex;
        icon.LeanMove(burstPos, UnityEngine.Random.Range(0.5f, 0.8f))
            .setDelay(UnityEngine.Random.Range(0f, 0.1f) + burstDelay)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                if (playCoinSfx && SoundController.Instance != null)
                    SoundController.Instance.PlaySingle(SoundController.Instance.Coin);

                icon.LeanMove(coinEndIcon.position, UnityEngine.Random.Range(0.7f, 1.2f))
                    .setEase(flyCurve)
                    .setOnComplete(() =>
                    {
                        Destroy(icon.gameObject);
                        PulseCoinEndTarget();

                        arrived[0]++;
                        if (arrived[0] >= totalIcons)
                            callback?.Invoke();
                    });
            });
    }

    private void PulseCoinEndTarget()
    {
        if (coinEndHome == null) return;
        LeanTween.scale(coinEndHome.gameObject, Vector3.one * 1.2f, 0f);
        LeanTween.cancel(coinEndHome.gameObject);
        LeanTween.scale(coinEndHome.gameObject, Vector3.one, 0.1f);
    }

    public Vector3 GetWorldPosInHighUI(Transform sourceTransform)
    {
        if (GameUIController.Instance == null || sourceTransform == null)
            return sourceTransform != null ? sourceTransform.position : Vector3.zero;

        Camera camUI = CameraController.Instance.UICamera;
        Camera camHighUI = CameraController.Instance.HighUICamera;

        if (camUI == null || camHighUI == null) return sourceTransform.position;

        // Chuyển tọa độ từ CameraUI sang Screen Space -> sang CameraHighUI
        Vector3 screenPoint = camUI.WorldToScreenPoint(sourceTransform.position);
        Vector3 worldPointHighUI = camHighUI.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, camHighUI.nearClipPlane));

        return worldPointHighUI;
    }

    public void UpdateCoin(TMPro.TextMeshProUGUI txtCoin, int changeValue, float duration = 0.4f, Action callback = null)
    {
        if (txtCoin == null)
        {
            txtCoin = GlobalController.CurrentStage == StageScreen.Home ? this.txtCoin[0] : this.txtCoin[1];
        }
        // Effects
        LeanTween.cancel(txtCoin.gameObject);
        LeanTween.scale(txtCoin.gameObject, Vector3.one * 1.1f, 0.05f).setOnComplete(() =>
        {
            LeanTween.scale(txtCoin.gameObject, Vector3.one, 0.25f);
        });
        LeanTween.value(CoinSystem.Instance.coin - changeValue, CoinSystem.Instance.coin, duration).setOnUpdate((float f) =>
        {
            txtCoin.text = f >= 1000 ? (f / 1000f).ToString("0.0") + "k" : f.ToString("0"); ;
        }).setEase(LeanTweenType.linear).setOnComplete(callback);

        CoinSystem.Instance.SaveCoin();
    }

    /*public void UpdateCoin(TMPro.TextMeshProUGUI txtCoin, int previousValue, int value, float duration = 1f, Action callback = null)
    {
        if (txtCoin == null)
        {
            txtCoin = GlobalController.CurrentStage == StageScreen.Home ? this.txtCoin[0] : this.txtCoin[1];
        }
        // Effects
        LeanTween.cancel(txtCoin.gameObject);
        LeanTween.scale(txtCoin.gameObject, Vector3.one * 1.5f, 0.05f).setOnComplete(() =>
        {
            LeanTween.scale(txtCoin.gameObject, Vector3.one, 0.25f);
        });
        LeanTween.value(previousValue, value, duration).setOnUpdate((float f) =>
        {
            txtCoin.text = f >= 1000 ? (f / 1000f).ToString("0.0") + "k" : f.ToString("0"); ;
            //txtCoinCollected.text = (f - previousValue).ToString("0");
        }).setOnComplete(callback);
        CoinSystem.Instance.SaveCoin();

    }*/

}
