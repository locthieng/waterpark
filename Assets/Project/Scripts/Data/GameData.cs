using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public int Coin;
    public bool UnLockBoooster_1;
    public bool UnLockBoooster_2;
    public bool UnLockBoooster_3;
    public int DailyAdsViewCount;
    public string LastAdsViewDate;
    public int BestScore;
    public int LevelCycleCount;
    public int LevelIndex;
    public int replayCount = 0;
    public List<int> Levels;
}

[Serializable]
public class GameStats
{
    public bool NoAds;
    public int GamePlayCount;
    public int IAPCount;

    public string FirstOpenUtc;
    public string LastOpenUtc;

    public int RewardCount;
    public int InterCount;
}

