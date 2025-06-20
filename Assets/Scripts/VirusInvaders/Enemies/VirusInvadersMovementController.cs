using UnityEngine;

public class VirusInvadersMovementController : MonoBehaviour
{
    private IVirusInvadersMovement movementComponent;
    private Transform target;
    
    public void Initialize(IVirusInvadersMovement movement, Transform playerTarget)
    {
        movementComponent = movement;
        target = playerTarget;
    }
    
    void Update()
    {
        if (movementComponent != null)
        {
            movementComponent.UpdateMovement(target);
        }
    }
} 