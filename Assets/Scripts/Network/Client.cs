using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class Client : MonoBehaviour
{
    public string ipAddress = "10.118.2.255";
    public ushort port = 9000;
public int id_key=-1; //identificador que donar� el servidor
private Dictionary<int, Player> oponents = new Dictionary<int, Player>();
Player player;
public float velocitat = 0.0001f;
    Vector3 posicio_ultima = new Vector3(0, 0);
    [SerializeField] Player playerPrefab;

    public NetworkDriver network_driver;
    private NetworkConnection connexio;
    // Start is called before the first frame update
    void Start()
    {
        string ipAddressCanvas = "10.118.2.255";
        ushort.TryParse("9000", out var portCanvas);
        ipAddress = string.IsNullOrEmpty(ipAddressCanvas) ? ipAddress : ipAddressCanvas;
        port = portCanvas == 0 ? port : portCanvas;

        //creem el driver i l'intentem vincular a l'adre�a indicada
        network_driver = NetworkDriver.Create();
        connexio = default(NetworkConnection);
        var direccio_server= NetworkEndpoint.Parse(ipAddress, port);
        connexio = network_driver.Connect(direccio_server);
        Debug.Log("CLIENT::conectant amb el port:" + direccio_server.Port + " i IP:" + direccio_server.Address);

    }
    public void OnDestroy()
    {
        connexio.Disconnect(network_driver);
        //A good pattern is to always set your NetworkConnection to
        //default(NetworkConnection) to avoid stale references.
        connexio = default(NetworkConnection); 
        network_driver.Dispose();
    }

    void Update()
    {

        //esperem a rebre tota la informaci� del driver
       network_driver.ScheduleUpdate().Complete();
        CheckConnexio(); //comprovem si encara estem conectats amb el servidor
        LlegirMissatgesRebuts(); //actualitzem els missatges rebuts del servidor

       
    }
    //d
private void FixedUpdate()
{
    // Obtener la entrada del teclado para el movimiento
    float mov_h = Input.GetAxis("Horizontal");
    float mov_v = Input.GetAxis("Vertical");

    // Calcular el vector de movimiento en el plano XY
    Vector3 movimiento = new Vector3(mov_h, mov_v, 0) * velocitat;

    // Verificar si hay un cambio de posición desde la última actualización
    if (posicio_ultima != movimiento)
    {
        // Aplicar el movimiento al GameObject en el plano XY
        player.transform.Translate(movimiento);

        // Guardar la última posición conocida
        posicio_ultima = movimiento;

        // Enviar información de movimiento al servidor
        SendMsgServer(new Missatge(id_key, "mou", JsonUtility.ToJson(player.transform.position)));
    }
}

    private void CheckConnexio()
    {
        if (!connexio.IsCreated)
        {
            Debug.Log("CLIENT:: error de conexio amb el servidor");
        }
    }
    private void LlegirMissatgesRebuts()
    {
        // mirem els events que s'hi han produ�t
         DataStreamReader stream_lectura;
        NetworkEvent.Type net_event_type = network_driver.PopEventForConnection(connexio, out stream_lectura);
        //mentres no retorni un event Empty, llegim el event
        while (net_event_type != NetworkEvent.Type.Empty)
        {
            switch (net_event_type)
            {
                case NetworkEvent.Type.Connect:

                    Debug.Log("CLIENT:: conectat amb el servidor");
                    //dd streamEscritura.WriteFixedString128("Soc el client");
                    SendMsgServer(new Missatge(-1, "inicia", ""));
                    break;

                case NetworkEvent.Type.Data:
                    FixedString128Bytes text = stream_lectura.ReadFixedString128();
                    Debug.Log("CLIENT rep:" + text);
                    //podem escriure una resposta
                    // network_driver.BeginSend(NetworkPipeline.Null, connexio, out var stream_escritura);
                    // stream_escritura.WriteFixedString128("OK");
                    // network_driver.EndSend(stream_escritura);
                    Missatge mis = JsonUtility.FromJson<Missatge>(text.ToString());
                    switch (mis.op)
                    {
                        case "inicia":
                            id_key = mis.key;

                            
                            KeyClientList keysList = JsonUtility.FromJson<KeyClientList>(mis.msg);

                            foreach (int key in keysList.Items)
                            {
                                if(key!= id_key)
                                {
                                    Player oponent = Instantiate(playerPrefab, new Vector3(3, 3), Quaternion.identity);
                                    oponent.is_main_player = false;
                                    oponents[key] = oponent;
                                }
                               
                            }

                                player = Instantiate(playerPrefab, new Vector3(1, 1), Quaternion.identity);
                                player.is_main_player = true;
                            break;
                        case "nou_oponent":
                            
                            Player g1 = Instantiate(playerPrefab, new Vector3(3,3), Quaternion.identity);
                            g1.is_main_player = false;
                            oponents[mis.key] = g1;


                            break;
                        case "mou":
                            oponents[mis.key].transform.position = ( JsonUtility.FromJson<Vector3>(mis.msg));
                            break;
                    }

                    break;
                case NetworkEvent.Type.Disconnect:
                    Debug.Log("SERVIDOR: client desconectat");
                    connexio = default(NetworkConnection);
                    break;
                default:
                    Debug.Log("CLIENT event no controlat: " + net_event_type);
                    break;
            }
            net_event_type = network_driver.PopEventForConnection(connexio, out stream_lectura);
        }
    }

    public void SendChatMessage(string text)
    {
        network_driver.BeginSend(connexio, out var streamEscritura);
        streamEscritura.WriteFixedString128("CLIENT: "+ text);
        network_driver.EndSend(streamEscritura);
    }
    public void SendMsgServer(Missatge m)
    {
        network_driver.BeginSend(connexio, out var streamEscritura);
        streamEscritura.WriteFixedString128(JsonUtility.ToJson(m));

        network_driver.EndSend(streamEscritura);
    }
}
