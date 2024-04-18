using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void clickEnviaPost()
    {
        WWWForm informacio = new WWWForm(); //L’omplim amb els valors a enviar
        informacio.AddField("username", "dawsed");
        informacio.AddField("password", "1234");
        StartCoroutine(enviaPost(informacio));
    }

    IEnumerator enviaPost(WWWForm parametres)
    {
        UnityWebRequest www = UnityWebRequest.Post("http://10.118.2.255:80/login.php ", parametres);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string txtResp = www.downloadHandler.text; //Obtenim la resposta com un text simp
            Debug.Log("Response: " + txtResp);
        }
    }
}
