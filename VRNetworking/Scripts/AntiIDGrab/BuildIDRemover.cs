#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Photon.Pun;
using PlayFab;
public class BuildIDRemover : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0;
    private static string Name = "Stop looking for my ids pal";
    private static iSDNcwkV IdStorage => AssetDatabase.LoadAssetAtPath<iSDNcwkV>("Assets/Resources/IDStorage.asset");
    public void OnPreprocessBuild(BuildReport report)
    {
        IdStorage.Version = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion;
        IdStorage.Region = PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion;
        IdStorage.Region = PhotonNetwork.PhotonServerSettings.DevRegion;
        IdStorage.TitleID = PlayFabSettings.TitleId;

        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = Name;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = Name;
        PhotonNetwork.PhotonServerSettings.DevRegion = Name;
        PlayFabSettings.TitleId = Name;

        EditorUtility.SetDirty(IdStorage);
        AssetDatabase.SaveAssets();
    }
    public void OnPostprocessBuild(BuildReport report)
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = IdStorage.Version;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = IdStorage.Region;
        PhotonNetwork.PhotonServerSettings.DevRegion = IdStorage.Region;
        PlayFabSettings.TitleId = IdStorage.TitleID;
    }
}
#endif