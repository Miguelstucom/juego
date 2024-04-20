using UnityEngine;
using UnityEngine.Networking;
using TMPro;  // Asegúrate de incluir este namespace
using System.Collections;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    public TextMeshProUGUI[] scoreTexts; // Array para los textos de los scores usando TextMeshPro

    void Start()
    {
        StartCoroutine(GetScores());
    }

    IEnumerator GetScores()
    {
        string url = "http://localhost/unitybackend/getscores.php";  // Asegúrate de que la URL sea correcta
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            ProcessScores(www.downloadHandler.text);
        }
    }

    private void ProcessScores(string jsonData)
    {
        Score[] score = JsonHelper.FromJson<Score>(jsonData);
        for (int i = 0; i < score.Length && i < scoreTexts.Length; i++)
        {
            scoreTexts[i].text = $"user: {score[i].user}, Score: {score[i].score}";
        }
    }
}

[System.Serializable]
public class Score
{
    public string user;
    public int score;
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{\"items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}


