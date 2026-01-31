using System;
using Unity.VisualScripting;
using UnityEngine;

public class Playerscript : MonoBehaviour
{
    public Rigidbody2D playerRb;

    Animator playerAnimator;
    float horizontalInput;
    float moveSpeed = 10f;
    bool isFacingRight = true;
    float jumpPower = 15f;
    bool isGrounded = false;
    // used when the console is open
    public bool inputEnabled = true;

    // respawn position
    private Vector3 spawnPoint;
    private bool isDead = false;


    /// updating the player's spawn point 
    public void SetSpawnPoint(Vector3 newSpawn)
    {
        spawnPoint = newSpawn;
        Debug.Log("Playerscript: Spawn point set to " + spawnPoint);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        // initial position as spawn point of player so they like consistently respawn there???
        spawnPoint = transform.position;
        playerAnimator = GetComponent<Animator>();
    }



    public void Die()
    {
        if (!isDead)
        {
            isDead = true;
            // Disable input while dead
            inputEnabled = false;
            // Reset velocity
            playerRb.linearVelocity = Vector2.zero;
            // Respawn the player
            Respawn();
        }
    }

    private void Respawn()
    {
        // Return to spawn point
        transform.position = spawnPoint;
        isDead = false;
        inputEnabled = true;
        isGrounded = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (inputEnabled)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            FlipSprite();
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocityX, jumpPower);
                isGrounded = false;
                playerAnimator.SetBool("IsJumping", !isGrounded);
            }
        }
        else
        {
            // so that the player cant move
            horizontalInput = 0f;
        }
    }

    private void FixedUpdate()
    {
        playerRb.linearVelocity = new Vector2(horizontalInput * moveSpeed, playerRb.linearVelocity.y);
        playerAnimator.SetFloat("xVelocity", Math.Abs(playerRb.linearVelocityX));
        playerAnimator.SetFloat("yVelocity", playerRb.linearVelocityY);
    }



    void FlipSprite()
    //flip sprite based on player direction, do not reverse it will be goofy af
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1;
            transform.localScale = ls;
        }
    }
    // i dont know why i added this but i feel safe with this being here
    private void OnCollisionEnter2D(Collision2D collision)
    {
        isGrounded = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        isGrounded = false;
        playerAnimator.SetBool("IsJumping", !isGrounded);
    }
}

