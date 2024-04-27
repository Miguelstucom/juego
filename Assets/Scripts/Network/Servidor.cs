using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class Servidor : MonoBehaviour
{
    public NetworkDriver net_driver;
    private NativeList<NetworkConnection> connections;
    public string ipAddress = "127.0.0.1";
    public ushort port = 9000;

    // Start is called before the first frame update
    void Start()
    {
        net_driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.Parse(ipAddress, port);
        if (net_driver.Bind(endpoint) != 0)
        {
            Debug.Log("Error al iniciar el servidor en: " + ipAddress + ":" + port);
        }
        else
        {
            connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
            net_driver.Listen();
            Debug.Log("Servidor escuchando en: " + ipAddress + ":" + port);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!net_driver.IsCreated) return;

        net_driver.ScheduleUpdate().Complete();
        CheckForNewConnections();
        CheckForDisconnected();
    }

    void CheckForNewConnections()
    {
        NetworkConnection c;
        while ((c = net_driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(c);
            Debug.Log("Nueva conexión aceptada");
        }
    }

    void CheckForDisconnected()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }
    }

    private void OnDestroy()
    {
        if (connections.IsCreated)
            connections.Dispose();

        if (net_driver.IsCreated)
            net_driver.Dispose();
    }


}

