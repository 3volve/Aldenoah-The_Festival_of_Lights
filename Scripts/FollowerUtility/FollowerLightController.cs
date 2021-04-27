using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FollowerLightController : MonoBehaviour
{
    [SerializeField] Transform followTarget;
    [SerializeField] Vector3 primaryPosition;

    [SerializeField] float verticalDelta;
    [SerializeField] float verticalSpeed;
    [SerializeField] float movementLag;
    [SerializeField] float movementMin;
    [SerializeField] float horizontalMax;
    [SerializeField] float horizontalWander;
    [SerializeField] Material active;
    [SerializeField] Material inactive;
    [SerializeField] bool isActive = false;

    [SerializeField] float burstDuration;
    [SerializeField] float burstHeight;
    [SerializeField] int burstRevolutions = 4;
    [SerializeField] float revolutionAmplitude = 0.5f;

    int horizontalDirection = 1;
    float sinCounter = 0;

    bool triggerBurst = false;
    Vector3 burstStartPos;
    float burstStartTime;

    HDAdditionalLightData myLight;
    MeshRenderer meshRenderer;
    ParticleSystem particles;

    void Awake()
    {
        myLight = GetComponent<HDAdditionalLightData>();
        meshRenderer = GetComponent<MeshRenderer>();
        particles = GetComponent<ParticleSystem>();

        ParticleSystem.MainModule main = particles.main;
        main.startColor = active.color;
        main.duration = burstDuration;

        ParticleSystem.Burst burst = particles.emission.GetBurst(0);
        burst.time = burstDuration - 0.05f;

        ParticleSystemRenderer particleRenderer = GetComponent<ParticleSystemRenderer>();
        particleRenderer.material = active;
        
        //to ensure that whatever the light is default set to, it will be correct after starting.
        ActiveLight(!isActive);
        ActiveLight(!isActive);
    }

    void Update()
    {
        Vector3 newPosition = transform.position;

        if (triggerBurst)
        {
            float percentage = Mathf.Clamp01((Time.time - burstStartTime) / burstDuration);
            
            newPosition.y = Mathf.Lerp(burstStartPos.y, burstHeight, percentage);
            newPosition.x = burstStartPos.x + revolutionAmplitude * Mathf.Sin(2 * Mathf.PI * percentage * burstRevolutions);
            newPosition.z = burstStartPos.z + revolutionAmplitude * Mathf.Cos(2 * Mathf.PI * percentage * burstRevolutions) - revolutionAmplitude;

            if (percentage >= 1) triggerBurst = false;
        }
        else
        { 
            primaryPosition.x = Mathf.Clamp(primaryPosition.x + Random.Range(0, horizontalWander * horizontalDirection), -horizontalMax, horizontalMax);
            Vector3 targetPosition = followTarget.TransformPoint(primaryPosition);
            sinCounter += verticalSpeed;

            newPosition.y = primaryPosition.y + (Mathf.Sin(sinCounter) * verticalDelta);

            float newVelocity = movementMin;
            if (followTarget.TryGetComponent(out Rigidbody rigidbody))
                newVelocity = Mathf.Max(newVelocity, rigidbody.velocity.magnitude * movementLag * 0.1f);

            newPosition.x = Mathf.Lerp(transform.position.x, targetPosition.x, newVelocity);
            newPosition.z = Mathf.Lerp(transform.position.z, targetPosition.z, newVelocity);


            if (Mathf.Abs(primaryPosition.x) == horizontalMax) horizontalDirection *= -1;
        }
        
        transform.position = newPosition;
    }

    public void ActiveLight(bool activate)
    {
        if (isActive == activate) return;
        else isActive = activate;

        meshRenderer.material = isActive ? active : inactive;
        myLight.intensity = isActive ? 1 : 0.1f;
    }

    public void BurstLight()
    {
        ActiveLight(true);

        triggerBurst = true;
        burstStartTime = Time.time;
        burstStartPos = transform.position;

        particles.Play();
    }

    public void ReplaceTarget(Transform newTarget) => followTarget = newTarget;
}
