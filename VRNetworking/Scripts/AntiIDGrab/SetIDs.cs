using UnityEngine;
using Photon.Pun;
using PlayFab;
using System.Collections;
public class SetIDs : MonoBehaviour
{
    [SerializeField] private iSDNcwkV Storage;
#if !UNITY_EDITOR
    void Start()
    {
        StartCoroutine(Delay());
    }
    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(1f);

        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = Storage.Version;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = Storage.Region;
        PhotonNetwork.PhotonServerSettings.DevRegion = Storage.DevRegion;
        PlayFabSettings.TitleId = Storage.TitleID;
        Playfablogin.instance.Login();
    }
    void ObfuscateAll()
    {
        string Name = "Stop looking for my ids pal";
        var settings = PhotonNetwork.PhotonServerSettings.AppSettings;

        settings.AppIdRealtime = Name;
        settings.AppIdVoice = Name;
        settings.AppIdChat = Name;
        settings.AppVersion = Name;
        settings.FixedRegion = Name;
        PhotonNetwork.PhotonServerSettings.DevRegion = Name;
        PlayFabSettings.TitleId = Name;
    }
    void OnApplicationQuit() => ObfuscateAll();
    void OnDestroy() => ObfuscateAll();
    void OnDisable() => ObfuscateAll();
#endif
    void Start()
    {
        Playfablogin.instance.Login();
    }
}