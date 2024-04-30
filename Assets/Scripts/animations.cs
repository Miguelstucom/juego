using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animations : MonoBehaviour
{

    public float speed = 5.0f; // Velocidad de movimiento del jugador
      public static Animator animator; // Para controlar las animaciones
    private Vector2 movement; // Dirección del movimiento

    void Start()
    {
        animator = GetComponent<Animator>(); // Obtiene el componente Animator del objeto
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); // Recoge movimiento horizontal
        float moveY = Input.GetAxisRaw("Vertical"); // Recoge movimiento vertical

        movement = new Vector2(moveX, moveY).normalized; // Normaliza el vector para mantener velocidad constante

        if (movement.magnitude > 0)
        {
            animator.SetBool("isWalking", true); // Activa la animación de caminar si hay movimiento
        }
        else
        {
            animator.SetBool("isWalking", false); // Desactiva la animación si no hay movimiento
        }

    }
}