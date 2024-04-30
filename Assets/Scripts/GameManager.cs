// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class GameManager : MonoBehaviour
// {
//     public static GameManager instance { get; private set; }

//     // Awake se llama antes de Start
//     void Awake()
//     {
//         if (instance == null)
//         {
//             // Si no hay una instancia de GameManager, asigna esta instancia a la variable instance
//             instance = this;
//         }
//         else
//         {
//             // Si ya hay una instancia de GameManager, destruye este objeto para evitar duplicados
//             Destroy(gameObject);
//         }

//         // Mantén este objeto a través de las escenas
//         DontDestroyOnLoad(gameObject);
//     }

//     // Resto del código de GameManager...

//     // Start is called before the first frame update
//     void Start()
//     {

//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }

//     public void reestart()
//     {
//         Scene currentScene = SceneManager.GetActiveScene();
//         SceneManager.LoadScene(currentScene.name);
//     }
// }
