using UnityEngine;

public class AUVrotation : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform;


    public void LookAtXZ(Vector3 point)
    {
        var direction = (point - transform.position).normalized;
        direction.y = 0f;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void LookAtXZ(Vector3 point, float speed)
    {
        var direction = (point - transform.position).normalized;
        direction.y = 0f;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), speed);
    }


    // Update is called once per frame
    void Update()
    {
        LookAtXZ(_targetTransform.position, 1);
    }
}
