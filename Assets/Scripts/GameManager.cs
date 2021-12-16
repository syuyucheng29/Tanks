using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks

{
    public static GameManager instance;
    string gameVersion = "1";

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogErrorFormat(gameObject,
                    "Multiple instances of {0} is not allow", GetType().Name);
            DestroyImmediate(gameObject);
            return;
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        DontDestroyOnLoad(gameObject);
        instance = this;
    }
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
    }
    public override void OnConnected()
    {
        Debug.Log("PUN Connected");
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Connected to Master");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Disconnected was called by PUN with reason {0}", cause);
    }
    public void JoinGameRoom()
    {
        var options = new RoomOptions
        {
            MaxPlayers = 6
        };

        PhotonNetwork.JoinOrCreateRoom("Kingdom", options, null);
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room!!");
    }
}