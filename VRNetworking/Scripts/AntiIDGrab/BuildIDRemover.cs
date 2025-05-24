#if UNITY_EDITOR
using Photon.Pun;
using PlayFab;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
public class BuildIDRemover : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 10;
    private static string AppVersion;
    private static string FixedRegion;
    private static string DevRegion;
    private static string TitleId;
    private static readonly string Obfuscate = "Stop looking for my ids pal";
    public void OnPreprocessBuild(BuildReport Report)
    {
        AppVersion = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion;
        FixedRegion = PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion;
        TitleId = PlayFabSettings.TitleId;

        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = Obfuscate;
        PhotonNetwork.PhotonServerSettings.DevRegion = Obfuscate;
        PlayFabSettings.TitleId = Obfuscate;

        SetIDs.AppVersion = AppVersion;
        SetIDs.DevRegion = DevRegion;
        SetIDs.TitleId = TitleId;
    }
    public void OnPostprocessBuild(BuildReport Report)
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = AppVersion;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = FixedRegion;
        PhotonNetwork.PhotonServerSettings.DevRegion = DevRegion;
        PlayFabSettings.TitleId = TitleId;
    }
}
#endif