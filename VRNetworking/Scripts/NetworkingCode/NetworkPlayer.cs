using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
public class NetworkPlayer : MonoBehaviourPun
{
    public static NetworkPlayer LocalPlayer;

    [Header("Objects")]
    public Transform Head;
    public Transform HandL;
    public Transform HandR;
    public List<MeshRenderer> ColorMeshes;

    public List<Slot> Slots = new();
    public int ColorIndex;

    [Header("Other")]
    public TextMeshPro NameText;

    [HideInInspector] public PhotonView View;
    void Awake()
    {
        View = photonView;
        if (View.IsMine) 
            LocalPlayer = this;

        DontDestroyOnLoad(gameObject);
        Refresh();
    }
    void Update()
    {
        if (!View.IsMine)
            return;

        Head.position = NetworkManager.Instance.Head.position;
        Head.rotation = NetworkManager.Instance.Head.rotation;

        HandR.position = NetworkManager.Instance.HandR.position;
        HandR.rotation = NetworkManager.Instance.HandR.rotation;

        HandL.position = NetworkManager.Instance.HandL.position;
        HandL.rotation = NetworkManager.Instance.HandL.rotation;
    }

    [PunRPC]
    public void RPCRefreshPlayerValues() => Refresh();
    void Refresh()
    {
        NameText.text = photonView.Owner.NickName;

        if (photonView.Owner.CustomProperties.TryGetValue("Colour", out var colorJson))
        {
            var Color = JsonUtility.FromJson<Color>((string)colorJson);
            foreach (var Mesh in ColorMeshes)
                Mesh.materials[ColorIndex].color = Color;
        }

        if (photonView.Owner.CustomProperties.TryGetValue("Cosmetics", out var data) && data is Dictionary<string, string> CosmeticData)
        {
            foreach (var Pair in CosmeticData)
            {
                foreach (var Slot in Slots)
                {
                    if (Slot.Name != Pair.Key) continue;
                    foreach (Transform Obj in Slot.Items)
                        Obj.gameObject.SetActive(Obj.name == Pair.Value);
                }
            }
        }
    }
    [Serializable]
    public class Slot
    {
        public string Name;
        public Transform Items;
    }
}