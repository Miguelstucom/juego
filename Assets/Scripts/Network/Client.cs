using UnityEngine;
using Unity.Networking.Transport;

public class Client : MonoBehaviour
{
    public string ipAddress = "127.0.0.1";
    public ushort port = 9000;
    public NetworkDriver net_driver;
    public NetworkConnection conexion;

    public GameObject playerPrefab; // Prefab del jugador

    void Start()
    {
        net_driver = NetworkDriver.Create();
        conexion = default(NetworkConnection);
        var direccion_servidor = NetworkEndpoint.Parse(ipAddress, port);
        conexion = net_driver.Connect(direccion_servidor);
        Debug.Log("CLIENT: Conectando");
    }

    public void OnDestroy()
    {
        conexion.Disconnect(net_driver);
        conexion = default(NetworkConnection);
        net_driver.Dispose();
    }

    private void CheckConexion()
    {
        if (!conexion.IsCreated)
        {
            Debug.Log("CLIENTE: no se ha podido conectar");
        }
    }

    private void CheckEvents()
    {
        Unity.Collections.DataStreamReader stream_lectura;
        NetworkEvent.Type net_evt_type = net_driver.PopEventForConnection(conexion, out stream_lectura);
        while (net_evt_type != NetworkEvent.Type.Empty)
        {
            switch (net_evt_type)
            {
                case NetworkEvent.Type.Connect:
                    Debug.Log("Cliente: conectado al servidor");
                    // Instanciar el GameObject del jugador en el cliente
                    GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                    break;
            }
            net_evt_type = net_driver.PopEventForConnection(conexion, out stream_lectura);
        }
    }

    void Update()
    {
        net_driver.ScheduleUpdate().Complete();
        CheckConexion();
        CheckEvents();
    }
}

