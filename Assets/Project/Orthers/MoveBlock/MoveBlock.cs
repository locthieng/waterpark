using UnityEngine;

public class MoveBlock : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask moveBlockMask;

    private Rigidbody rb;
    private Camera cam;
    private Vector3 targetPosition;
    private float yHeight;
    private bool isDragging = false;
    private Bounds combinedBounds;
    Collider[] colliders;
    BoardController board => BoardController.Instance;

    public int SizeX { get; private set; }
    public int SizeZ { get; private set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        cam = Camera.main;
        yHeight = transform.position.y;
        colliders = GetComponentsInChildren<Collider>();
        combinedBounds = GetCombinedBounds();

        CalculateSizes();
    }

    private void CalculateSizes()
    {
        float tileSize = BoardController.Instance.tileSize;
        Vector3 size = combinedBounds.size;

        SizeX = Mathf.RoundToInt(size.x / tileSize);
        SizeZ = Mathf.RoundToInt(size.z / tileSize);
    }


    void OnMouseDown()
    {
        SetCollidersLayer(LayerMaskToLayer(moveBlockMask));
    }

    void OnMouseUp()
    {
        isDragging = false;

        // Snap block vào lưới
        if (BoardController.Instance != null)
        {
            Vector3 snappedPos = BoardController.Instance.GetSnappedPosition(transform.position);
            snappedPos.y = yHeight;
            rb.MovePosition(snappedPos);
        }

        SetCollidersLayer(LayerMaskToLayer(obstacleMask));
        BoardController.Instance?.ClearShadows();
    }


    private void SetCollidersLayer(int layer)
    {
        foreach (var col in colliders)
        {
            col.gameObject.layer = layer;
        }
    }

    private int LayerMaskToLayer(LayerMask mask)
    {
        int layer = mask.value;
        for (int i = 0; i < 32; i++)
        {
            if ((layer & (1 << i)) != 0)
                return i;
        }
        return 0;
    }



    void OnMouseDrag()
    {
        Plane plane = new Plane(Vector3.up, Vector3.up * yHeight);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            targetPosition = ray.GetPoint(distance);
            targetPosition.y = yHeight;
        }
        isDragging = true;

    }

    private Bounds GetCombinedBounds()
    {
        Bounds bounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }
        return bounds;
    }


    private void FixedUpdate()
    {
        if (!isDragging) return;

        Vector3 current = rb.position;
        Vector3 desired = targetPosition;
        Vector3 direction = desired - current;
        direction.y = 0f;

        float moveStep = moveSpeed * Time.fixedDeltaTime;

        Vector3 nextPosition = current;

        float moveX = ComputeAxisMovement(current, Vector3.right * Mathf.Sign(direction.x), Mathf.Abs(direction.x), moveStep);
        nextPosition.x += moveX;

        float moveZ = ComputeAxisMovement(nextPosition, Vector3.forward * Mathf.Sign(direction.z), Mathf.Abs(direction.z), moveStep);
        nextPosition.z += moveZ;

        if (isDragging && board != null)
        {
            Vector3[] childPositions = new Vector3[colliders.Length];
            for (int i = 0; i < colliders.Length; i++)
            {
                Vector3 localOffset = colliders[i].transform.position - transform.position;
                childPositions[i] = nextPosition + localOffset;
            }

            board.ShowShadows(childPositions);
        }
        else if (board != null)
        {
            board.ClearShadows();
        }

        nextPosition.y = yHeight;
        rb.MovePosition(nextPosition);
    }

    private float ComputeAxisMovement(Vector3 virtualPos, Vector3 dir, float totalDistance, float step)
    {
        float move = Mathf.Min(totalDistance, step);
        if (move <= 0.001f) return 0f;

        Vector3 offset = virtualPos - transform.position;

        for (int i = 0; i < colliders.Length; i++)
        {
            Vector3 center = colliders[i].bounds.center + offset;
            Vector3 halfExtents = colliders[i].bounds.extents * 0.95f;

            if (Physics.BoxCast(center, halfExtents, dir, out RaycastHit hit, Quaternion.identity, move + 0.015f, obstacleMask))
            {
                move = Mathf.Min(move, Mathf.Max(0f, hit.distance - 0.015f));
            }
        }

        return dir.normalized.x * move + dir.normalized.z * move;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(combinedBounds.center, combinedBounds.size);
    }

}
