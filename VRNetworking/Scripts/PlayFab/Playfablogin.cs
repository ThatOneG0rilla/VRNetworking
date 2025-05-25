using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Photon.Pun;
using System;
using System.Globalization;
using Photon.Voice;
public class Playfablogin : MonoBehaviour
{
    [Header("COSMETICS")]
    public static Playfablogin instance;
    public string MyPlayFabID;
    public string CatalogName;
    public List<GameObject> specialitems;
    public List<GameObject> disableitems;
    [Header("CURRENCY")]
    public string CurrencyCode = "HS";
    public string CurrencyName;
    public TextMeshPro currencyText;
    [SerializeField]
    public int coins;
    [Header("BANNED")]
    public string bannedscenename;
    [Header("TITLE DATA")]
    public TextMeshPro MOTDText;
    [Header("BAN ITEMS")]
    public List<GameObject> BannedEnableItems;
    public List<GameObject> BannedDisableItems;
    public TextMeshPro banString;
    public TextMeshPro BanReason;
    public TextMeshPro BanTime;
    [Header("Security")]
    public bool PhotonXPlayFab;
    public void Awake()
    {
        instance = this;
    }
    public void Login()
    {
        instance = this;

        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, instance.OnLoginSuccess, instance.OnError);
    }
    public void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("logging in");
        MyPlayFabID = result.PlayFabId;

        if (PhotonXPlayFab)
        {
            PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
            {
                PhotonApplicationId = PhotonAppSettings.Instance.AppSettings.AppIdRealtime
            }, AuthenticateWithPhoton, OnError);
        }

        GetAccountInfoRequest InfoRequest = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(InfoRequest, AccountInfoSuccess, OnError);
        GetVirtualCurrencies();
        GetMOTD();

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
            OnError(error);
        });
    }
    public void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj)
    {
        NetworkManager.ConnectAuth(MyPlayFabID, obj.PhotonCustomAuthenticationToken);
    }
    public void AccountInfoSuccess(GetAccountInfoResult result)
    {
        MyPlayFabID = result.AccountInfo.PlayFabId;

        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        (result) =>
        {
            foreach (var item in result.Inventory)
            {
                if (item.CatalogVersion == CatalogName)
                {
                    for (int i = 0; i < specialitems.Count; i++)
                    {
                        if (specialitems[i].name == item.ItemId)
                            specialitems[i].SetActive(true);
                        else
                            specialitems[i].SetActive(false);
                    }
                    for (int i = 0; i < disableitems.Count; i++)
                    {
                        if (disableitems[i].name == item.ItemId)
                            disableitems[i].SetActive(false);
                        else
                            specialitems[i].SetActive(true);
                    }
                }
            }
        },
        (error) =>
        {
            Debug.LogError(error.GenerateErrorReport());
        });
    }
    public void GetVirtualCurrencies()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnGetUserInventorySuccess, OnError);
    }
    void OnGetUserInventorySuccess(GetUserInventoryResult result)
    {
        coins = result.VirtualCurrency[CurrencyCode];
        currencyText.text = "You have " + coins.ToString() + " " + CurrencyName;
    }
    private void OnError(PlayFabError error)
    {
        if (error.Error == PlayFabErrorCode.AccountBanned)
        {
            PhotonNetwork.Disconnect();

            for (int i = 0; i < BannedEnableItems.Count; i++)
                BannedEnableItems[i].SetActive(true);
            for (int i = 0; i < BannedDisableItems.Count; i++)
                BannedDisableItems[i].SetActive(false);

            foreach (var item in error.ErrorDetails)
            {
                BanReason.text = item.Key;

                string UnbanTime = item.Value[0];
                if (DateTime.TryParseExact(UnbanTime, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime UnBanDate))
                {
                    banString.text = "Your Account has been temporarily Banned.";
                    BanTime.text = ((int)Math.Floor(Math.Abs((UnBanDate - DateTime.UtcNow).TotalHours))).ToString() + " hours remain.";
                }
                else
                {
                    banString.text = "Your Account has been permanently Banned.";
                    BanTime.text = null;
                }
            }
        }
    }
    public void GetMOTD()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), MOTDGot, OnError);
    }
    public void MOTDGot(GetTitleDataResult result)
    {
        if (result.Data == null || result.Data.ContainsKey("MOTD") == false)
        {
            Debug.Log("No MOTD");
            return;
        }
        MOTDText.text = result.Data["MOTD"];
    }
}