using UnityEngine;

public class LigthMove : MonoBehaviour
{
    [Header("Drift Settings")]
    [SerializeField] private float amplitude = 5f;
    [SerializeField] private float speed = 0.5f;

    private Quaternion startRot;
    private float xSeed;
    private float ySeed;
    private float zSeed;

    private void Awake()
    {
        startRot = transform.localRotation;

        xSeed = Random.value * 100f;
        ySeed = Random.value * 100f;
        zSeed = Random.value * 100f;
    }

    private void Update()
    {
        float x = (Mathf.PerlinNoise(Time.time * speed, xSeed) - 0.5f) * amplitude;
        float y = (Mathf.PerlinNoise(Time.time * speed, ySeed) - 0.5f) * amplitude;
        float z = (Mathf.PerlinNoise(Time.time * speed, zSeed) - 0.5f) * amplitude;

        Quaternion driftRot = Quaternion.Euler(x, y, z);
        transform.localRotation = startRot * driftRot;
    }
}
