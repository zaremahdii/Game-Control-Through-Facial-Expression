# Game Control Through Facial Expression

This Unity project receives AI control data from a local FastAPI server. Unity does not capture or upload camera frames for this AI pipeline.

## Prerequisite

The local AI server must be running and this WebSocket endpoint must be available:

```text
ws://127.0.0.1:8000/ws
```

The AI server setup guide is located at `E:\facial expression\Game-Control-Through-Facial-Expression-AI\README.md`.

## WebSocket client setup

1. Open the project with Unity `2022.3.62f3`.
2. Create an empty GameObject in the scene, for example `AI Client`.
3. Add the `AIWebSocketClient` component.
4. Add the `AIInputController` component to the same GameObject.
5. Assign the scene Paddle to the `Paddle` field, or leave it empty for automatic lookup.
6. Set `Server Url` to `ws://127.0.0.1:8000/ws`.
7. Play the scene.

The client reconnects every two seconds after a disconnection. Received messages are displayed in the Unity Console.

## Client files

```text
Assets/Scripts/AI/AIControlResult.cs
Assets/Scripts/AI/AIWebSocketClient.cs
Assets/Scripts/AI/AIInputController.cs
```

`AIWebSocketClient` only receives data. `AIInputController` maps fresh `left`, `right`, and `neutral` values to the Paddle. A missing, disconnected, or stale message stops the paddle.

## Message format

```json
{
  "direction": "left",
  "emotion": "neutral"
}
```

The current test server sends `direction` and `emotion`. The C# model also includes optional fields for later expansion.
