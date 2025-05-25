using System.Collections.Generic;
using UnityEngine;
public class VRNetworkValueSaver : MonoBehaviour
{
    public static void SaveDictionary(string Key, Dictionary<string, string> Map)
    {
        PlayerPrefs.SetString(Key, string.Join(",", Map.Keys));
        foreach (KeyValuePair<string, string> Entry in Map)
            PlayerPrefs.SetString(Key + Entry.Key, Entry.Value);
    }
    public static Dictionary<string, string> GetDictionary(string Key)
    {
        string[] Keys = PlayerPrefs.GetString(Key).Split(',');
        Dictionary<string, string> Map = new();
        foreach (string K in Keys)
            Map[K] = PlayerPrefs.GetString(Key + K);
        return Map;
    }
}