using UnityEngine;

public class elevator : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 20f;
    private Vector3 nextPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextPosition = pointB.position;

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, nextPosition, speed * Time.deltaTime);
        if (transform.position == nextPosition)
        {
            nextPosition = (nextPosition == pointA.position) ? pointB.position : pointA.position;
        }
    }
}
