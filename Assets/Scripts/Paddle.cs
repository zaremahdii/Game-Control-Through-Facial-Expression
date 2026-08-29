using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Paddle : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 direction;
    private float aiHorizontalInput;
    private bool hasAiInput;

    public float speed = 30f;
    public float maxBounceAngle = 75f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        ResetPaddle();
    }

    public void ResetPaddle()
    {
        rb.velocity = Vector2.zero;
        transform.position = new Vector2(0f, transform.position.y);
    }

    public void SetAIInput(float horizontalInput)
    {
        aiHorizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);
        hasAiInput = true;
    }

    public void ClearAIInput()
    {
        aiHorizontalInput = 0f;
        hasAiInput = false;
    }

    private void Update()
    {
        if (hasAiInput)
        {
            direction = Vector2.right * aiHorizontalInput;
            return;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            direction = Vector2.left;
        } else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            direction = Vector2.right;
        } else {
            direction = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (direction != Vector2.zero) {
            rb.AddForce(direction * speed);
        } else {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ball")) {
            return;
        }

        Rigidbody2D ball = collision.rigidbody;
        Collider2D paddle = collision.otherCollider;

        // Gather information about the collision
        Vector2 ballDirection = ball.velocity.normalized;
        Vector2 contactDistance = paddle.bounds.center - ball.transform.position;

        // Rotate the direction of the ball based on the contact distance
        // to make the gameplay more dynamic and interesting
        float bounceAngle = (contactDistance.x / paddle.bounds.size.x) * maxBounceAngle;
        ballDirection = Quaternion.AngleAxis(bounceAngle, Vector3.forward) * ballDirection;

        // Re-apply the new direction to the ball
        ball.velocity = ballDirection * ball.velocity.magnitude;
    }

}
