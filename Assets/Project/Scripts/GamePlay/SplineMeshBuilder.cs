using UnityEngine;
using BezierSolution; // Khai báo namespace của BezierSpline

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplineMeshBuilder : MonoBehaviour
{
    [Header("Spline Settings")]
    public BezierSpline spline;

    [Header("Mesh Settings")]
    [Tooltip("Độ rộng của dải băng")]
    public float width = 1f;

    [Tooltip("Số lượng đốt (segments) chia dọc theo đường cong. Càng cao càng mượt.")]
    public int resolution = 50;

    [Tooltip("Độ giãn của texture dọc theo đường cong. Điều chỉnh để texture không bị bóp méo.")]
    public float uvTiling = 1f;

    [Tooltip("Góc xoay của dải lưới. Thay đổi góc Z (Roll) để xoay nghiêng hoặc xoay ngang dải băng.")]
    public Vector3 rotationOffset = new Vector3(0f, 0f, 0f);

    [Tooltip("Sử dụng vector normal của spline. Nếu tắt, dải băng sẽ luôn nằm ngang bám đất.")]
    public bool useSplineNormal = false;

    [Header("Material Settings")]
    [Tooltip("Kéo thả Material của bà vào ô này")]
    public Material meshMaterial;

    private Mesh generatedMesh;

    void Start()
    {
        GenerateMesh();
    }

    [ContextMenu("Generate Mesh Now")]
    public void GenerateMesh()
    {
        if (spline == null)
        {
            Debug.LogWarning("Chưa gán BezierSpline vào script!");
            return;
        }

        // Đảm bảo dữ liệu spline (vị trí, tiếp tuyến, pháp tuyến) được cập nhật mới nhất từ Scene View
        spline.Refresh();

        if (generatedMesh == null)
        {
            generatedMesh = new Mesh();
            generatedMesh.name = "Spline Mesh";
#if UNITY_EDITOR
            GetComponent<MeshFilter>().sharedMesh = generatedMesh;
#else
            GetComponent<MeshFilter>().mesh = generatedMesh;
#endif
        }

        // Gán Material tự động vào MeshRenderer
        if (meshMaterial != null)
        {
            GetComponent<MeshRenderer>().sharedMaterial = meshMaterial;
        }

        // 1. Khởi tạo mảng dữ liệu cho Mesh
        int vertexCount = (resolution + 1) * 2;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[resolution * 6];

        float currentDistance = 0f;
        Vector3 previousWorldPoint = spline.GetPoint(0f);

        // 2. Chạy dọc theo đường cong để tạo các đỉnh (Vertices) và UV
        for (int i = 0; i <= resolution; i++)
        {
            // t chạy từ 0 đến 1
            float t = (float)i / resolution;

            // Lấy dữ liệu từ Spline (World Space)
            Vector3 worldCenter = spline.GetPoint(t);
            Vector3 worldTangent = spline.GetTangent(t).normalized;

            Vector3 worldRight;
            if (useSplineNormal)
            {
                Vector3 worldNormal = spline.GetNormal(t).normalized;
                Quaternion segmentRotation = Quaternion.LookRotation(worldTangent, worldNormal) * Quaternion.Euler(rotationOffset);
                worldRight = segmentRotation * Vector3.right;
            }
            else
            {
                // Tự động tính toán vector nằm ngang (horizontal) vuông góc với hướng đi của Spline
                if (Mathf.Approximately(Mathf.Abs(worldTangent.y), 1f))
                {
                    // Nếu đường spline đi thẳng đứng lên/xuống, lấy hướng forward mặc định làm vector ngang
                    worldRight = Vector3.Cross(worldTangent, transform.forward).normalized;
                    if (worldRight.sqrMagnitude < 0.1f)
                        worldRight = Vector3.Cross(worldTangent, Vector3.forward).normalized;
                }
                else
                {
                    worldRight = Vector3.Cross(worldTangent, Vector3.up).normalized;
                }

                // Áp dụng góc xoay nghiêng (Roll) bằng rotationOffset.z quanh tangent
                // Cũng hỗ trợ xoay Pitch (X) và Yaw (Y) cho linh hoạt
                Quaternion offsetRotation = Quaternion.AngleAxis(rotationOffset.z, worldTangent) *
                                             Quaternion.AngleAxis(rotationOffset.x, worldRight) *
                                             Quaternion.AngleAxis(rotationOffset.y, Vector3.up);
                worldRight = offsetRotation * worldRight;
            }

            // Chuyển đổi về Local Space để Mesh đi theo GameObject này
            Vector3 localCenter = transform.InverseTransformPoint(worldCenter);
            Vector3 localRight = transform.InverseTransformDirection(worldRight);

            // Tạo 2 đỉnh ở 2 bên mép
            int vertIndex = i * 2;
            vertices[vertIndex] = localCenter - localRight * (width * 0.5f);     // Điểm bên Trái
            vertices[vertIndex + 1] = localCenter + localRight * (width * 0.5f); // Điểm bên Phải

            // Tính toán khoảng cách thực tế để UV không bị giãn méo
            if (i > 0)
            {
                currentDistance += Vector3.Distance(worldCenter, previousWorldPoint);
            }
            previousWorldPoint = worldCenter;

            // Trải UV (Trục Y cố định 0-1 cho 2 mép, Trục X chạy dài theo khoảng cách)
            float u = currentDistance / uvTiling;
            uvs[vertIndex] = new Vector2(u, 0f);
            uvs[vertIndex + 1] = new Vector2(u, 1f);
        }

        // 3. Nối các điểm lại thành các hình tam giác (Triangles)
        for (int i = 0; i < resolution; i++)
        {
            int vertIndex = i * 2;
            int triIndex = i * 6;

            // Tam giác 1
            triangles[triIndex] = vertIndex;
            triangles[triIndex + 1] = vertIndex + 2;
            triangles[triIndex + 2] = vertIndex + 1;

            // Tam giác 2
            triangles[triIndex + 3] = vertIndex + 1;
            triangles[triIndex + 4] = vertIndex + 2;
            triangles[triIndex + 5] = vertIndex + 3;
        }

        // 4. Gán dữ liệu vào Mesh
        generatedMesh.Clear();
        generatedMesh.vertices = vertices;
        generatedMesh.uv = uvs;
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateNormals();
    }
}