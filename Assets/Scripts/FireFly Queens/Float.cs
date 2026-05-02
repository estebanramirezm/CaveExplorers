using UnityEngine;

public class FloatUpDown : MonoBehaviour
{
    public float amplitude = 0.2f;
    public float speed = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * speed) * amplitude, 0f);
    }
}