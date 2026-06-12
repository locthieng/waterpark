using UnityEngine;

public class WallTile : MonoBehaviour
{
    public enum TileType
    {
        Normal,
        BlockEater
    }

    public TileType tileType = TileType.Normal;

    private void OnValidate()
    {
        UpdateVisual();
    }

    public void SetTileType(TileType type)
    {
        tileType = type;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // Cập nhật sprite hoặc màu tùy theo loại tile
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (tileType == TileType.BlockEater)
                sr.color = Color.red; // Tạm thời đổi màu để dễ debug
            else
                sr.color = Color.gray;
        }
    }
}
