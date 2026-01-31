using UnityEngine;

public class LigthMove : MonoBehaviour
{
    [Header("Drift Settings")]
    [SerializeField] private float amplitude = 0.1f;   
    [SerializeField] private float speed = 0.5f;      

    private Vector3 startPos;
    private float xSeed;
    private float ySeed;
    private float zSeed;

    private void Awake()
    {
        startPos = transform.localPosition;

        xSeed = Random.value * 100f;
        ySeed = Random.value * 100f;
        zSeed = Random.value * 100f;
    }

    private void Update()
    {
        float x = (Mathf.PerlinNoise(Time.time * speed, xSeed) - 0.5f) * amplitude;
        float y = (Mathf.PerlinNoise(Time.time * speed, ySeed) - 0.5f) * amplitude;
        float z = (Mathf.PerlinNoise(Time.time * speed, zSeed) - 0.5f) * amplitude;

        transform.localPosition = startPos + new Vector3(x, y, z);
    }
}
