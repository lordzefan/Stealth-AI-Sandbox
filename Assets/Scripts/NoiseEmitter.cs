using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseEmitter : MonoBehaviour
{
    [SerializeField] private float walkNoiseRadius = 3f;
    [SerializeField] private float sneakNoiseRadius = 1f;

    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public float GetCurrentNoiseRadius()
    {
        return playerController.IsSneaking()
            ? sneakNoiseRadius
            : walkNoiseRadius;
    }
}
