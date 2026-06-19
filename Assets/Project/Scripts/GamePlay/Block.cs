using UnityEngine;
using Common.Helper; 

public class Block : MonoBehaviour
{
    public int ColorID;

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
}