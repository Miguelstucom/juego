using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
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
    public Health healthComponent; // Referencia al componente Health
    private bool isAlive = true;
    private static bool hasBeenTriggered = false;


    public void Start()
    {
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

        if (Input.GetKeyDown(KeyCode.E))
        {
            ScoreScript.scoreValue += 10;
            SaveScore(ScoreScript.scoreValue);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            int scorevalue = PlayerPrefs.GetInt("PlayerScore");
            string username = PlayerPrefs.GetString("username", "defaultUsername");
            scoreAdder.Login(username, scorevalue);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animations.animator.SetTrigger("attack");
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            animations.animator.ResetTrigger("attack"); // Restablecer el trigger cuando se suelta la tecla
        }
        if (healthComponent.health <= 0 && isAlive)
        {
            isAlive = false;
            animations.animator.SetBool("isDead", true); 
            StartCoroutine(RestartSceneAfterDelay(3f)); // Restart scene after 3 seconds
        }
    }

    private IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    void FixedUpdate()
    {
        if (!isAlive)
        {
            rb.velocity = Vector2.zero; // Detiene el movimiento del jugador si está muerto
            return;
        }

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


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("slime"))
        {
            if (healthComponent != null)
            {
                healthComponent.DecreaseHealth();
                Debug.Log("Health after collision: " + healthComponent.health);
            }
        }
        if (other.gameObject.CompareTag("end") && !hasBeenTriggered)
        {
            hasBeenTriggered = true; // Marcar como ya activado

            Debug.Log("partida acabada");
            int scorevalue = PlayerPrefs.GetInt("PlayerScore");
            string username = PlayerPrefs.GetString("username", "defaultUsername");
            scoreAdder.Login(username, scorevalue);
            SceneManager.LoadScene(2);
        }

        if (other.gameObject.CompareTag("Palanca")) {
                        ScoreScript.scoreValue += 72;
            SaveScore(ScoreScript.scoreValue);
    }
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