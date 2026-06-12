using UnityEngine;
using System.Collections.Generic;

public class BlockEater : MonoBehaviour
{
    public enum Direction
    {
        Horizontal, 
        Vertical
    }

    [Header("Configs")]
    public List<WallTile> tiles = new List<WallTile>();
    public Direction lookDirection;

    private float tileSize;

    private void Start()
    {
        tileSize = BoardController.Instance.tileSize;
    }

    public int Width
    {
        get
        {
            if (tiles == null || tiles.Count == 0)
                return 0;

            Bounds bounds = new Bounds(tiles[0].transform.position, Vector3.zero);
            for (int i = 1; i < tiles.Count; i++)
                bounds.Encapsulate(tiles[i].transform.position);

            return Mathf.RoundToInt(
                (lookDirection == Direction.Horizontal ? bounds.size.x : bounds.size.z) / tileSize
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        MoveBlock block = other.GetComponentInParent<MoveBlock>();
        if (block != null)
        {
            Vector3 eaterCenter = transform.position;
            Vector3 blockCenter = block.transform.position;

            float distAlongApproachAxis = (lookDirection == Direction.Horizontal)
                ? Mathf.Abs(eaterCenter.x - blockCenter.x)
                : Mathf.Abs(eaterCenter.z - blockCenter.z);

            if (distAlongApproachAxis <= tileSize * 0.5f)
            {
                int blockSize = (lookDirection == Direction.Horizontal) ? block.SizeX : block.SizeZ;

                if (blockSize <= Width)
                {
                    Debug.Log($"🟣 BlockEater ({lookDirection}) nuốt block có size {blockSize}");
                    // TODO: thêm hiệu ứng nuốt, animation, destroy...
                }
                else
                {
                    Debug.Log($"🔴 Block quá to để bị nuốt! ({blockSize} > {Width})");
                }
            }
        }
    }
}
