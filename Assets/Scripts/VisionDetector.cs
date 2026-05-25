using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionDetector : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float visionRange = 5f;

    [SerializeField]
    [Range(0f, 360f)]
    private float visionAngle = 90f;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private LayerMask obstacleLayer;

    private Transform player;

    private Vector2 lastSeenPosition;

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void OnDrawGizmosSelected()
{
    Gizmos.color = Color.red;

    Gizmos.DrawWireSphere(
        transform.position,
        visionRange
    );

    Vector3 leftBoundary =
        Quaternion.Euler(
            0,
            0,
            visionAngle * 0.5f
        ) * transform.right;

    Vector3 rightBoundary =
        Quaternion.Euler(
            0,
            0,
            -visionAngle * 0.5f
        ) * transform.right;

    Gizmos.DrawLine(
        transform.position,
        transform.position + leftBoundary * visionRange
    );

    Gizmos.DrawLine(
        transform.position,
        transform.position + rightBoundary * visionRange
    );
}

    public bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector2 directionToPlayer =
            (player.position - transform.position);

        float distanceToPlayer =
            directionToPlayer.magnitude;

        // STEP 1 — RANGE CHECK
        if (distanceToPlayer > visionRange)
            return false;

        // STEP 2 — ANGLE CHECK
        float angle =
            Vector2.Angle(
                transform.right,
                directionToPlayer
            );

        if (angle > visionAngle * 0.5f)
            return false;

        // STEP 3 — OBSTRUCTION CHECK
        RaycastHit2D hit =
            Physics2D.Raycast(
                transform.position,
                directionToPlayer.normalized,
                visionRange,
                obstacleLayer | playerLayer
            );

        if (hit.collider == null)
            return false;

        bool detected =
            hit.collider.CompareTag("Player");

        if (detected)
        {
            lastSeenPosition = player.position;
        }

        return detected;
    }

    public Vector2 GetLastSeenPosition()
    {
        return lastSeenPosition;
    }
}
