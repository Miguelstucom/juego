using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class Client : MonoBehaviour
{

    public string ipAddress = "127.0.0.1";
    public ushort port = 9000;
    public NetworkDriver net_driver;
    public NetworkConnection conexion;
    // Start is called before the first frame update
    void Start()
    {
        net_driver = NetworkDriver.Create();
        conexion = default(NetworkConnection);
        var direccion_servidor = NetworkEndpoint.Parse(ipAddress,port);
        conexion = net_driver.Connect(direccion_servidor);
        Debug.Log("CLIENT: Conectando");
    }

    public void OnDestroy(){
        conexion.Disconnect(net_driver);
        conexion = default(NetworkConnection);
        net_driver.Dispose();
    }

    private void CheckConexion(){
        if(!conexion.IsCreated){
            Debug.Log("CLIENTE: no se ha podido conectar");
        }
    }
    private void check(){
        Unity.Collections.DataStreamReader stream_lectura;
        NetworkEvent.Type net_evt_type = net_driver.PopEventForConnection(conexion, out stream_lectura);
        while (net_evt_type!=NetworkEvent.Type.Empty){
            switch(net_evt_type){
                case NetworkEvent.Type.Connect:
                Debug.Log("Cliente: conectado al servidor");
                break;
            }
            net_evt_type = net_driver.PopEventForConnection(conexion, out stream_lectura);
        }
    }

    // Update is called once per frame
    void Update()
    {
        net_driver.ScheduleUpdate().Complete();
        CheckConexion();
        check();
    }
}
