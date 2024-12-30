using System;
using System.Collections.Generic;
using System.Text;

public class PacketBuilder
{
    // 패킷 배열을 만드는 함수
    private static List<string> BuildPacketArray(List<string> arr)
    {
        List<string> result = new List<string>();

        // 각 문자열에 \f 문자를 추가
        foreach (var item in arr)
        {
            result.Add("\f" + item);
        }
        // 마지막에 \f를 추가
        result.Add("\f");
        return result;
    }

    // 문자열 배열을 바이트로 변환하는 함수
    private static byte[] MakeBytes(List<string> arr)
    {
        // 모든 문자열을 결합한 후 UTF-8로 바이트 배열로 변환
        string combined = string.Join("", arr);
        return Encoding.UTF8.GetBytes(combined);
    }

    // 헤더를 만드는 함수
    private static List<string> MakeHeader(int svc, int bodyLength)
    {
        List<string> header = new List<string>
        {
            "\u001b",     // 이스케이프 문자
            "\t",         // 탭 문자
            svc.ToString("D4"),       // 서비스 번호를 4자리로 고정
            bodyLength.ToString("D6"),// 바디 길이를 6자리로 고정
            "00"          // 고정 값 00
        };
        return header;
    }

    // 패킷을 생성하는 함수
    public static byte[] CreatePacket(int svc, List<string> data)
    {
        // 바디 생성
        List<string> packetArray = BuildPacketArray(data);
        byte[] body = MakeBytes(packetArray);

        // 헤더 생성
        List<string> headerArray = MakeHeader(svc, body.Length);
        byte[] header = MakeBytes(headerArray);

        // 헤더와 바디 결합 후 반환
        byte[] packet = new byte[header.Length + body.Length];
        Buffer.BlockCopy(header, 0, packet, 0, header.Length);
        Buffer.BlockCopy(body, 0, packet, header.Length, body.Length);

        return packet;
    }
}

public enum ServiceCode
{
    SVC_KEEPALIVE = 0,
    SVC_LOGIN = 1,
    SVC_JOINCH = 2,
    SVC_QUITCH = 3,
    SVC_CHUSER = 4,
    SVC_CHATMESG = 5,
    SVC_SETCHNAME = 6,
    SVC_SETBJSTAT = 7,
    SVC_SETDUMB = 8,
    SVC_DIRECTCHAT = 9,
    SVC_NOTICE = 10,
    SVC_KICK = 11,
    SVC_SETUSERFLAG = 12,
    SVC_SETSUBBJ = 13,
    SVC_SETNICKNAME = 14,
    SVC_SVRSTAT = 15,
    SVC_RELOADHOST = 16,
    SVC_CLUBCOLOR = 17,
    SVC_SENDBALLOON = 18,
    SVC_ICEMODE = 19,
    SVC_SENDFANLETRTRER = 20,
    SVC_ICEMODE_EX = 21,
    SVC_GET_ICEMODE_RELAY = 22,
    SVC_SLOWMODE = 23,
    SVC_RELOADBURNLEVEL = 24,
    SVC_BLINDKICK = 25,
    SVC_MANAGERCHAT = 26,
    SVC_APPENDDATA = 27,
    SVC_BASEBALLEVENT = 28,
    SVC_PAIDITEM = 29,
    SVC_TOPFAN = 30,
    SVC_SNSMESSAGE = 31,
    SVC_SNSMODE = 32,
    SVC_SENDBALLOONSUB = 33,
    SVC_SENDFANLETRTRERSUB = 34,
    SVC_TOPFANSUB = 35,
    SVC_BJSTICKERITEM = 36,
    SVC_CHOCOLATE = 37,
    SVC_CHOCOLATESUB = 38,
    SVC_TOPCLAN = 39,
    SVC_TOPCLANSUB = 40,
    SVC_SUPERCHAT = 41,
    SVC_UPDATETICKET = 42,
    SVC_NOTIGAMERANKER = 43,
    SVC_STARCOIN = 44,
    SVC_SENDQUICKVIEW = 45,
    SVC_ITEMSTATUS = 46,
    SVC_ITEMUSING = 47,
    SVC_USEQUICKVIEW = 48,
    SVC_NOTIFY_POLL = 50,
    SVC_CHATBLOCKMODE = 51,
    SVC_BDM_ADDBLACKINFO = 52,
    SVC_SETBROADINFO = 53,
    SVC_BAN_WORD = 54,
    SVC_SENDADMINNOTICE = 58,
    SVC_FREECAT_OWNER_JOIN = 65,
    SVC_BUYGOODS = 70,
    SVC_BUYGOODSSUB = 71,
    SVC_SENDPROMOTION = 72,
    SVC_NOTIFY_VR = 74,
    SVC_NOTIFY_MOBBROAD_PAUSE = 75,
    SVC_KICK_AND_CANCEL = 76,
    SVC_KICK_USERLIST = 77,
    SVC_ADMIN_CHUSER = 78,
    SVC_CLIDOBAEINFO = 79,
    SVC_VOD_BALLOON = 86,
    SVC_ADCON_EFFECT = 87,
    SVC_SVC_KICK_MSG_STATE = 90,
    SVC_FOLLOW_ITEM = 91,
    SVC_ITEM_SELL_EFFECT = 92,
    SVC_FOLLOW_ITEM_EFFECT = 93,
    SVC_TRANSLATION_STATE = 94,
    SVC_TRANSLATION = 95,
    SVC_GIFT_TICKET = 102,
    SVC_VODADCON = 103,
    SVC_BJ_NOTICE = 104,
    SVC_VIDEOBALLOON = 105,
    SVC_STATION_ADCON = 107,
    SVC_SENDSUBSCRIPTION = 108,
    SVC_OGQ_EMOTICON = 109,
    SVC_ITEM_DROPS = 111,
    SVC_VIDEOBALLOON_LINK = 117,
    SVC_OGQ_EMOTICON_GIFT = 118,
    SVC_AD_IN_BROAD_JSON = 119,
    SVC_GEM_ITEMSEND = 120,
    SVC_MISSION = 121,
    SVC_LIVE_CAPTION = 122,
    SVC_MISSION_SETTLE = 125,
    SVC_SET_ADMIN_FLAG = 126,
}