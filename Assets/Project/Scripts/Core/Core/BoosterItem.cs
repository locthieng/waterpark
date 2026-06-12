using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;
//using Spine.Unity;

public class BoosterItem : MonoBehaviour
{
    [SerializeField] public BoosterType boosterType;
    [SerializeField] private CanvasGroup boosterButton;
    [Header("Lock Cover")]
    [SerializeField] private GameObject lockCover;
    [SerializeField] private Image avatarLock;
    [SerializeField] private TextMeshProUGUI levelUnlock;
    [Header("Unlock")]
    [SerializeField] private GameObject unlockCover;
    [SerializeField] private Image avatar;
    [SerializeField] public Image trace;
    [SerializeField] private Image amountIcon;
    [SerializeField] private TextMeshProUGUI amountTxt;
    //[SerializeField] private SkeletonDataAsset skeletonDataAsset;
    //[SerializeField] private SpineAnimController animBooster;
    //[SerializeField] private SkeletonGraphic skeletonGraphic;

    public bool isTutorialBooster;
    private BoosterItemData data;
    public bool tutorialItem = false;

    private bool IsLock
    {
        get
        {
            //return false;
            if (BoosterController.Instance == null) return true;
            if (SceneManager.GetActiveScene().name == "LevelEditor")
            {
                return false;
            }
            return BoosterController.Instance.boostersData[(int)boosterType].UnlockLevel > DataController.Instance.Data.LevelIndex;
            //return data.UnlockLevel > GlobalController.CurrentLevelIndex - 1;
        }
    }

    private int Amount
    {
        get
        {
            if (BoosterDataController.Instance == null)
            {
                return 0;
            }
            return BoosterDataController.Instance.Data.GetCount(boosterType) > 99 ? 99 : BoosterDataController.Instance.Data.GetCount(boosterType);
        }

    }

    public void SetUp(BoosterType type)
    {
        boosterType = type;
        
        for (int i = 0; i < BoosterController.Instance.boostersData.Length; i++)
        {
            var boosterData = BoosterController.Instance.boostersData[i];
            if (boosterData.Type == type)
            {
                data = boosterData;
                break;
            }
        }
        Refresh();
    }
        
    public void Refresh(bool tutorialItem = false)
    {
        if (tutorialItem)
        {
            this.tutorialItem = true;
            //avatar.sprite = data.AvatarSprite;
            unlockCover.gameObject.SetActive(true);
            lockCover.SetActive(false);
            //txtUnlock.gameObject.SetActive(false);
            amountIcon.gameObject.SetActive(true);
            if (BoosterDataController.Instance.Data.IsIntroduced(boosterType))
            {
                amountTxt.text = (BoosterDataController.Instance.Data.GetCount(boosterType) + 1).ToString();
            }
            else
            {
                amountTxt.text = (BoosterDataController.Instance.Data.GetCount(boosterType) + 2).ToString();
                BoosterDataController.Instance.Data.AddBooster(boosterType);
                BoosterDataController.Instance.Data.SetIntroduced(boosterType);
            }
            return;
        }
        avatar.sprite = data.AvatarSprite;
        avatar.SetNativeSize();
        avatarLock.sprite = data.AvatarLockSprite;
        //unlockCover.gameObject.SetActive(!IsLock);
        lockCover.SetActive(IsLock);
        levelUnlock.gameObject.SetActive(IsLock);
        levelUnlock.text = "Lv." + data.UnlockLevel;
        //animBooster.gameObject.SetActive(!IsLock);
        //if (data.dataAnim != null)
        //{
        //    skeletonGraphic.skeletonDataAsset = data.dataAnim;
        //    skeletonGraphic.Initialize(true);
        //} 
        //else
        //{
        //    animBooster.gameObject.SetActive(false);
        //    avatar.enabled = true;
        //    avatar.transform.LeanScale(Vector3.one * 0.9f, 0f);
        //}    
        amountIcon.gameObject.SetActive(Amount > 0);
        amountTxt.text = Amount.ToString();
        trace.gameObject.SetActive(false);
    }


    public void TapOnBoosterButton()
    {
        LevelController level = LevelController.Instance;
        GameUIController.Instance.SetInteractBoosterUI(false);
        if (tutorialItem)
        {
            UseBooster(level);
            gameObject.SetActive(false);
            return;
        }
        if (IsLock)
        {
            GameUIController.Instance.SetInteractBoosterUI(true);
            return;
        }
        if (BoosterDataController.Instance.Data.GetCount(boosterType) > 0)
        {
            UseBooster(level);
            BoosterDataController.Instance.Data.RemoveBooster(boosterType);
            Refresh();
        }
        else
        {
            //Show InLevelBoosterOverlay
        }
    }

    public void RefreshBooster()
    {
        GameUIController.Instance.SetInteractBoosterUI(true);
    }

    public void UseBooster(LevelController level)
    {
        switch (boosterType)
        {
            case BoosterType.Clock:
                {
                    break;
                }
            case BoosterType.Hammer:
                {
                    break;
                }

            case BoosterType.Saw:
                {
                    break;
                }
        }
    }
}
