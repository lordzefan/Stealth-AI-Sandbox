using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PatrolPath : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints;

    public Transform GetWaypoint(int index)
    {
        return waypoints[index];
    }

    public int GetWaypointCount()
    {
        return waypoints.Count;
    }
}
