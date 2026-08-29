using UnityEngine;
using UnityEngine.UI;

public class AIStreamPreview : MonoBehaviour
{
    [SerializeField] private AIWebSocketClient webSocketClient;
    [SerializeField] private RawImage targetImage;

    private Texture2D previewTexture;

    private void Awake()
    {
        if (webSocketClient == null)
            webSocketClient = FindObjectOfType<AIWebSocketClient>();

        if (targetImage == null)
            targetImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        if (webSocketClient == null || targetImage == null)
            return;

        if (!webSocketClient.TryGetLatestPreview(out byte[] previewBytes))
            return;

        if (previewTexture == null)
            previewTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);

        if (ImageConversion.LoadImage(previewTexture, previewBytes, false))
            targetImage.texture = previewTexture;
    }

    private void OnDestroy()
    {
        if (previewTexture != null)
            Destroy(previewTexture);
    }
}
