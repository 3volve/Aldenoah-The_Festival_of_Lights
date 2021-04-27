using System.Collections;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
    new Rigidbody rigidbody;
    new BoxCollider collider;

    Animator movingAnimator;
    Vector3 currentDirection;
    
    public bool Running { private set; get; } = false;
    float walkingSpeed;
    float runningSpeed;
    float accel;
    readonly float turningSpeed = 10;

    protected void InternalUpdate()
    {
        movingAnimator.SetFloat("Speed", rigidbody.velocity.magnitude);
        movingAnimator.SetFloat("Turning", Mathf.Clamp(Vector3.SignedAngle(transform.forward, currentDirection, Vector3.up), -15, 15) );
    }

    protected void Init(float walking, float running, float acceleration)
    {
        walkingSpeed = walking;
        runningSpeed = running;
        accel = acceleration;
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<BoxCollider>();
        movingAnimator = GetComponentsInChildren<Animator>()[0];
        currentDirection = transform.forward;
    }

    protected void Move(Vector3 direction, bool running)
    {
        float movementSpeed = (Running = running) ? runningSpeed : walkingSpeed;

        //If I want to here, I can check if walking, and performing a full 180, then perhaps run a specific turning animation?
        if (direction.magnitude > 0.01f) currentDirection = direction.normalized;
        direction = direction.normalized;

        transform.rotation = 
            Quaternion.RotateTowards(
                transform.rotation, 
                Quaternion.LookRotation(currentDirection, Vector3.up), 
                turningSpeed
            );
        
        rigidbody.velocity += direction * movementSpeed * accel;
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, movementSpeed);
    }
}
