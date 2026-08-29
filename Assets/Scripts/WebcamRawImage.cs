using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WebcamRawImage : MonoBehaviour
{
    [SerializeField] private RawImage cameraPreview;
    [SerializeField] private string deviceName = "";
    [SerializeField] private bool preferFrontFacingCamera = true;
    [SerializeField, Min(1)] private int requestedWidth = 1280;
    [SerializeField, Min(1)] private int requestedHeight = 720;
    [SerializeField, Range(1, 60)] private int requestedFrameRate = 30;
    [SerializeField] private bool mirrorPreview = true;

    private WebCamTexture webcamTexture;
    private RectTransform previewRect;

    private IEnumerator Start()
    {
        if (cameraPreview == null)
        {
            Debug.LogError("WebcamRawImage: Assign a RawImage to Camera Preview.", this);
            yield break;
        }

        previewRect = cameraPreview.rectTransform;

        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("WebcamRawImage: Camera permission was not granted.", this);
            yield break;
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("WebcamRawImage: No camera device was found.", this);
            yield break;
        }

        WebCamDevice selectedDevice = FindDevice(devices);
        webcamTexture = new WebCamTexture(
            selectedDevice.name,
            requestedWidth,
            requestedHeight,
            requestedFrameRate);

        cameraPreview.texture = webcamTexture;
        webcamTexture.Play();
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

    private void Update()
    {
        if (webcamTexture == null || !webcamTexture.isPlaying)
            return;

        previewRect.localEulerAngles = new Vector3(0f, 0f, -webcamTexture.videoRotationAngle);
        float yScale = webcamTexture.videoVerticallyMirrored ? -1f : 1f;
        float xScale = mirrorPreview ? -1f : 1f;
        previewRect.localScale = new Vector3(xScale, yScale, 1f);
    }

    private void OnDisable()
    {
        if (webcamTexture == null)
            return;

        if (webcamTexture.isPlaying)
            webcamTexture.Stop();

        if (cameraPreview != null && cameraPreview.texture == webcamTexture)
            cameraPreview.texture = null;

        webcamTexture = null;
    }
}
