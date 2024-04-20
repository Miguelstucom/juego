using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class addScore : MonoBehaviour
{

private string loginURL = "http://localhost:80/unitybackend/addscore.php";

    public void Login(string username, int score)
    {
        StartCoroutine(LoginCoroutine(username, score));
    }

    IEnumerator LoginCoroutine(string username, int score)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("score", score);

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
                
            }
        }
    }

}
