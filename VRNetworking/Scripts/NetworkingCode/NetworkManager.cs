using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
[Serializable]
public class BackupServer
{
    public string AppID;
    public string VoiceID;
}
public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region Vars
    public static NetworkManager Instance { get; private set; }

    [Header("Photon")]
    public string AppID;
    public string VoiceID;
    public List<BackupServer> BackupServers = new();
    public string Region = "us";
    public Color PlayerColor;
    public string Queue = "Default";
    public int MaxPlayers = 10;
    public Dictionary<string, string> Cosmetics { get; private set; } = new();

    [Header("Player")]
    public string PrefabLocation = "VRNetworkingPlayer";
    public Transform Head;
    public Transform HandL;
    public Transform HandR;
    public string DefaultName = "Ape";

    [Header("Other")]
    public bool AutoConnect = true;
    public bool AutoJoin = true;
    public bool UseBackupIfFail = true;

    private GameObject TemporaryPlayer;
    private RoomOptions RoomOpts;
    private int CurrentIndex = 0;
    private readonly List<string> DebugLog = new List<string>();
    private readonly int MaxDebug = 20;
    #endregion

    #region Unity Stuff
    void Awake()
    {
        if (string.IsNullOrEmpty(AppID) || string.IsNullOrEmpty(VoiceID))
        {
            Debug.LogError("Missing App IDs");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = AppID;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = VoiceID;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = Region;
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (AutoConnect)
            ConnectToPhoton();

        if (PlayerPrefs.HasKey("Color"))
            PlayerColor = JsonUtility.FromJson<Color>(PlayerPrefs.GetString("Color"));

        if (PlayerPrefs.HasKey("Cosmetics"))
            Cosmetics = JsonUtility.FromJson<Dictionary<string, string>>(PlayerPrefs.GetString("Cosmetics"));
    }
    #endregion

    #region Debug Methods
    public void AddDebugMessage(string Msg, bool IsError = false)
    {
        string NewMessage = $"[{DateTime.Now:HH:mm:ss}] {(IsError ? "ERROR: " : "")}{Msg}";
        DebugLog.Add(NewMessage);
        if (DebugLog.Count > MaxDebug)
            DebugLog.RemoveAt(0);
    }
    public string GetDebugLog()
    {
        return string.Join("\n", DebugLog);
    }
    public void ClearDebugLog() => DebugLog.Clear();
    #endregion

    #region Connection
    public void ConnectToPhoton() => PhotonNetwork.ConnectUsingSettings();
    public static void ConnectAuth(string ID, string Token)
    {
        var customAuth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };
        customAuth.AddAuthParameter("username", ID);
        customAuth.AddAuthParameter("token", Token);
        PhotonNetwork.AuthValues = customAuth;
        PhotonNetwork.ConnectUsingSettings();
    }
    public static void ChangeServer(string ID, string Voice)
    {
        PhotonNetwork.Disconnect();
        Instance.AppID = ID;
        Instance.VoiceID = Voice;
        Instance.ConnectToPhoton();
    }
    #endregion

    #region Player Settings
    public void SetPlayerName(string Name)
    {
        PhotonNetwork.LocalPlayer.NickName = Name;
        PlayerPrefs.SetString("Username", Name);
        if (NetworkPlayer.LocalPlayer != null)
            NetworkPlayer.LocalPlayer.View.RPC(nameof(NetworkPlayer.RPCRefreshPlayerValues), RpcTarget.All);
        AddDebugMessage($"Player name set to: {Name}");
    }
    public void SetPlayerColor(Color Color)
    {
        PlayerColor = Color;
        var Props = PhotonNetwork.LocalPlayer.CustomProperties;
        Props["Color"] = JsonUtility.ToJson(Color);
        PhotonNetwork.LocalPlayer.SetCustomProperties(Props);
        PlayerPrefs.SetString("Color", JsonUtility.ToJson(Color));
        if (NetworkPlayer.LocalPlayer != null)
            NetworkPlayer.LocalPlayer.View.RPC(nameof(NetworkPlayer.RPCRefreshPlayerValues), RpcTarget.All);
        AddDebugMessage($"Player color set to: {Color}");
    }
    public void UpdateAllCosmetics(Dictionary<string, string> NewCosmetics)
    {
        Cosmetics = new Dictionary<string, string>(NewCosmetics);
        var props = new ExitGames.Client.Photon.Hashtable { ["Cosmetics"] = Cosmetics };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        PlayerPrefs.SetString("Cosmetics", JsonUtility.ToJson(Cosmetics));
        if (NetworkPlayer.LocalPlayer != null)
            NetworkPlayer.LocalPlayer.View.RPC(nameof(NetworkPlayer.RPCRefreshPlayerValues), RpcTarget.All);
        AddDebugMessage("Cosmetics updated");
    }
    public void SetCosmetic(string Type, string ID)
    {
        Cosmetics[Type] = ID;
        var Props = PhotonNetwork.LocalPlayer.CustomProperties;
        Props["Cosmetics"] = Cosmetics;
        PhotonNetwork.LocalPlayer.SetCustomProperties(Props);
        PlayerPrefs.SetString("Cosmetics", JsonUtility.ToJson(Cosmetics));
        if (NetworkPlayer.LocalPlayer != null)
            NetworkPlayer.LocalPlayer.View.RPC(nameof(NetworkPlayer.RPCRefreshPlayerValues), RpcTarget.All);
        AddDebugMessage($"Cosmetic {Type} set to {ID}");
    }
    #endregion

    #region Photon Callbacks
    public override void OnConnectedToMaster()
    {
        AddDebugMessage("Connected to Server");

        if (PlayerPrefs.GetString("Username") != null)
        {
            string Name = PlayerPrefs.GetString(DefaultName, "Player" + UnityEngine.Random.Range(1000, 9999));
            PhotonNetwork.LocalPlayer.NickName = Name;
            PlayerPrefs.SetString("Username", Name);
        }
        else
            PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("Username");

        UpdateNameAsync();

        PhotonNetwork.LocalPlayer.CustomProperties["Color"] = JsonUtility.ToJson(PlayerColor);
        PhotonNetwork.LocalPlayer.CustomProperties["Cosmetics"] = Cosmetics;
        if (AutoJoin)
            JoinRandom(Queue);
    }
    private async void UpdateNameAsync()
    {
        while (!PlayFabClientAPI.IsClientLoggedIn())
            await Task.Delay(500);

        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = PhotonNetwork.LocalPlayer.NickName
        },
        result =>
        {
            Debug.Log("Display Name Changed!");
        },
        error =>
        {
            Debug.Log("Error");
        });
    }
    public override void OnJoinedRoom()
    {
        TemporaryPlayer = PhotonNetwork.Instantiate(PrefabLocation, Vector3.zero, Quaternion.identity);
        AddDebugMessage($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
    }
    public override void OnDisconnected(DisconnectCause Cause)
    {
        if (TemporaryPlayer != null)
            PhotonNetwork.Destroy(TemporaryPlayer);
        AddDebugMessage($"Disconnected: {Cause}", true);

        if (UseBackupIfFail && (Cause == DisconnectCause.Exception || Cause == DisconnectCause.ServerTimeout))
        {
            if (CurrentIndex < BackupServers.Count)
            {
                var Entry = BackupServers[CurrentIndex++];
                AppID = Entry.AppID;
                VoiceID = Entry.VoiceID;
                PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = AppID;
                PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = VoiceID;
                AddDebugMessage($"Switching to Backup Server {CurrentIndex}");
                ConnectToPhoton();
            }
            else
            {
                AddDebugMessage("No more backup servers available.", true);
            }
        }
    }
    public override void OnJoinRandomFailed(short Code, string Msg)
    {
        AddDebugMessage($"Join random failed: {Msg}");
        var CodeStr = GenerateRoomCode();
        PhotonNetwork.CreateRoom(CodeStr, RoomOpts);
        AddDebugMessage($"Created new room: {CodeStr}");
    }
    #endregion

    #region Room Management
    public static void JoinRandom(string Queue = null)
    {
        var Props = new ExitGames.Client.Photon.Hashtable
        {
            ["Queue"] = Queue,
            ["Version"] = Application.version
        };

        var Options = new RoomOptions
        {
            MaxPlayers = (byte)Instance.MaxPlayers,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = Props,
            CustomRoomPropertiesForLobby = new[] { "Queue", "Version" }
        };

        Instance.RoomOpts = Options;
        PhotonNetwork.JoinRandomRoom(Props, Options.MaxPlayers);
        Instance.AddDebugMessage($"Joining random room in queue: {Queue}");
    }
    public static void JoinPrivate(string ID)
    {
        PhotonNetwork.JoinOrCreateRoom(ID, new RoomOptions
        {
            IsVisible = false,
            IsOpen = true,
            MaxPlayers = (byte)Instance.MaxPlayers
        }, null);
        Instance.AddDebugMessage($"Joining private room: {ID}");
    }
    public string GenerateRoomCode()
    {
        const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        return new string(Enumerable.Range(0, 4).Select(_ => Chars[UnityEngine.Random.Range(0, Chars.Length)]).ToArray());
    }
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(NetworkManager))]
public class NetworkManagerGUI : Editor
{
    private NetworkManager Manager;
    private string NewRoomCode = "";
    private string NewPlayerName = "";
    private Color NewPlayerColor = Color.white;
    private Dictionary<string, string> TempCosmetics = new();
    private string NewCosmeticType = "";
    private string NewCosmeticValue = "";
    private void OnEnable()
    {
        Manager = (NetworkManager)target;
        if (Manager.Cosmetics != null)
            TempCosmetics = new Dictionary<string, string>(Manager.Cosmetics);
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(20);

        EditorGUILayout.LabelField("Network Debug Tools", EditorStyles.boldLabel);
        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Connection Status", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Connected: {PhotonNetwork.IsConnected}");
        if (PhotonNetwork.IsConnected)
        {
            EditorGUILayout.LabelField($"Ping: {PhotonNetwork.GetPing()}ms");
            EditorGUILayout.LabelField($"Region: {PhotonNetwork.CloudRegion}");
            EditorGUILayout.LabelField($"In Room: {PhotonNetwork.InRoom}");

            if (PhotonNetwork.InRoom)
            {
                EditorGUILayout.LabelField($"Room: {PhotonNetwork.CurrentRoom.Name}");
                EditorGUILayout.LabelField($"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Connection Controls", EditorStyles.boldLabel);
        if (!PhotonNetwork.IsConnected)
        {
            if (GUILayout.Button("Connect To Photon"))
            {
                Manager.ConnectToPhoton();
            }
        }
        else
        {
            if (GUILayout.Button("Disconnect"))
            {
                PhotonNetwork.Disconnect();
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Room Management", EditorStyles.boldLabel);

        NewRoomCode = EditorGUILayout.TextField("Room Code", NewRoomCode);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Join Room"))
        {
            if (!string.IsNullOrEmpty(NewRoomCode))
            {
                PhotonNetwork.JoinRoom(NewRoomCode);
            }
            else
            {
                Debug.LogError("Please Enter A Room Code");
            }
        }

        if (GUILayout.Button("Create Room"))
        {
            NewRoomCode = Manager.GenerateRoomCode();
            PhotonNetwork.CreateRoom(NewRoomCode, new RoomOptions { MaxPlayers = (byte)Manager.MaxPlayers });
        }
        EditorGUILayout.EndHorizontal();

        if (PhotonNetwork.InRoom && GUILayout.Button("Leave Room"))
        {
            PhotonNetwork.LeaveRoom();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Player Settings", EditorStyles.boldLabel);

        NewPlayerName = EditorGUILayout.TextField("Player Name", NewPlayerName);
        NewPlayerColor = EditorGUILayout.ColorField("Player Color", NewPlayerColor);

        if (GUILayout.Button("Update Player Info"))
        {
            Manager.SetPlayerName(NewPlayerName);
            Manager.SetPlayerColor(NewPlayerColor);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Cosmetics", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        NewCosmeticType = EditorGUILayout.TextField("Type", NewCosmeticType);
        NewCosmeticValue = EditorGUILayout.TextField("Value", NewCosmeticValue);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Add/Update Cosmetic"))
        {
            if (!string.IsNullOrEmpty(NewCosmeticType))
            {
                TempCosmetics[NewCosmeticType] = NewCosmeticValue;
                Manager.UpdateAllCosmetics(TempCosmetics);
            }
        }

        EditorGUILayout.LabelField("Current Cosmetics:");
        foreach (var cosmetic in TempCosmetics)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(cosmetic.Key, GUILayout.Width(100));
            EditorGUILayout.LabelField(cosmetic.Value);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                TempCosmetics.Remove(cosmetic.Key);
                Manager.UpdateAllCosmetics(TempCosmetics);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        if (PhotonNetwork.InRoom)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Players In Room", EditorStyles.boldLabel);

            foreach (var player in PhotonNetwork.PlayerList)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(player.NickName);
                if (player.IsLocal)
                {
                    EditorGUILayout.LabelField("(You)", GUILayout.Width(40));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Debug Log", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(Manager.GetDebugLog(), GUILayout.Height(100));
        if (GUILayout.Button("Clear Debug Log"))
        {
            Manager.ClearDebugLog();
        }
        EditorGUILayout.EndVertical();
    }
}
#endif