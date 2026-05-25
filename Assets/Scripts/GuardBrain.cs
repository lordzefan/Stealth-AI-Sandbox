using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardBrain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GuardMovement movement;
    [SerializeField] private PatrolPath patrolPath;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitTimeAtWaypoint = 1f;

    private int currentWaypointIndex;
    private float waitTimer;
    private bool isWaiting;

    private void Update()
    {
        HandlePatrol();
    }

    private void HandlePatrol()
    {
        if (patrolPath == null)
            return;

        Transform currentWaypoint =
            patrolPath.GetWaypoint(currentWaypointIndex);

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                GoToNextWaypoint();
            }

            return;
        }

        movement.MoveTo(
            currentWaypoint.position,
            patrolSpeed
        );

        if (movement.ReachedDestination(currentWaypoint.position))
        {
            movement.Stop();

            isWaiting = true;
            waitTimer = waitTimeAtWaypoint;
        }
    }

    private void GoToNextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= patrolPath.GetWaypointCount())
        {
            currentWaypointIndex = 0;
        }
    }
}
