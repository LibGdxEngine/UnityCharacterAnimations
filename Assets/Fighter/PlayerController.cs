using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Vector2 moveDirection;
    private float jumpDirection;
    private float jumpSpeed = 600f;
    public float moveSpeed = 2f;
    public float maxForwardSpeed = 10f;
    private float desiredSpeed;
    private float forwardSpeed;

    private const float groundAccel = 5f;
    private const float groundDecel = 25f;

    private Animator anim;
    private Rigidbody rb;
    private bool onGround = true;

    bool IsMoveInput
    {
        get { return !Mathf.Approximately(moveDirection.sqrMagnitude, 0f); }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumpDirection = context.ReadValue<float>();
    }

    private bool readyJump = false;
    private float jumpEffort = 0;

    void Jump(float direction)
    {
        if (direction > 0 && onGround)
        {
            anim.SetBool("ReadyToJump", true);
            readyJump = true;
            jumpEffort += Time.deltaTime;
            Debug.Log("Jump Effort " + jumpEffort);
        }
        else
        {
            if (readyJump)
            {
                anim.SetBool("Launch", true);
                readyJump = false;
                anim.SetBool("ReadyToJump", false);
            }
        }
    }

    public void Launch()
    {
        rb.AddForce(Vector3.up * jumpSpeed * Mathf.Clamp(jumpEffort, 1, 3), ForceMode.Impulse);
        anim.SetBool("Launch", false);
        anim.applyRootMotion = false;
    }

    public void Land()
    {
        anim.SetBool("Land", false);
        anim.applyRootMotion = true;
        anim.SetBool("Launch", false);
        jumpEffort = 0;
    }


    void Move(Vector2 direction)
    {
        float turnDirection = direction.x;
        float fDirection = direction.y;
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();
        desiredSpeed = direction.magnitude * maxForwardSpeed * Mathf.Sin(fDirection);
        float acceleration = IsMoveInput ? groundAccel : groundDecel;

        forwardSpeed = Mathf.MoveTowards(forwardSpeed, desiredSpeed, acceleration * moveSpeed * Time.deltaTime);
        anim.SetFloat("ForwardSpeed", forwardSpeed);
        // Debug.Log(turnDirection * turnSpeed * Time.deltaTime);
        Debug.Log(turnDirection);
        // Debug.Log( ".");
        var speed = Time.deltaTime * 100 * (turnDirection);
        this.transform.Rotate(0, speed, 0, Space.World);

        //transform.Translate(direction.x * moveSpeed * Time.deltaTime, 0, direction.y * moveSpeed * Time.deltaTime);
    }

    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody>();
    }

    private float groundRayDist = 1f;

    void Update()
    {
        Move(moveDirection);
        Jump(jumpDirection);
        RaycastHit hit;
        Ray ray = new Ray(transform.position + (Vector3.up * (groundRayDist * 0.5f)), -Vector3.up);
        if (Physics.Raycast(ray, out hit, groundRayDist))
        {
            if (!onGround)
            {
                onGround = true;
                anim.SetFloat("LandingVelocity", rb.velocity.magnitude);
                anim.SetBool("Land", true);
                anim.SetBool("Falling", false);
                
            }
        }
        else
        {
            onGround = false;
            anim.SetBool("Falling", true);
            anim.applyRootMotion = false;
            
        }

        Debug.DrawRay(transform.position + (Vector3.up * (groundRayDist * 0.5f)), -Vector3.up * groundRayDist,
            Color.red);
    }
}