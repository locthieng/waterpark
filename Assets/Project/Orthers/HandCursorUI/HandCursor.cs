using UnityEngine;
using UnityEngine.UI;

public class HandCursor : MonoBehaviour
{
    [Header("Hand Settings")]
    public RectTransform handIcon;
    public float scaleNormal = 1f;
    public float scalePressed = 0.85f;
    public float tweenTime = 0.1f;

    private Canvas canvas;

    void Start()
    {
        if (handIcon == null) handIcon = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        handIcon.localScale = Vector3.one * scaleNormal;

        // ?n con tr? chu?t m?c ??nh n?u mu?n
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out pos
        );
        handIcon.localPosition = pos;

        if (Input.GetMouseButtonDown(0))
        {
            LeanTween.scale(handIcon, Vector3.one * scalePressed, tweenTime)
                     .setEaseOutQuad();
        }

        if (Input.GetMouseButtonUp(0))
        {
            LeanTween.scale(handIcon, Vector3.one * scaleNormal, tweenTime)
                     .setEaseOutBack();
        }
    }
}
