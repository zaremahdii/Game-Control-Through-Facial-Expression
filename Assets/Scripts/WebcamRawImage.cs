using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WebcamRawImage : MonoBehaviour
{
    [SerializeField] private RawImage cameraPreview;
    [SerializeField] private AIWebSocketClient webSocketClient;
    [SerializeField] private string deviceName = "";
    [SerializeField] private bool preferFrontFacingCamera = true;
    [SerializeField, Min(1)] private int requestedWidth = 1280;
    [SerializeField, Min(1)] private int requestedHeight = 720;
    [SerializeField, Range(1, 60)] private int requestedFrameRate = 30;
    [SerializeField] private bool mirrorPreview = true;

    private WebCamTexture webcamTexture;
    private RectTransform previewRect;
    private bool isStarting;
    private bool permissionDenied;
    private bool wasSocketConnected;
    private bool receivedFirstFrame;

    private void Start()
    {
        if (cameraPreview == null)
        {
            Debug.LogError("WebcamRawImage: Assign a RawImage to Camera Preview.", this);
            enabled = false;
            return;
        }

        previewRect = cameraPreview.rectTransform;
        if (webSocketClient == null)
            webSocketClient = FindObjectOfType<AIWebSocketClient>();
    }

    private void Update()
    {
        if (webSocketClient == null)
            webSocketClient = FindObjectOfType<AIWebSocketClient>();

        bool socketConnected = webSocketClient != null && webSocketClient.IsConnected;
        if (socketConnected && !wasSocketConnected)
            Debug.Log("WebcamRawImage: WebSocket connected. Starting Unity camera preview.", this);

        if (!socketConnected && wasSocketConnected)
            Debug.Log("WebcamRawImage: WebSocket disconnected. Stopping Unity camera preview.", this);

        wasSocketConnected = socketConnected;
        if (!socketConnected)
        {
            StopCamera();
            return;
        }

        if (webcamTexture == null && !isStarting && !permissionDenied)
            StartCoroutine(StartCamera());

        if (webcamTexture == null || !webcamTexture.isPlaying)
            return;

        if (webcamTexture.didUpdateThisFrame && !receivedFirstFrame)
        {
            receivedFirstFrame = true;
            Debug.Log($"WebcamRawImage: First frame received at {webcamTexture.width}x{webcamTexture.height}.", this);
        }

        previewRect.localEulerAngles = new Vector3(0f, 0f, -webcamTexture.videoRotationAngle);
        float yScale = webcamTexture.videoVerticallyMirrored ? -1f : 1f;
        float xScale = mirrorPreview ? -1f : 1f;
        previewRect.localScale = new Vector3(xScale, yScale, 1f);
    }

    private IEnumerator StartCamera()
    {
        isStarting = true;
        Debug.Log("WebcamRawImage: Requesting camera permission.", this);
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            permissionDenied = true;
            isStarting = false;
            Debug.LogError("WebcamRawImage: Camera permission was not granted.", this);
            yield break;
        }

        if (webSocketClient == null || !webSocketClient.IsConnected)
        {
            isStarting = false;
            yield break;
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("WebcamRawImage: No camera device was found.", this);
            isStarting = false;
            yield break;
        }

        WebCamDevice selectedDevice = FindDevice(devices);
        Debug.Log($"WebcamRawImage: Selected camera '{selectedDevice.name}'.", this);
        webcamTexture = new WebCamTexture(
            selectedDevice.name,
            requestedWidth,
            requestedHeight,
            requestedFrameRate);

        cameraPreview.texture = webcamTexture;
        webcamTexture.Play();
        Debug.Log("WebcamRawImage: Camera preview started. Waiting for first frame.", this);
        isStarting = false;
    }

    private WebCamDevice FindDevice(WebCamDevice[] devices)
    {
        if (!string.IsNullOrWhiteSpace(deviceName))
        {
            foreach (WebCamDevice device in devices)
            {
                if (device.name == deviceName)
                    return device;
            }

            Debug.LogWarning($"WebcamRawImage: Camera '{deviceName}' was not found; using an available camera.", this);
        }

        foreach (WebCamDevice device in devices)
        {
            if (device.isFrontFacing == preferFrontFacingCamera)
                return device;
        }

        return devices[0];
    }

    private void OnDisable()
    {
        StopCamera();
    }

    private void StopCamera()
    {
        if (webcamTexture == null)
            return;

        Debug.Log("WebcamRawImage: Camera preview stopped.", this);

        if (webcamTexture.isPlaying)
            webcamTexture.Stop();

        if (cameraPreview != null && cameraPreview.texture == webcamTexture)
            cameraPreview.texture = null;

        webcamTexture = null;
        receivedFirstFrame = false;
    }
}
