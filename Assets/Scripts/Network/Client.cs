using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class Client : MonoBehaviour
{
    public string ipAddress = "10.118.2.255";
    public ushort port = 9000;
public int id_key=-1;
private Dictionary<int, Player> oponents = new Dictionary<int, Player>();
Player player;
public float velocitat = 0.0001f;
    Vector3 posicio_ultima = new Vector3(0, 0);
    [SerializeField] Player playerPrefab;

    public NetworkDriver network_driver;
    private NetworkConnection connexio;
    void Start()
    {
        string ipAddressCanvas = "10.118.2.255";
        ushort.TryParse("9000", out var portCanvas);
        ipAddress = string.IsNullOrEmpty(ipAddressCanvas) ? ipAddress : ipAddressCanvas;
        port = portCanvas == 0 ? port : portCanvas;
        network_driver = NetworkDriver.Create();
        connexio = default(NetworkConnection);
        var direccio_server= NetworkEndpoint.Parse(ipAddress, port);
        connexio = network_driver.Connect(direccio_server);
        Debug.Log("CLIENT::conectant amb el port:" + direccio_server.Port + " i IP:" + direccio_server.Address);

    }
    public void OnDestroy()
    {
        connexio.Disconnect(network_driver);
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
        float mov_h = Input.GetAxis("Horizontal");
        float mov_v = Input.GetAxis("Vertical");

        Vector3 posicio = new Vector3(mov_h, mov_v, 0f) * velocitat ;
        if (posicio_ultima != posicio)
        {
            player.transform.Translate(posicio);
            //avisa al servidor
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

                            if (id_key != 1){
                                player = Instantiate(playerPrefab, new Vector3(-4, -1), Quaternion.identity);
                                player.is_main_player = false;
                            }else{
                                player = Instantiate(playerPrefab, new Vector3(-4, 1), Quaternion.identity);
                                player.is_main_player = true;}

                            break;
                        case "nou_oponent":
                            
                            Player g1 = Instantiate(playerPrefab, new Vector3(-4,-1), Quaternion.identity);
                            g1.is_main_player = false;
                            oponents[mis.key] = g1;


                            break;
                        case "mou":
                            oponents[mis.key].transform.position = ( JsonUtility.FromJson<Vector3>(mis.msg));
                            break;

                        case "object_update":
                            InteractionData data = JsonUtility.FromJson<InteractionData>(mis.msg);
                            GameObject obj = FindObjectByID(data.objectID);
                            if (obj != null) {
                                obj.SetActive(false);
                            }
                            break;
                        case "palanca_activada":
                        RemoveTrapFromTilemap();
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

GameObject FindObjectByID(int id) {
    // This method should locate an object by ID. Implement this based on your game's logic.
    foreach (var obj in FindObjectsOfType<GameObject>()) {
        if (obj.GetInstanceID() == id) {
            return obj;
        }
    }
    return null; // Return null if object not found
}

void RemoveTrapFromTilemap() {
    // Encuentra todos los objetos en la escena con el tag "Traps"
    GameObject[] traps = GameObject.FindGameObjectsWithTag("Traps");
    foreach (GameObject trap in traps) {
        // Desactiva el objeto para dejar de mostrarlo y que deje de interactuar en el juego
        // trap.SetActive(false);

        // O, si prefieres eliminar completamente el objeto de la escena, puedes usar Destroy
        Destroy(trap);
    }

    Debug.Log("Todas las trampas han sido eliminadas del tilemap");
}

void OnTriggerEnter2D(Collider2D other) {
    if (other.gameObject.CompareTag("slime")) {
        Debug.Log("tocando slime");

        // Obtiene el ID de instancia del objeto 'slime' para la identificación en el servidor
        int objectID = other.gameObject.GetInstanceID();

        // Desactiva el objeto 'slime' localmente inmediatamente
        other.gameObject.SetActive(false);

        // Envía un mensaje al servidor sobre esta interacción
        SendMsgServer(new Missatge(id_key, "object_interacted", JsonUtility.ToJson(new InteractionData(objectID))));
    }

        if (other.gameObject.CompareTag("Palanca")) {
        Debug.Log("Interacción con Palanca");

        // Lógica para eliminar una trampa del tilemap
        RemoveTrapFromTilemap();

        // Envía un mensaje al servidor sobre esta interacción
        SendMsgServer(new Missatge(id_key, "palanca_activada", ""));
    }
}


[System.Serializable]
public class InteractionData {
    public int objectID;

    public InteractionData(int objectID) {
        this.objectID = objectID;
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
