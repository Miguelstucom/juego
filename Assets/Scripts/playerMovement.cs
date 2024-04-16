using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.InputSystem;

 
public class PlayerMovement : MonoBehaviour
{
    //I recommend 7 for the move speed, and 1.2 for the force damping
    public Rigidbody2D rb;
    public float moveSpeed;
    public Vector2 forceToApply;
    public Vector2 PlayerInput;
    public float forceDamping;

    // public void Start(){
    //     rb = GetComponent<Rigidbody2D>();
    //     animator = GetComponent<Animator>();
    // }
    void Update()
    {
        PlayerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }
    void FixedUpdate()
    {
        Vector2 moveForce = PlayerInput * moveSpeed;
        moveForce += forceToApply;
        forceToApply /= forceDamping;
        if (Mathf.Abs(forceToApply.x) <= 0.01f && Mathf.Abs(forceToApply.y) <= 0.01f)
        {
            forceToApply = Vector2.zero;
        }
        rb.velocity = moveForce;
    }

    //  public void OnMove(InputValue value)
    // {
    //     moveInput = value.Get<Vector2>();
 
    //     // Only set the animation direction if the player is trying to move
    //     if(moveInput != Vector2.zero) {
    //         animator.SetFloat("XInput", moveInput.x);
    //         animator.SetFloat("YInput", moveInput.y);
    //     }
    // }
 
    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (collision.collider.CompareTag("Bullet"))
    //     {
    //         forceToApply += new Vector2(-20, 0);
    //         Destroy(collision.gameObject);
    //     }
    // }
}