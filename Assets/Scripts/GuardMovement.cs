using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GuardMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void MoveTo(Vector2 targetPosition, float speed)
    {
        Vector2 direction =
            (targetPosition - rb.position).normalized;

        rb.velocity = direction * speed;
    }

    public void Stop()
    {
        rb.velocity = Vector2.zero;
    }

    public bool ReachedDestination(Vector2 target, float threshold = 0.1f)
    {
        return Vector2.Distance(rb.position, target) <= threshold;
    }
}      