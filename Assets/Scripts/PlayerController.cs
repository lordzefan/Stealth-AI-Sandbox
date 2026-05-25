using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private PlayerMovement movement;

    [Header("Speed")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sneakSpeed = 2.5f;

    private Vector2 moveInput;
    private bool isSneaking;

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        movement.Move(moveInput, GetCurrentSpeed());
    }

    private void ReadInput()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        isSneaking = Input.GetKey(KeyCode.LeftShift);
    }

    private float GetCurrentSpeed()
    {
        return isSneaking ? sneakSpeed : walkSpeed;
    }

    public bool IsSneaking()
    {
        return isSneaking;
    }
}
