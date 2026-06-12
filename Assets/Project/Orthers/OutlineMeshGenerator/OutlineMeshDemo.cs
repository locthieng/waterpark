using System.Collections.Generic;
using UnityEngine;
using static OutlineMeshGenerator;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Demo dùng OutlineMeshGenerator với mảng Transform làm input points.
/// Generate realtime trong Edit Mode, không context menu.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OutlineMeshDemo : MonoBehaviour
{
    [Header("Path Points (theo thứ tự vòng quanh)")]
    public Transform[] points;

    [Header("Outline Settings")]
    public OutlinePlane planeAxis;
    [Range(0.01f, 1f)] public float thickness = 0.15f;
    [Range(0.01f, 2f)] public float height = 0.5f;
    public bool closed = true;

    [Header("Corner Smoothing")]
    [Range(0f, 1f)] public float cornerRadius = 0.1f;
    [Range(0, 16)] public int cornerSegments = 4;
    [Header("Edge Smoothing")]
    [Range(0f, 1f)] public float edgeBluntRate = 0f;

    [Header("Materials (Top / Bottom / Side)")]
    public Material topMaterial;
    public Material bottomMaterial;
    public Material sideMaterial;

    private MeshFilter mf;
    private MeshRenderer mr;

    // cache để detect thay đổi transform
    private Vector3[] lastPositions;

    void OnEnable()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        GenerateMesh();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GenerateMesh();
        }
#endif
    }

    public void GenerateMesh()
    {
        if (points == null || points.Length < 2) return;

        if (mf == null) mf = GetComponent<MeshFilter>();
        if (mr == null) mr = GetComponent<MeshRenderer>();
        if (mf.sharedMesh == null) mf.sharedMesh = new Mesh { name = "OutlineMesh" };

        List<Vector3> pts = new List<Vector3>();
        lastPositions = new Vector3[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null) continue;
            Vector3 localPos = transform.InverseTransformPoint(points[i].position);
            pts.Add(localPos);
            lastPositions[i] = points[i].position;
        }

        if (pts.Count < 2) return;

        Mesh mesh = Build(
            pts, thickness, height, closed, cornerRadius, cornerSegments, edgeBluntRate, planeAxis
        );
        mf.sharedMesh = mesh;

        if (mr != null)
        {
            mr.sharedMaterials = new Material[3]
            {
                topMaterial ?? sideMaterial,
                sideMaterial,
                bottomMaterial ?? sideMaterial,
            };
        }
    }

    void OnDrawGizmos()
    {
        if (points == null || points.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null) continue;
            Vector3 a = points[i].position;
            Vector3 b = points[(i + 1) % points.Length].position;
            if (!closed && i == points.Length - 1) break;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawSphere(a, 0.05f);
        }
    }
}
