using UnityEngine;

public class PlayerCoinPreviewAnimation : MonoBehaviour
{
    public float rotationSpeed = 150f; 
    public float bounceAmplitude = 0.3f; 
    public float bounceFrequency = 2.5f; 
    public float wobbleIntensity = 15f; 

    private bool hasValidPosition; 
    private Vector3 basePosition; 
    

    void Update()
    {
        if (CheckPositionValid())
        {
            AnimateCoin(basePosition);
        }
    }

    private bool CheckPositionValid()
    {
        hasValidPosition = transform.position != Vector3.zero;

        if (!hasValidPosition)
        {
            transform.position = new Vector3(0, 1, 0);
            basePosition = transform.position;
            hasValidPosition = true; 
        }
        else
        {
            basePosition = transform.position;
        }

        return hasValidPosition;
    }

    private void AnimateCoin(Vector3 basePosition)
    {
        float bounceOffset = Mathf.Sin(Time.time * bounceFrequency) * bounceAmplitude;
        Vector3 animatedPosition = basePosition + new Vector3(0, bounceOffset, 0);
        
        transform.position = animatedPosition;
        
        transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime), Space.Self);
        
        float wobble = Mathf.Sin(Time.time * bounceFrequency * 0.5f) * wobbleIntensity;
        transform.rotation *= Quaternion.Euler(wobble * Time.deltaTime, 0, wobble * Time.deltaTime);
    }
}