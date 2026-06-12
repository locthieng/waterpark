using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PageUI : MonoBehaviour
{
    [SerializeField] RectTransform[] pages;
    [SerializeField] RectTransform pageContainer;
    [SerializeField] int defautPage;

    public static PageUI Instance { get; set; }

    private void Start()
    {
        Instance = this;
        //SetUp();
    }

    /*public void SetUp()
    {
        *//*Vector2 size = GetComponent<RectTransform>().rect.size;
        Vector2 pageSize = GameUIController.IsUIMatchWidth ? size : new Vector2(Mathf.Ceil(Screen.width * (1920f / Screen.height)), 1920f - ((1080f / Screen.width) * Screen.height - size.y));
        float containerSizeX = pageSize.x * pages.Length;
        float startPageX = -containerSizeX / 2;
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].sizeDelta = pageSize;
            pages[i].transform.localPosition = new Vector2(startPageX + (pageSize.x * i + pageSize.x / 2), 0);
        }
        pageContainer.sizeDelta = new Vector2(containerSizeX, pageSize.y);
        pageContainer.transform.localPosition = new Vector3(-pages[defautPage].localPosition.x, pageContainer.localPosition.y, pageContainer.localPosition.z);*//*
    }*/

    public void SetUp()
    {
        Vector2 size = GetComponent<RectTransform>().rect.size;

        Vector2 pageSize = GameUIController.IsUIMatchWidth ? size : new Vector2(Mathf.Ceil(Screen.width * (1920f / Screen.height)), 1920f - ((1080f / Screen.width) * Screen.height - size.y));

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].sizeDelta = new Vector2(pages[i].sizeDelta.x, pageSize.y);
                Debug.Log(pages[i].name + " = " + pages[i].sizeDelta);
            }
        }

        if (pageContainer != null)
        {
            pageContainer.sizeDelta = new Vector2(pageContainer.sizeDelta.x, pageSize.y);
        }
    }
}
