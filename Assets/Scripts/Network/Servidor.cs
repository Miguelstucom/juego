using System;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.Collections;

using UnityEngine;

public class Servidor : MonoBehaviour
{
    public string ipAddress = "10.118.2.255";
    public ushort port = 9000;

//d
public NetworkDriver network_driver;
private NativeList<NetworkConnection> connexions;

private Dictionary<int, NetworkConnection> connexions_dic = new Dictionary<int, NetworkConnection>();
private int total_connexions = 0;

    void Start()
{
    string ipAddressCanvas = "10.118.2.255";
    ushort.TryParse( "9000",  out var portCanvas);
    ipAddress = string.IsNullOrEmpty(ipAddressCanvas)  ? ipAddress: ipAddressCanvas;
    port = portCanvas == 0 ? port : portCanvas;

    //creem el driver i l'intentem vincular a l'adre�a indicada
    network_driver = NetworkDriver.Create();
    var servidor_ws = NetworkEndpoint.Parse(ipAddress, port);
    if (network_driver.Bind(servidor_ws) != 0)
    {
        Debug.Log("SERVIDOR::No ens hem pogut vincular amb el port:" + servidor_ws.Port+" i IP:"+ servidor_ws.Address);
    }
    else
    {
        //iniciem la llista que emmagatzemi les conexions
        connexions = new NativeList<NetworkConnection>(4, Allocator.Persistent);
        //ens quedem a l'espera de noves conexion
        network_driver.Listen();
        Debug.Log("SERVIDOR:: a la espera en el port:" + servidor_ws.Port + " i IP:" + servidor_ws.Address);
    }
}
    /*Quan tanquem el servidor hem d'alliberar la conexi� i la llista de conexions
     */

    private void OnDestroy()
    {//si hem inicialitzat el driver, llavors l'esborrarem de la memoria junt amb la llista de conexions
        if (network_driver.IsCreated)
        {
            network_driver.Dispose();
            connexions.Dispose();
        }
        Debug.Log("SERVIDOR:: stop");
    }

    


    void Update()
    {
        //esperem a rebre tota la informaci� del driver
        network_driver.ScheduleUpdate().Complete();
        RefrescarConnexions();
        LlegirMissatgesRebuts();
    }
    /*Netejem connexions caducades i acceptem noves connexions*/
    private void RefrescarConnexions()
    {
        //netejar connexions caducades
        for(int i=0; i < connexions.Length; i++)
        {
            if (!connexions[i].IsCreated)
            {
                connexions.RemoveAtSwapBack(i);
                i--;
            }
        }
        //acceptem noves connexions amb network_driver.Accept();
        //retorna  default(NetworkConnection) si no hi han m�s connexions
        NetworkConnection net_connection = network_driver.Accept();
        while (net_connection != default(NetworkConnection))
        {
//total_connexions++;
connexions_dic[++total_connexions] = net_connection;
//d            connexions.Add(net_connection);

            Debug.Log("SERVER:Nova connexio acceptada");
            net_connection = network_driver.Accept(); //llegim la seguen connexio
        }
    }
    /*Recorrem totes les conexions llegint els "NeworkEvents" que s'hagin pogut produir en aquella conexi�. 
     * Els "events" que controlarem son:
     *  "Data" -> ens indica que en la conexi� hem rebut informaci�
     *  "Disconect" -> ens indica que el client s'ha desconectat
     * */
    public void LlegirMissatgesRebuts()
    {

//d       for (int k = 0; k < connexions.Length; k++)
foreach (var kvp in connexions_dic)
        
        {
            
                        //per cada conexi� mirem els events que s'hi han produ�t
                        //out permet pasar una variable per referencia (a diferencia de ref no es necesari que estigui declarada )
                        DataStreamReader stream_lectura;

//d            NetworkEvent.Type net_event_type = network_driver.PopEventForConnection(connexions[k], out stream_lectura);
NetworkEvent.Type net_event_type = network_driver.PopEventForConnection(kvp.Value, out stream_lectura);

            //mentres no retorni un event Empty, llegim el event
            while (net_event_type != NetworkEvent.Type.Empty)
            {
                switch (net_event_type)
                {
                    case NetworkEvent.Type.Data:
                        FixedString128Bytes text = stream_lectura.ReadFixedString128();
                        Debug.Log("SERVIDOR rep:" + text);

                        //podem escriure una resposta
                        /* network_driver.BeginSend(NetworkPipeline.Null, connexions[k], out var stream_escritura);
                         stream_escritura.WriteFixedString128("-"+ text);
                         network_driver.EndSend(stream_escritura);*/

//d                      BroadCast(text.ToString());
Missatge mis = JsonUtility.FromJson<Missatge>(text.ToString());
switch (mis.op)
{

    case "inicia":
        Debug.Log("client  inicia");
        KeyClientList key_ids = new KeyClientList(new List<int>(connexions_dic.Keys));
                                
        SendToOne(kvp.Value, new Missatge(kvp.Key, "inicia", JsonUtility.ToJson(key_ids)));
                BroadCastOthers(kvp.Key, kvp.Value, new Missatge(kvp.Key, "nou_oponent", "s'ha iniciat un nou oponent"));
        break;
    case "mou":
        Debug.Log("client es mou");
        BroadCastOthers(kvp.Key, kvp.Value, mis);
        break;

        case "object_interacted":
    InteractionData interaction = JsonUtility.FromJson<InteractionData>(mis.msg);
    Debug.Log($"Object with ID {interaction.objectID} was interacted with by client {kvp.Key}");

    // Broadcast to other clients that the object should be deactivated
    BroadCastOthers(kvp.Key, kvp.Value, new Missatge(-1, "object_update", JsonUtility.ToJson(interaction)));
    break;

    case "palanca_activada":
    Debug.Log($"Palanca activada por el cliente {kvp.Key}");

    // Suponemos que este mensaje ya incluye la lógica necesaria para identificar qué trampas eliminar
    // Broadcast this message to all other clients so they can update their game state
    BroadCastOthers(kvp.Key, kvp.Value, new Missatge(-1, "palanca_activada", ""));

    // Puedes elegir también enviar un mensaje de confirmación al cliente que activó la palanca, si es necesario
    SendToOne(kvp.Value, new Missatge(kvp.Key, "confirmacion_palanca", "Palanca activada y trampas eliminadas."));
    break;

    default:
        Debug.Log("accio del client no controlada en el servidor");
        break;
}


                        break;
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("SERVIDOR: client desconectat");
//d                        connexions[k] = default(NetworkConnection);
connexions_dic.Remove(kvp.Key);
                        break;
                    default:
                        Debug.Log("SERVIDOR event no controlat: " + net_event_type);
                        break;
                }
//d net_event_type = network_driver.PopEventForConnection(connexions[k], out stream_lectura);
net_event_type = network_driver.PopEventForConnection(kvp.Value, out stream_lectura);

            }

        }
    }
public virtual void BroadCastOthers(int key, NetworkConnection connection, Missatge missatge)
{
    foreach (var kvp in connexions_dic)
    {
        if (kvp.Value.IsCreated && kvp.Key != key)
        {

            string text = JsonUtility.ToJson(missatge);
          //  Debug.Log("SERVER:broadcast:" + text);
            network_driver.BeginSend(NetworkPipeline.Null, kvp.Value, out var stream_escritura);
            stream_escritura.WriteFixedString128(text);
            network_driver.EndSend(stream_escritura);
        }
    }
}
public virtual void SendToOne(NetworkConnection connection, Missatge missatge)
{
        Debug.Log("Send to One: " + missatge.msg);
    string text = JsonUtility.ToJson(missatge);
    network_driver.BeginSend(NetworkPipeline.Null, connection, out var stream_escritura);
    stream_escritura.WriteFixedString128(text);
    network_driver.EndSend(stream_escritura);
}


    public virtual void BroadCast(string text)
    {
        for (int i = 0; i < connexions.Length; i++)
        {
            if (connexions[i].IsCreated)
            {
                Debug.Log("SERVER:broadcast:" + text);
                network_driver.BeginSend(NetworkPipeline.Null, connexions[i], out var stream_escritura);
                stream_escritura.WriteFixedString128("-" + text);
                network_driver.EndSend(stream_escritura);
            }
        }
    }
}
[System.Serializable]
public class Missatge
{
    public int key;
    public string op;
    public  string msg;
    public Missatge(int key = -1, string op="", string msg ="")
    {
        this.key = key;
        this.op = op;
        this.msg = msg;
    }
}

[System.Serializable]
public class InteractionData {
    public int objectID;

    public InteractionData(int objectID) {
        this.objectID = objectID;
    }
}


[System.Serializable]
public class KeyClientList
{    public KeyClientList(List<int> items)
    {
        this.Items = items;
    }
    public List<int> Items;
}

