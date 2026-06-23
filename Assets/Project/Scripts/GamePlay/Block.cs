using UnityEngine;
using Common.Helper;
using System;
using System.Collections;

public class Block : MonoBehaviour
{
    public int ColorID;

    public int ColumnIndex;

    public BlockCell OwnerCell { get; private set; }

    [SerializeField] private Renderer _renderer;

    private Hole ParentHole;

    public void Init(int colorID, BlockCell ownerCell)
    {
        ColorID = colorID;
        OwnerCell = ownerCell;

        ApplyColorFromDatabase();
    }

    private void ApplyColorFromDatabase()
    {
        if (_renderer == null) return;

        if (InGameController.Instance != null && InGameController.Instance._GameDataBase != null)
        {
            var colorList = InGameController.Instance._GameDataBase.Colors;

            if (ColorID >= 0 && ColorID < colorList.Count)
            {
                Color targetColor = colorList[ColorID];

                _renderer.SetBaseColor(targetColor);
            }
            else
            {
                Debug.LogWarning($"ColorID {ColorID} không có trong GameDataBase kìa!");
            }
        }
        else
        {
            Debug.LogWarning("Chưa có InGameController hoặc GameDataBase chưa được gán!");
        }
    }

    public void MoveAlongCurve(Transform target, Action callback)
    {
        if (target == null)
        {
            callback?.Invoke();
            Destroy(gameObject);
            return;
        }

        transform.SetParent(null);
        Vector3 startPoint = transform.position;
        
        float curveHeight = 6.0f;
        float moveDuration = 0.3f;

        LeanTween.value(gameObject, 0f, 1f, moveDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float t) =>
            {
                if (target != null)
                {
                    Vector3 currentTargetPos = target.position;

                    Vector3 controlPoint1 = startPoint + (Vector3.up * curveHeight);
                    Vector3 controlPoint2 = currentTargetPos + (Vector3.up * curveHeight);

                    // Cubic Bezier interpolation:
                    float u = 1f - t;
                    float uu = u * u;
                    float uuu = uu * u;
                    float tt = t * t;
                    float ttt = tt * t;

                    Vector3 positionOnCurve = uuu * startPoint +
                                              3f * uu * t * controlPoint1 +
                                              3f * u * tt * controlPoint2 +
                                              ttt * currentTargetPos;

                    transform.position = positionOnCurve;
                }
            })
            .setOnComplete(() =>
            {
                if (target != null)
                {
                    transform.position = target.position;
                }
                callback?.Invoke();
                Destroy(gameObject);
            });
    }
}