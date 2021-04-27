using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode;

public class CameraController : MonoBehaviour
{
    public Transform playerTransform;
    public float followingSpeed = 0.05f;

    public bool CameraControlPaused = false;

    readonly static float zDeltaPosition = 2.5f;
    readonly static int castDelay = 10;
    readonly static int distanceBehindCameraToStart = 10;

    int castDelayCounter = 0;

    Vector3 newPosition = Vector3.zero;
    Vector3 testingPosition = Vector3.zero;
    Vector3 testingDirection = Vector3.zero;
    float testingDistance;

    struct RendererAndMode
    {
        public MeshRenderer renderer;
        public ShadowCastingMode mode;
    }

    Dictionary<string, RendererAndMode> intersectedBuildings = new Dictionary<string, RendererAndMode>();
    Dictionary<string, RendererAndMode> testingIntersections = new Dictionary<string, RendererAndMode>();

    void Start()
    {
        newPosition = new Vector3(
            playerTransform.position.x,
            transform.position.y,
            playerTransform.position.z - zDeltaPosition
        );

        transform.position = newPosition;
        testingPosition = transform.TransformPoint(Vector3.back * distanceBehindCameraToStart);

        testingDirection = playerTransform.position - testingPosition;
        testingDistance = Vector3.Distance(playerTransform.position, testingPosition);
    }

    void FixedUpdate()
    {
        if (CameraControlPaused) return;

        newPosition = new Vector3(
            Mathf.Lerp(transform.position.x, playerTransform.position.x, followingSpeed),
            transform.position.y,
            Mathf.Lerp(transform.position.z, playerTransform.position.z - zDeltaPosition, followingSpeed)
        );

        transform.position = newPosition;
        testingPosition = transform.TransformPoint(Vector3.back * distanceBehindCameraToStart);

        if (castDelayCounter++ < castDelay) return;
        else castDelayCounter = 0;
        
        string hitTarget = "";
        Vector3 castPosition = testingPosition;
        MeshRenderer currentRenderer;
        
        while (hitTarget != "Player")
        {
            if (Physics.BoxCast(
                castPosition,
                new Vector3(0.1f, 0.1f, 0.1f),
                testingDirection,
                out RaycastHit hit,
                transform.rotation,
                testingDistance,
                LayerMask.GetMask("BuildingsBlocking", "Player")))
            {
                hitTarget = hit.collider.tag;
                if (hitTarget == "Player" || hitTarget == "Ground" || Vector3.Distance(hit.point, playerTransform.position) <= 2) break;

                currentRenderer = hit.collider.GetComponentInParent<MeshRenderer>() ??
                                  hit.collider.GetComponent<MeshRenderer>();
                castPosition = hit.point;
                
                MeshRenderer[] children = currentRenderer.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer renderer in children)
                    AddNewRendererAndMode(renderer);
            }
            else break;
        }
        
        if (testingIntersections.Keys.GetHashCode() != intersectedBuildings.Keys.GetHashCode())
        {
            foreach (string name in intersectedBuildings.Keys)
            {
                if (intersectedBuildings.TryGetValue(name, out RendererAndMode value))
                {
                    if (testingIntersections.ContainsKey(name))
                    {
                        testingIntersections.Remove(name);
                        testingIntersections.Add(name, value);
                    }
                    else value.renderer.shadowCastingMode = value.mode;
                }
            }

            intersectedBuildings.Clear();

            if (testingIntersections.Count != 0)
            {
                intersectedBuildings = testingIntersections;
                testingIntersections = new Dictionary<string, RendererAndMode>();
            }
        }

        testingIntersections.Clear();
    }

    void AddNewRendererAndMode(MeshRenderer currentRenderer)
    {
        if (testingIntersections.ContainsKey(currentRenderer.name)) return;

        RendererAndMode current = new RendererAndMode
        {
            renderer = currentRenderer,
            mode = currentRenderer.shadowCastingMode
        };

        currentRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        testingIntersections.Add(currentRenderer.name, current);
    }
}
