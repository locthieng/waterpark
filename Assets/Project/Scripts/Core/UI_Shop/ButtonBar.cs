using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ButtonBarType
{
    Home,
    Shop,
    Rank
}

public class ButtonBar : MonoBehaviour
{
    [SerializeField] ButtonBarType buttonType;
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] CanvasGroup dialogCanvas;
    [SerializeField] TextMeshProUGUI txtDialog;
    [SerializeField] GameObject nameObject;
    public Transform targetPos;
    public bool IsLock;

    public void SetUp()
    {
        icon.transform.localPosition = Vector3.zero;
        label.transform.localScale = Vector3.zero;
    }

    public void ShowLog(string mess)
    {
        if (txtDialog == null || dialogCanvas == null) return;
        dialogCanvas.gameObject.LeanCancel();
        dialogCanvas.alpha = 0;
        txtDialog.text = mess;
        dialogCanvas.transform.localPosition = Vector3.zero;
        dialogCanvas.transform.localScale = Vector3.zero;
        dialogCanvas.gameObject.LeanScale(Vector3.one, 0.2f);
        dialogCanvas.gameObject.LeanMoveLocalY(150f, 0.2f);
        dialogCanvas.LeanAlpha(1, 0.2f);
        dialogCanvas.LeanAlpha(0, 2f).setDelay(1);
    }

    public void OnSelect()
    {
        icon.gameObject.LeanCancel();
        label.gameObject.LeanCancel();
        icon.gameObject.LeanMoveLocalY(50, 0.15f);
        icon.gameObject.LeanScale(Vector3.one * 1.3f, 0.15f);
        label.gameObject.LeanScale(Vector3.one, 0.15f);
        //label.gameObject.LeanMoveLocalY(-100f, 0.1f);
        if (buttonType == ButtonBarType.Shop)
        {
            //GlobalSDKController.Instance.LogIAPShow(DataController.Instance.Data.LevelIndex, "home_shop", "shop", "all_shop");
        }
    }

    public void OnDeselect()
    {
        icon.gameObject.LeanCancel();
        label.gameObject.LeanCancel();
        icon.gameObject.LeanMoveLocalY(0, 0.15f);
        icon.gameObject.LeanScale(Vector3.one, 0.15f);
        label.gameObject.LeanScale(Vector3.zero, 0.15f);
        //label.gameObject.LeanMoveLocalY(-124f, 0.15f);
        //label.gameObject.LeanScale(Vector3.zero, 0.15f);
    }
}
