using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FieldOfView : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewRadius = 8f;
    [Range(0, 360)]
    public float viewAngle = 90f;

    [Header("Detection")]
    public LayerMask targetMask;      // Layer "Player"
    public LayerMask obstacleMask;    // Layer "Wall", "Obstacle"
    public float detectionDelay = 0.1f;

    [Header("Mesh")]
    public int rayCount = 50;         // Jumlah ray (kualitas mesh)
    public float meshResolution = 0.5f;
    public int edgeResolveIterations = 4;
    public float edgeDstThreshold = 0.5f;
    public MeshFilter viewMeshFilter;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    private Mesh viewMesh;

    void Start()
    {
        viewMesh = new Mesh { name = "View Mesh" };
        viewMeshFilter.mesh = viewMesh;
        StartCoroutine(FindTargetsWithDelay(detectionDelay));
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void LateUpdate()
    {
        DrawFieldOfView();
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider2D[] targets = Physics2D.OverlapCircleAll(
            transform.position, viewRadius, targetMask);

        foreach (Collider2D col in targets)
        {
            Transform target = col.transform;
            Vector2 dirToTarget = (target.position - transform.position).normalized;
            float angle = Vector2.Angle(transform.up, dirToTarget); // pakai transform.up (hadap atas)

            if (angle < viewAngle / 2f)
            {
                float distToTarget = Vector2.Distance(transform.position, target.position);
                bool blocked = Physics2D.Raycast(
                    transform.position, dirToTarget, distToTarget, obstacleMask);

                if (!blocked)
                    visibleTargets.Add(target);
            }
        }
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = default;

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.z - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded =
                    Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;

                if (oldViewCast.hit != newViewCast.hit ||
                   (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero) viewPoints.Add(edge.pointA);
                    if (edge.pointB != Vector3.zero) viewPoints.Add(edge.pointB);
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        // Build mesh
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero; // pivot lokal guard
        for (int i = 0; i < vertexCount - 1; i++)
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

        for (int i = 0; i < vertexCount - 2; i++)
        {
            triangles[i * 3]     = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, obstacleMask);

        if (hit)
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        else
            return new ViewCastInfo(false,
                (Vector2)transform.position + (Vector2)dir * viewRadius,
                viewRadius, globalAngle);
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2f;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded =
                Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;

            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.z;
        // Unity 2D: 0 derajat = atas (transform.up)
        return new Vector3(
            Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),
            Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        { hit=_hit; point=_point; dst=_dst; angle=_angle; }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA, pointB;
        public EdgeInfo(Vector3 a, Vector3 b) { pointA=a; pointB=b; }
    }
}