using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasmManager : MonoBehaviour
{

    [SerializeField] private GameObject serverManagerPrefab;
    private GameObject serverManager;

    public void ClickStartServer()
    {
        if (serverManager == null)
        {
            serverManager = Instantiate(serverManagerPrefab);
        }
        else
        {
            Destroy(serverManager);
            serverManager = Instantiate(serverManagerPrefab);
        }
    }

    [SerializeField] private GameObject clientManagerPrefab;
    private GameObject clientManager;

    public void ClickStartClient()
    {
        if (clientManager == null)
        {
            clientManager = Instantiate(clientManagerPrefab);
        }
        else
        {
            Destroy(clientManager);
            clientManager = Instantiate(clientManagerPrefab);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
