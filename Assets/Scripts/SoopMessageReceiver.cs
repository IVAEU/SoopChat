using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

public class SoopMessageReceiver : MonoBehaviour
{
    [Serializable]
    public class StationResponse
    {
        [Serializable]
        public class Broad
        {
            public int broad_grade;
            public int broad_no;
            public string broad_title;
            public int current_sum_viewer;
            public bool is_password;
            public string user_id;
        }
        
        [Serializable]
        public class Station
        {
            public string user_nick;
        }

        [CanBeNull] public Broad broad;
        [CanBeNull] public Station station;
    }

    [Serializable]
    public class BroadcastResponse
    {
        [Serializable]
        public class Channel
        {
            public string CHATNO;
            public string CHDOMAIN;
            public string CHPT;
            public string FTK;
        }

        public Channel CHANNEL;
    }
    
    private ClientWebSocket _ws;

    public string userID = "";

    public Action<string[]> OnMessageReceive;
    
    
    async void Start()
    {
        string stationData = await GetStation(userID);
        StationResponse stationJson = JsonUtility.FromJson<StationResponse>(stationData);
        if (stationJson.broad.user_id == null)
        {
            Debug.Log($"{userID}는 방송 중이 아닙니다");
            return;
        }
        else
        {
            Debug.Log($"{stationJson.station.user_nick}의 방송입니다");
        }

        //await LoginSoop(); //Cookie Missing
        
        //Get Broadcast Uri
        BroadcastResponse channelJson = await GetLiveInfoJson();
        string serverUriText = GetServerUriText(channelJson);
        Uri serverUri = new Uri($"{serverUriText}");
        
        //Setting Client WebSocket
        _ws = new ClientWebSocket();
        _ws.Options.KeepAliveInterval = TimeSpan.FromMinutes(1);
        _ws.Options.AddSubProtocol("chat");
        
        //Connect To Server WebSocket
        await _ws.ConnectAsync(serverUri, CancellationToken.None);
        Debug.Log("WebSocket connected!");
        
        //HandShake 
        await SendHandshake(channelJson.CHANNEL);
        
        //ReceiveMessages (while loop)
        await ReceiveMessages();
    }
    
    private async Task<string> GetStation(string channalId)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get($"https://chapi.sooplive.co.kr/api/{channalId}/station");
        webRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
        await webRequest.SendWebRequest();
        return webRequest.downloadHandler.text;
    } 
    
    private async Task LoginSoop()
    {
        WWWForm form = new WWWForm();
        form.AddField("szWork",$"login");
        UnityWebRequest webRequest = UnityWebRequest.Post($"https://login.sooplive.co.kr/app/LoginAction.php", form);
        webRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
        await webRequest.SendWebRequest();
        Dictionary<string, string> header = webRequest.GetResponseHeaders();
        foreach (var h in header)
        {
            Debug.Log($"{h.Key}: {h.Value}");
        }
    }

    private async Task<BroadcastResponse> GetLiveInfoJson()
    {
        WWWForm form = new WWWForm();
        form.AddField("bid",$"{userID}");
        form.AddField("type","live");
        form.AddField("player_type","html5");
        
        UnityWebRequest liveInfo = UnityWebRequest.Post($"https://live.sooplive.co.kr/afreeca/player_live_api.php?bjid={userID}", form);
        await liveInfo.SendWebRequest();
        return JsonUtility.FromJson<BroadcastResponse>(liveInfo.downloadHandler.text);
    }
    
    private string GetServerUriText(BroadcastResponse channelJson)
    {
        string serverUriText = $"wss://{channelJson.CHANNEL.CHDOMAIN}:{int.Parse(channelJson.CHANNEL.CHPT) + 1}/Websocket/{userID}";
        Debug.Log($"{serverUriText}");
        return serverUriText;
    }
    
    private async Task SendHandshake(BroadcastResponse.Channel channel)
    {
        //First HandShake: Login
        string handshakeLogin = "GwkwMDAxMDAwMDA2MDAMDAwxNgw=";
        ArraySegment<byte> bytesToSend1 = new ArraySegment<byte>(DecodeBase64(handshakeLogin));
        await _ws.SendAsync(bytesToSend1, WebSocketMessageType.Binary, true, CancellationToken.None);
        Debug.Log("Handshake Login sent");
        
        //Receive
        ArraySegment<byte> bytesToReceive = new ArraySegment<byte>(new byte[1024]);
        await _ws.ReceiveAsync(bytesToReceive.Array, CancellationToken.None);
        Debug.Log("Handshake message receive");
        Debug.Log($"{EncodeBase64(bytesToReceive.Array)}");

        //Second HandShake: Join
        ServiceCode serviceCode = ServiceCode.SVC_JOINCH;
        Byte[] handshakeJoinByte = PacketBuilder.CreatePacket((int)serviceCode, new List<string>() 
        { 
            channel.CHATNO, 
            channel.FTK,
            "0",
            "",
            "", 
        });
        await _ws.SendAsync(handshakeJoinByte, WebSocketMessageType.Binary, true, CancellationToken.None);
        Debug.Log("Handshake Join sent");
    }

    private async Task ReceiveMessages()
    {
        ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);

        while (_ws.State == WebSocketState.Open)
        {
            try
            {
                WebSocketReceiveResult result = await _ws.ReceiveAsync(buffer, CancellationToken.None);
                if(buffer == null) return;
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    Debug.Log("Received: " + message);
                }
                else if(result.MessageType == WebSocketMessageType.Binary)
                {
                    List<List<Byte>> receiveResult = new List<List<Byte>>();
                    receiveResult.Add(new List<Byte>());
                    for (int i = 0; i < result.Count; i++)
                    {
                        if ((char)buffer.Array[i] == '\f')
                        {
                            receiveResult.Add(new List<Byte>());  
                        }
                        else
                        {
                            receiveResult[^1].Add(buffer.Array[i]);
                        }
                    }

                    string chatText = Encoding.UTF8.GetString(receiveResult[1].ToArray(), 0, receiveResult[1].Count);
                    string idText = Encoding.UTF8.GetString(receiveResult[2].ToArray(), 0, receiveResult[2].Count);
                    string nicknameText = Encoding.UTF8.GetString(receiveResult[6].ToArray(), 0, receiveResult[6].Count);

                    if (chatText != "-1" && chatText != "1" && !chatText.Contains("|"))
                    {
                        OnMessageReceive?.Invoke(new [] {idText, nicknameText, chatText});

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < receiveResult.Count; ++i)
                        {
                            sb.Append(Encoding.UTF8.GetString(receiveResult[i].ToArray(), 0, receiveResult[i].Count));
                            sb.Append("|");
                        }
                        
                        Debug.Log($"Received: {sb}"); 
                    }
                }
            }
            catch (Exception ex)
            {
                //Debug.Log("Error while receiving: " + ex.Message);
            }
        }
    }

    public async Task<string> GetUserNickName()
    {
        string stationData = await GetStation(userID);
        StationResponse stationJson = JsonUtility.FromJson<StationResponse>(stationData);
        return stationJson.station.user_nick;
    }
    
    public async Task<string> GetUserBroadcastName()
    {
        string stationData = await GetStation(userID);
        StationResponse stationJson = JsonUtility.FromJson<StationResponse>(stationData);
        return stationJson.broad.broad_title;
    }
    
    private string EncodeBase64(Byte[] plainBytes)
    {
        return Convert.ToBase64String(plainBytes);
    }
    
    private Byte[] DecodeBase64(string base64PlainText)
    {
        return Convert.FromBase64String(base64PlainText);
    }
    
    private void OnApplicationQuit()
    {
        _ws?.Dispose();
    }
}