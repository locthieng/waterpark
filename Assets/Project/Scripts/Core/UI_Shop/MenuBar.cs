using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBar : MonoBehaviour
{
    [SerializeField] ButtonBar[] buttons;
    [SerializeField] GameObject[] pages;
    [SerializeField] Transform selectIcon;
    [SerializeField] int defaultButtonIndex;
    [SerializeField] Transform pageTransform;
    [SerializeField] ConsumableShopUI shopUI;
    private int currentActiveButtonIndex = -1;

    private void Start()
    {
        SetUp();
    }

    public void SetUp()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetUp();
        }
        UpdateButton(defaultButtonIndex);
    }

    private void UpdateButton(int index)
    {
        
        if (buttons[index].IsLock)
        {
            buttons[index].ShowLog("Coming soon!");
            return;
        }
        buttons[index].OnSelect();
        LeanTween.cancel(selectIcon.gameObject);
        LeanTween.moveLocalX(selectIcon.gameObject, buttons[index].transform.localPosition.x, 0.3f).setEase(LeanTweenType.easeOutQuad);
        if (currentActiveButtonIndex > -1)
        {
            buttons[currentActiveButtonIndex].OnDeselect();
        }
        currentActiveButtonIndex = index;
        float target = pageTransform.position.x - buttons[index].targetPos.position.x;
        pageTransform.gameObject.LeanCancel();
        pageTransform.LeanMoveX(0 + target, 0.3f).setEase(LeanTweenType.easeOutQuad);
        if (pages != null && pages.Length > 0)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                {
                    pages[i].SetActive(i != index);
                }
            }
        }


        //Log event
        if (GlobalController.CurrentStage != StageScreen.InGame)
        {
            if (index == 0)
            {
                GlobalController.CurrentStage = StageScreen.Shop;
                shopUI.LogIAPShowEvent();
            }
            else if (index == 1)
            {
                GlobalController.CurrentStage = StageScreen.Home;
            }
        }
    }

    public void OnButtonSelect(int index)
    {
        if (currentActiveButtonIndex == index) return;
        UpdateButton(index);
    }
}
