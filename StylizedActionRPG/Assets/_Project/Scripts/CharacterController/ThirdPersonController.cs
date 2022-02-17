using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonController : MonoBehaviour
{
    // Input Fields
    private ThirdPersonActions playerActionsAsset;
    private InputAction move;
    
    // Movement Fields
    private Rigidbody rb;
    private Animator anim;

    [SerializeField] private float
        movementForce = 1f,
        jumpForce = 5f,
        maxSpeed = 5f;

    private Vector3 forceDirection = Vector3.zero;

    [SerializeField] private Camera playerCamera;

    private bool canMove;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        anim = this.GetComponent<Animator>();
        playerActionsAsset = new ThirdPersonActions();

        canMove = true;
    }

    private void OnEnable()
    {
        playerActionsAsset.Player.Jump.started += DoJump;
        playerActionsAsset.Player.Attack.started += DoAttack;
        move = playerActionsAsset.Player.Move;
        playerActionsAsset.Player.Enable();
    }

    private void OnDisable()
    {
        playerActionsAsset.Player.Jump.started -= DoJump;
        playerActionsAsset.Player.Attack.started -= DoAttack;
        playerActionsAsset.Player.Disable();
    }

    private void FixedUpdate()
    {
        CheckMovement();

        if (!canMove)
        {
            rb.velocity = Vector3.zero;
        }
        else
        {
            forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera) * movementForce;
            forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera) * movementForce;
        
            rb.AddForce(forceDirection, ForceMode.Impulse);
            forceDirection = Vector3.zero;

            if (rb.velocity.y < 0f)
                rb.velocity -= Vector3.down * Physics.gravity.y * Time.deltaTime;

            Vector3 horizontalVelocity = rb.velocity;
            horizontalVelocity.y = 0f;
            if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
                rb.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;
        
            LookAt();   
        }
    }

    private void CheckMovement()
    {
        AnimatorStateInfo animInfo = anim.GetCurrentAnimatorStateInfo(1);
        if (animInfo.IsName("attack"))
            canMove = false;
        else
            canMove = true;
    }

    private void LookAt()
    {
        Vector3 direction = rb.velocity;
        direction.y = 0f;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            this.rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        else
           rb.angularVelocity = Vector3.zero; 
    }

    private Vector3 GetCameraForward(Camera playerCamera)
    {
        Vector3 forward = this.playerCamera.transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        Vector3 right = this.playerCamera.transform.right;
        right.y = 0f;
        return right.normalized;
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (isGrounded())
        {
            forceDirection += Vector3.up * jumpForce;
        }
    }

    private bool isGrounded()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up * .25f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.3f))
            return true;
        else
            return false;
    }
    
    private void DoAttack(InputAction.CallbackContext obj)
    {
        anim.SetTrigger("attack");
        
    }
}
