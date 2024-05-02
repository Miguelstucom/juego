using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class Server : MonoBehaviour
{
    public ushort port = 9000;
    public NetworkDriver net_driver;
    private NetworkPipeline pipeline;
    public GameObject playerPrefab;
    private NetworkEndpoint serverEndpoint;

    void Start()
    {
        net_driver = NetworkDriver.Create();
        serverEndpoint = NetworkEndpoint.AnyIpv4;
        serverEndpoint.Port = port;

        if (net_driver.Bind(serverEndpoint) != 0)
        {
            Debug.Log("Error al enlazar el servidor al puerto " + port);
        }
        else
        {
            net_driver.Listen();
            Debug.Log("Servidor escuchando en el puerto " + port);
        }
    }

    private void OnDestroy()
    {
        net_driver.Dispose();
    }

    void Update()
    {
        net_driver.ScheduleUpdate().Complete();

        // Aceptar nuevas conexiones
        NetworkConnection conexion;
        while ((conexion = net_driver.Accept()) != default(NetworkConnection))
        {
            Debug.Log("Servidor: nuevo cliente conectado");

            // Envía un mensaje de confirmación al cliente de que se ha conectado correctamente
            // Esto puede ser necesario dependiendo de la lógica de tu juego
            // Enviar mensaje de confirmación

            // Instanciar el GameObject del jugador en el servidor
            GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}

