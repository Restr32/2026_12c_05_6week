using UnityEngine;

[CreateAssetMenu(fileName = "Moving Platform Config", menuName = "Scriptable Objects/Platform")]

public class MovingPlatform : MonoBehaviour
{
    public float distance;
    public float duration;
    private Vector3 startPos;
    private Vector3 endPos;
    private float travelTime;
    private bool movingRight = true;

    void Start()
    {
        startPos = transform.position;
        endPos = new Vector3(startPos.x + distance, startPos.y, startPos.z);
    }

    void Update()
    {
        travelTime += Time.deltaTime;

        if (movingRight)
        {
            transform.position = Vector3.Lerp(startPos, endPos, travelTime / duration);

            if (travelTime >= duration)
            {
                travelTime = 0f;
                movingRight = false;
            }
        }
        else
        {
            transform.position = Vector3.Lerp(endPos, startPos, travelTime / duration);

            if (travelTime >= duration)
            {
                travelTime = 0f;
                movingRight = true;
            }
        }
    }
}
