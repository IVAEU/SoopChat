using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageUI : MonoBehaviour
{
    [SerializeField] private SoopMessageReceiver messageReceiver;
    [Space]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private Transform chatContainer;
    private TextMeshProUGUI[] _chatText;

    private void Start()
    {
        SetNickname(); 
        
        _chatText = new TextMeshProUGUI[chatContainer.childCount];
        for (int i = 0; i < chatContainer.childCount; i++)
        {
            _chatText[i] = chatContainer.GetChild(i).GetComponent<TextMeshProUGUI>();
        }
        messageReceiver.OnMessageReceive += AddMessage;
    }

    private async void SetNickname()
    {
        string nickname = await messageReceiver.GetUserNickName();
        string broadcastName = await messageReceiver.GetUserBroadcastName();
        nicknameText.text = $"{nickname}: {broadcastName}";
    }

    private void AddMessage(string[] str)
    {
        for (int i = 0; i < chatContainer.childCount - 1; i++)
        {
            _chatText[i].text = _chatText[i + 1].text;
        }
        _chatText[^1].text = $"{str[1]}: {str[2]}";
    }
}
