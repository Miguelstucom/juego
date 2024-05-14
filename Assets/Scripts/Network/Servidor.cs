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
    network_driver = NetworkDriver.Create();
    var servidor_ws = NetworkEndpoint.Parse(ipAddress, port);
    if (network_driver.Bind(servidor_ws) != 0)
    {
        Debug.Log("SERVIDOR::No ens hem pogut vincular amb el port:" + servidor_ws.Port+" i IP:"+ servidor_ws.Address);
    }
    else
    {
        connexions = new NativeList<NetworkConnection>(4, Allocator.Persistent);
        network_driver.Listen();
        Debug.Log("SERVIDOR:: a la espera en el port:" + servidor_ws.Port + " i IP:" + servidor_ws.Address);
    }
}
    private void OnDestroy()
    {
        if (network_driver.IsCreated)
        {
            network_driver.Dispose();
            connexions.Dispose();
        }
        Debug.Log("SERVIDOR:: stop");
    }

    


    void Update()
    {
        network_driver.ScheduleUpdate().Complete();
        RefrescarConnexions();
        LlegirMissatgesRebuts();
    }
    private void RefrescarConnexions()
    {
        for(int i=0; i < connexions.Length; i++)
        {
            if (!connexions[i].IsCreated)
            {
                connexions.RemoveAtSwapBack(i);
                i--;
            }
        }
        NetworkConnection net_connection = network_driver.Accept();
        while (net_connection != default(NetworkConnection))
        {
connexions_dic[++total_connexions] = net_connection;

            Debug.Log("SERVER:Nova connexio acceptada");
            net_connection = network_driver.Accept();
        }
    }
    public void LlegirMissatgesRebuts()
    {

foreach (var kvp in connexions_dic)
        
        {
            
                        DataStreamReader stream_lectura;

NetworkEvent.Type net_event_type = network_driver.PopEventForConnection(kvp.Value, out stream_lectura);

            while (net_event_type != NetworkEvent.Type.Empty)
            {
                switch (net_event_type)
                {
                    case NetworkEvent.Type.Data:
                        FixedString128Bytes text = stream_lectura.ReadFixedString128();
                        Debug.Log("SERVIDOR rep:" + text);

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

    BroadCastOthers(kvp.Key, kvp.Value, new Missatge(-1, "object_update", JsonUtility.ToJson(interaction)));
    break;

    case "palanca_activada":
    Debug.Log($"Palanca activada por el cliente {kvp.Key}");

    BroadCastOthers(kvp.Key, kvp.Value, new Missatge(-1, "palanca_activada", ""));

    SendToOne(kvp.Value, new Missatge(kvp.Key, "confirmacion_palanca", "Palanca activada y trampas eliminadas."));
    break;

    default:
        Debug.Log("accio del client no controlada en el servidor");
        break;
}


                        break;
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("SERVIDOR: client desconectat");
connexions_dic.Remove(kvp.Key);
                        break;
                    default:
                        Debug.Log("SERVIDOR event no controlat: " + net_event_type);
                        break;
                }
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

