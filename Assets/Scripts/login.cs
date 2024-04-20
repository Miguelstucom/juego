using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;  // Necesario para cambiar de escena

public class LoginManager : MonoBehaviour
{
    [SerializeField]
    public TMPro.TMP_InputField usernameInputField; // Referencia al campo de nombre de usuario
    public TMPro.TMP_InputField passwordInputField; // Referencia al campo de contraseña
    public Button loginButton; // Referencia al botón de login

    private string loginURL = "http://localhost:80/unitybackend/login.php"; // URL de tu script PHP

    void Start()
    {
        loginButton.onClick.AddListener(() => Login(usernameInputField.text, passwordInputField.text));
    }

    public void Login(string username, string password)
    {
        StartCoroutine(LoginCoroutine(username, password));
    }

    IEnumerator LoginCoroutine(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(loginURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error en el login: " + www.error);
            }
            else
            {
                Debug.Log("Respuesta del servidor: " + www.downloadHandler.text);
                HandleResponse(www.downloadHandler.text, username);
            }
        }
    }

    void HandleResponse(string response, string username)
    {
        if (response.Contains("Login exitoso"))
        {
            Debug.Log("¡Login exitoso!");
            PlayerPrefs.SetString("username", username);
            PlayerPrefs.Save();
            SceneManager.LoadScene(1);
        }
        else if (response.Contains("Usuario no encontrado"))
        {
            Debug.LogError("Usuario no encontrado. Por favor verifica tus credenciales.");
        }
        else
        {
            Debug.LogError("Error en la respuesta del servidor: " + response);
        }
    }
}

