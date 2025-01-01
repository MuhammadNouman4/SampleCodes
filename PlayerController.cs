using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Jump and Slide Settings")]
    public float slideSpeed = -2f; // Speed for sliding down walls
    public float ShortJumpThreshold = 0.1f;
    [Header("Wall Detection")]
    public float raycastDistance = 0.5f; // Distance to detect walls
    public LayerMask wallLayer; // Layer to detect walls


    private Rigidbody2D rb;
    public bool isTouchingWall = false; // Check if player is on a wall
    private bool isSliding = false; // Check if the player is sliding
    private bool isOnLeftWall = true; // Start on the left wall

    [Header("Jump Settings")]
    public float jumpForce = 6f; // Force for jumping

    public float maxJumpForce = 12f; // Max possible force (before clamping)
    public float minJumpForce = 6f; // Minimum force for a quick tap
    private float maxHoldTime = 0.2f; // Time needed to charge to max force
    public float holdTime = 0f; // Time the jump button is held



    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    private void Update()
    {
        HandleJump();
        DetectWall();
        UImanager.instance.TapHold.text = holdTime.ToString();
    }
    private void FixedUpdate()
    {
        if (isSliding)
        {
            SlideDownWall();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Stick to the wall
            isTouchingWall = true;
            isSliding = true;
            rb.velocity = Vector3.zero;
             SoundManager.Instance.PlayCollideSound();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = false;
            isSliding = false;

        }
    }


    private void DetectWall()
    {
        // Cast a ray to the left or right to detect walls
        Vector2 direction = isOnLeftWall ? Vector2.left : Vector2.right;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, raycastDistance, wallLayer);

        if (hit.collider != null)
        {
            // Wall detected
            if (!isTouchingWall)
            {
                isTouchingWall = true;
                isSliding = true;

                Vector3 wallPosition = hit.point;
                transform.position = new Vector3(wallPosition.x, transform.position.y, transform.position.z);
                rb.velocity = Vector2.zero; // Stop any movement
            }
        }

    }



    private void OnDrawGizmos()
    {
        // Visualize raycast in the Scene view
        Gizmos.color = Color.red;
        Vector2 direction = isOnLeftWall ? Vector2.left : Vector2.right;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(direction * raycastDistance));



    }


    private void HandleJump()
    {
        if (isTouchingWall && Input.GetMouseButton(0)) // Holding the jump button
        {
            holdTime += Time.deltaTime;
        }

        if (Input.GetMouseButtonUp(0) && isTouchingWall) // Releasing the jump button
        {

            SoundManager.Instance.PlayJumpSound();

            // Calculate the jump force based on the hold time, capped at the max force
            float appliedJumpForce = Mathf.Clamp(holdTime / maxHoldTime * maxJumpForce, minJumpForce, maxJumpForce);

            if (holdTime < ShortJumpThreshold) // Short tap threshold
            {
                // Debug.Log($"Short Tap Detected! Speed: {appliedJumpForce}");
            }
            else if (holdTime > ShortJumpThreshold) // Long tap
            {
                //  Debug.Log($"Long Tap Detected! Speed: {appliedJumpForce}");
            }
            // Set the jump direction based on the wall side
            Vector2 jumpDirection = isOnLeftWall ? new Vector2(1, 1) : new Vector2(-1, 1);

            // Apply the jump force
            rb.velocity = jumpDirection * appliedJumpForce;

            // Update rotation and wall side
            isOnLeftWall = !isOnLeftWall;
            isSliding = false;



            // Smoothly rotate the player based on the wall side
            StartCoroutine(SmoothRotation(isOnLeftWall));
            /*  //Rotate
              if (isOnLeftWall)
              {
                  // Rotate the player to face right (flip horizontally)
                  transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
              }
              else
              {
                  // Rotate the player to face left (flip horizontally)
                  transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
              }*/
            //Rotat end
            // Reset hold time
            holdTime = 0f;
        }
    }

    // Coroutine to smoothly rotate the player
    private IEnumerator SmoothRotation(bool isJumpingToLeftWall)
    {
        float startRotation = transform.eulerAngles.z;
        float endRotation = isJumpingToLeftWall ? 0f : -180f; // Corrected rotation logic
        float rotationDuration = 0.3f; // Adjust for how fast the rotation should happen
        float timeElapsed = 0f;

        while (timeElapsed < rotationDuration)
        {
            // Smoothly interpolate the rotation
            float newRotation = Mathf.LerpAngle(startRotation, endRotation, timeElapsed / rotationDuration);
            transform.eulerAngles = new Vector3(0, 0, -newRotation);

            timeElapsed += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        // Ensure the final rotation is set to avoid any minor floating-point inaccuracies
        transform.eulerAngles = new Vector3(0, 0, -endRotation);
    }


    private void SlideDownWall()
    {
        // Apply constant sliding motion
        rb.velocity = new Vector2(0, slideSpeed);
    }

}
