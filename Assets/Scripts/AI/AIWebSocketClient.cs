using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AIWebSocketClient : MonoBehaviour
{
    [SerializeField] private string serverUrl = "ws://127.0.0.1:8000/ws";
    [SerializeField] private float reconnectDelaySeconds = 2f;
    [SerializeField] private bool logReceivedData = true;

    private readonly object resultLock = new object();
    private ClientWebSocket socket;
    private CancellationTokenSource cancellation;
    private AIControlResult latestResult;
    private byte[] latestPreviewBytes;
    private bool hasNewResult;
    private bool hasNewPreview;
    private string pendingError;

    public AIControlResult LatestResult
    {
        get
        {
            lock (resultLock)
            {
                return latestResult;
            }
        }
    }

    public bool IsConnected { get; private set; }
    public float LastReceivedTime { get; private set; } = -1f;

    public bool TryGetLatestPreview(out byte[] previewBytes)
    {
        lock (resultLock)
        {
            if (!hasNewPreview)
            {
                previewBytes = null;
                return false;
            }

            previewBytes = latestPreviewBytes;
            hasNewPreview = false;
            return true;
        }
    }

    private void OnEnable()
    {
        cancellation = new CancellationTokenSource();
        _ = ConnectLoopAsync(cancellation.Token);
    }

    private void OnDisable()
    {
        if (cancellation != null)
        {
            cancellation.Cancel();
            cancellation.Dispose();
            cancellation = null;
        }

        if (socket != null)
        {
            socket.Abort();
            socket.Dispose();
            socket = null;
        }

        IsConnected = false;
    }

    private async Task ConnectLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using (ClientWebSocket newSocket = new ClientWebSocket())
                {
                    socket = newSocket;
                    await newSocket.ConnectAsync(new Uri(serverUrl), token);
                    IsConnected = true;
                    await ReceiveLoopAsync(newSocket, token);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                lock (resultLock)
                {
                    pendingError = exception.Message;
                }
            }
            finally
            {
                IsConnected = false;
                socket = null;
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(reconnectDelaySeconds), token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket connectedSocket, CancellationToken token)
    {
        byte[] buffer = new byte[4096];
        while (connectedSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
        {
            using (MemoryStream message = new MemoryStream())
            {
                WebSocketReceiveResult receiveResult;
                do
                {
                    receiveResult = await connectedSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await connectedSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        return;
                    }

                    message.Write(buffer, 0, receiveResult.Count);
                }
                while (!receiveResult.EndOfMessage);

                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    string json = Encoding.UTF8.GetString(message.ToArray());
                    AIControlResult result = JsonUtility.FromJson<AIControlResult>(json);
                    if (result == null)
                        continue;

                    lock (resultLock)
                    {
                        latestResult = result;
                        hasNewResult = true;
                    }
                }
                else if (receiveResult.MessageType == WebSocketMessageType.Binary)
                {
                    lock (resultLock)
                    {
                        latestPreviewBytes = message.ToArray();
                        hasNewPreview = true;
                    }
                }
            }
        }
    }

    private void Update()
    {
        AIControlResult result = null;
        string error = null;
        lock (resultLock)
        {
            if (hasNewResult)
            {
                result = latestResult;
                hasNewResult = false;
            }

            if (!string.IsNullOrEmpty(pendingError))
            {
                error = pendingError;
                pendingError = null;
            }
        }

        if (!string.IsNullOrEmpty(error))
            Debug.LogWarning($"AI WebSocket: {error}", this);

        if (logReceivedData && result != null)
        {
            LastReceivedTime = Time.unscaledTime;
            Debug.Log($"AI direction: {result.direction}, emotion: {result.emotion}", this);
        }
        else if (result != null)
        {
            LastReceivedTime = Time.unscaledTime;
        }
    }
}
