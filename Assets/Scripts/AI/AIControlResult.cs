using System;

[Serializable]
public class AIControlResult
{
    public string direction;
    public string emotion;
    public float roll;
    public float yaw;
    public float pitch;
    public float emotionConfidence;
    public bool faceDetected;
    public double timestamp;
}
