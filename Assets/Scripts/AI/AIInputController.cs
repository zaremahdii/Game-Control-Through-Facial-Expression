using UnityEngine;

public class AIInputController : MonoBehaviour
{
    [SerializeField] private AIWebSocketClient webSocketClient;
    [SerializeField] private Paddle paddle;
    [SerializeField] private float maxControlAgeSeconds = 0.5f;

    private void Awake()
    {
        if (webSocketClient == null)
            webSocketClient = FindObjectOfType<AIWebSocketClient>();

        if (paddle == null)
            paddle = FindObjectOfType<Paddle>();
    }

    private void Update()
    {
        if (paddle == null)
            return;

        AIControlResult result = webSocketClient != null ? webSocketClient.LatestResult : null;
        bool isFresh = result != null
            && webSocketClient.IsConnected
            && webSocketClient.LastReceivedTime >= 0f
            && Time.unscaledTime - webSocketClient.LastReceivedTime <= maxControlAgeSeconds;

        paddle.SetAIInput(isFresh ? ToHorizontalInput(result.direction) : 0f);
    }

    private void OnDisable()
    {
        if (paddle != null)
            paddle.ClearAIInput();
    }

    private static float ToHorizontalInput(string direction)
    {
        if (direction == "left")
            return -1f;
        if (direction == "right")
            return 1f;
        return 0f;
    }
}
