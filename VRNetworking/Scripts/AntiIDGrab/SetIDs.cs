using UnityEngine;
using Photon.Pun;
using PlayFab;
public class SetIDs : MonoBehaviour
{
    public static string AppVersion;
    public static string FixedRegion;
    public static string DevRegion;
    public static string TitleId;
    void Awake()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = AppVersion;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = FixedRegion;
        PhotonNetwork.PhotonServerSettings.DevRegion = DevRegion;
        PlayFabSettings.TitleId = TitleId;
    }
    void OnApplicationQuit()
    {
        var Obf = "Stop looking for my ids pal";
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = Obf;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = Obf;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat = Obf;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = Obf;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = Obf;
        PhotonNetwork.PhotonServerSettings.DevRegion = Obf;
        PlayFabSettings.TitleId = Obf;
    }
    void OnDestroy()
    {
        var Obf = "Stop looking for my ids pal";
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = Obf;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = Obf;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat = Obf;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = Obf;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = Obf;
        PhotonNetwork.PhotonServerSettings.DevRegion = Obf;
        PlayFabSettings.TitleId = Obf;
    }
    void OnDisable()
    {
        var Obf = "Stop looking for my ids pal";
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = Obf;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = Obf;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat = Obf;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = Obf;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = Obf;
        PhotonNetwork.PhotonServerSettings.DevRegion = Obf;
        PlayFabSettings.TitleId = Obf;
    }
}