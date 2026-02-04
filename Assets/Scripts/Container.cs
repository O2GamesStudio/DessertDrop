using UnityEngine;

public class Container : MonoBehaviour
{
    private const float REFERENCE_WIDTH = 1080f;
    private const float REFERENCE_HEIGHT = 1920f;
    private const float BASE_SCALE = 1.15f;

    private void Awake()
    {
        AdjustScale();
    }

    private void AdjustScale()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float referenceAspect = REFERENCE_WIDTH / REFERENCE_HEIGHT;
        float screenAspect = screenWidth / screenHeight;

        float aspectRatio = screenAspect / referenceAspect;
        float finalScale = BASE_SCALE * aspectRatio;

        transform.localScale = Vector3.one * finalScale;

        Debug.Log($"Screen: {screenWidth}x{screenHeight}, Aspect: {screenAspect:F3}, AspectRatio: {aspectRatio:F3}, Final Scale: {finalScale:F3}");
    }
}