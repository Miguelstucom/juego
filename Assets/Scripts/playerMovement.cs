using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// using UnityEngine.InputSystem;

 
public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed;
    public Vector2 forceToApply;
    public Vector2 PlayerInput;
    public float forceDamping;
    private float originalScaleX;
    private float originalScaleY;
    public addScore scoreAdder;


    public void Start(){
         //rb = GetComponent<Rigidbody2D>();
         //animator = GetComponent<Animator>();
        originalScaleX = Mathf.Abs(transform.localScale.x);
        originalScaleY = transform.localScale.y;
     }
    void Update()
    {
        PlayerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (PlayerInput.x > 0)
        {
            transform.localScale = new Vector3(originalScaleX, originalScaleY, 1);
        }
        else if (PlayerInput.x < 0)
        {
            transform.localScale = new Vector3(-originalScaleX, originalScaleY, 1);
        }

        if (Input.GetKeyDown(KeyCode.E)){
            ScoreScript.scoreValue += 10;
            SaveScore(ScoreScript.scoreValue);
        }

        if (Input.GetKeyDown(KeyCode.R)){
            int scorevalue = PlayerPrefs.GetInt("PlayerScore");
            string username = PlayerPrefs.GetString("username", "defaultUsername");
            scoreAdder.Login(username, scorevalue);
        }
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

        public void SaveScore(int score)
    {
        PlayerPrefs.SetInt("PlayerScore", score);
        PlayerPrefs.Save();
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