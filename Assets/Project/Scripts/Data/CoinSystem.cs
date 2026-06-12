using System;
using UnityEngine;

public class CoinSystem : MonoBehaviour
{
    public static CoinSystem Instance { get; set; }

    public event Action<int> OnCoinChanged;

    private const string COIN_KEY = "PLAYER_COIN";

    public int coin;
    /*public int Coin
    {
        get => _coin;
        set => _coin = value;
    }*/

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCoin();
    }

    #region Load / Save

    public void LoadCoin()
    {
        coin = PlayerPrefs.GetInt(COIN_KEY, 450);
        Notify();
    }

    public void SaveCoin()
    {
        PlayerPrefs.SetInt(COIN_KEY, coin);
        PlayerPrefs.Save();
    }

    #endregion

    #region Public API

    public void AddCoin(int amount)
    {
        //Debug.Log(coin + "Coin");
        if (amount < 0) return;

        coin += amount;
        SaveCoin();
        Notify();

    }

    public bool SpendCoin(int amount)
    {
        if (amount < 0) return true;
        if (coin < amount) return false;

        coin -= amount;
        SaveCoin();
        Notify();
        return true;
    }

    public void SetCoin(int value)
    {
        coin = Mathf.Max(0, value);
        SaveCoin();
        Notify();
    }

    public bool HasEnough(int amount)
    {
        return coin >= amount;
    }

    #endregion

    private void Notify()
    {
        OnCoinChanged?.Invoke(coin);
    }
}