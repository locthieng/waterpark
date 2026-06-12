using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TileGenerator
{
    public static List<Vector2> GenerateSquareTilePos2D(Vector2Int tileFloorDimension, float tileSize, float tileGap)
    {
        List<Vector2> listTilePos = new List<Vector2>();
        Vector2 floorSize = new Vector2(
            tileFloorDimension.x > 1 ? (tileSize + tileGap) * tileFloorDimension.x - tileSize : 0,
            tileFloorDimension.y > 1 ? (tileSize + tileGap) * tileFloorDimension.y - tileSize : 0);
        Vector2 gap = new Vector2(
            tileFloorDimension.x > 1 ? floorSize.x / (tileFloorDimension.x - 1) : 0,
            tileFloorDimension.y > 1 ? floorSize.y / (tileFloorDimension.y - 1) : 0);
        for (int y = 0; y < tileFloorDimension.y; y++)
        {
            for (int x = 0; x < tileFloorDimension.x; x++)
            {
                listTilePos.Add(new Vector3(floorSize.x / 2 - x * gap.x, y * gap.y - floorSize.y / 2));
            }
        }
        return listTilePos;
    }

    public static List<Vector3> GenerateSquareTilePos(Vector3Int tileFloorDimension, float tileSize, float tileGap)
    {
        List<Vector3> listTilePos = new List<Vector3>();
        float floorSizeZ = (tileSize + tileGap) * tileFloorDimension.z - tileSize;
        float gap = floorSizeZ / (tileFloorDimension.z - 1);
        for (int i = 0; i < tileFloorDimension.z; i++)
        {
            List<Vector2> listPos = GenerateSquareTilePos2D(new Vector2Int(tileFloorDimension.x, tileFloorDimension.y), tileSize, tileGap);
            for (int j = 0; j < listPos.Count; j++)
            {
                listTilePos.Add(new Vector3(listPos[j].x, listPos[j].y, i * gap - floorSizeZ / 2));
            }
        }
        return listTilePos;
    }

    public static List<Vector2> GenerateHexTilePos(Vector2 tileFloorDimension, float tileInnerRadius, float tileOuterRadius, float tileGap)
    {
        List<Vector2> listTilePos = new List<Vector2>();
        Vector2 floorSize = new Vector2((tileFloorDimension.x + tileFloorDimension.y * 0.5f - (int)tileFloorDimension.x / 2) * (tileInnerRadius + tileGap) * 2f - tileInnerRadius,
            tileFloorDimension.y * (tileOuterRadius + tileGap) * 1.5f - tileOuterRadius);
        for (int x = 0; x < tileFloorDimension.x; x++)
        {
            for (int y = 0; y < tileFloorDimension.y; y++)
            {
                listTilePos.Add(new Vector3((x + y * 0.5f - y / 2) * (tileInnerRadius + tileGap) * 2f - floorSize.x / 2, y * (tileOuterRadius + tileGap) * 1.5f - floorSize.y / 2));
            }
        }
        return listTilePos;
    }
}
