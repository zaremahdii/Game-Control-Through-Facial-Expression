using UnityEngine;
using UnityEngine.UI;

public class EmotionSpriteController : MonoBehaviour
{
    [SerializeField] private AIWebSocketClient webSocketClient;
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite happinessSprite;
    [SerializeField] private Sprite sadnessSprite;
    [SerializeField] private Sprite neutralSprite;

    private string currentEmotion;

    private void Awake()
    {
        if (webSocketClient == null)
            webSocketClient = FindObjectOfType<AIWebSocketClient>();
    }

    private void Update()
    {
        if (webSocketClient == null || targetImage == null)
            return;

        AIControlResult result = webSocketClient.LatestResult;
        string emotion = result != null ? result.emotion : "neutral";
        if (emotion == currentEmotion)
            return;

        currentEmotion = emotion;
        targetImage.sprite = GetSprite(emotion);
    }

    private Sprite GetSprite(string emotion)
    {
        if (emotion == "happiness")
            return happinessSprite;
        if (emotion == "sadness")
            return sadnessSprite;
        return neutralSprite;
    }
}
