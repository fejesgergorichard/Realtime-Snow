using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public GameObject Target;
    public float CameraDistance = 10f;
    public float CameraHeight = 3f;
    public float FollowSlerpSpeed = 1f;

    // Use this for initialization
    void Start()
    {

    }

    // LateUpdate is called after Update each frame
    void FixedUpdate()
    {
        Vector3 newPosition = Target.transform.position - Target.transform.forward * CameraDistance;
        transform.position = Vector3.Slerp(transform.position, newPosition, Time.deltaTime * FollowSlerpSpeed);

        newPosition = new Vector3(transform.position.x, transform.position.y + CameraHeight, transform.position.z);
        transform.position = newPosition;
        transform.LookAt(Target.transform.position);
    }
}