using System;
using UnityEngine;
//using Spine.Unity;
public enum BoosterType
{
    Clock = 0,
    Hammer = 1,
    Saw = 2
}

[Serializable]
public class BoosterItemData
{
    public BoosterType Type;
    public string Name;
    public string Description;
    public int PriceInCoin;
    public int UnlockLevel;
    public int GiveAwayCount;
    public Sprite AvatarSprite;
    public Sprite AvatarLockSprite;
    public Sprite AvatarBooster;
    public Sprite AvatarBoosterOverlay;
    public GameObject boosterPopOp;
    public string NameAnimIdle;
    public string NameAnimAction;
    public bool Loop;
    public bool IsIntroduced;
    public string tutorialText;
    public bool PreLevel;
}

public class BoosterController : MonoBehaviour
{
    [SerializeField] public BoosterItem[] boosterItems;
    //public BoosterItemData[] boostersData = new BoosterItemData[Utility.GetEnumLength<BoosterType>()];
    public BoosterItemData[] boostersData;
    public Transform dialogRoot;
    public int TotalBoostersUsedInLevel;
    public BoosterType TypeBoosterUsed;

    public static BoosterController Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        int i = 0;
        foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
        {
            boosterItems[i].SetUp(type);
            i++;
            if (i >= boosterItems.Length)
                break;
        }
        TypeBoosterUsed = BoosterType.Clock;
    }

    public void SetUp()
    {
        for (int i = 0; i < boosterItems.Length; i++)
        {
            boosterItems[i].SetUp((BoosterType)i);
        }
    }    

    private bool CheckUnlockBooster(int currentLevel)
    {
        for (int i = 0; i < boostersData.Length; i++)
        {
            if (currentLevel == boostersData[i].UnlockLevel)
            {
                return true;
            }
        }
        return false;
    }

}
