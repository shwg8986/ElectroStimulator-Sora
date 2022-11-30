using System;

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SoraDataChannel : MonoBehaviour
{
    Sora sora;
    [SerializeField] string ChannelID;
    [SerializeField] string SignalingUrl;
    [SerializeField] string dataChannelLabel;

    List<Sora.DataChannel> dataChannels;

    //public int[] recvIntData = new int[300];
    public int[,,] recvIntMultiData = new int[3, 10, 10];
    public static int[,,] pressureDistribution;

    // Start is called before the first frame update
    void Start()
    {
        dataChannels = new List<Sora.DataChannel>();
        dataChannels.Add(new Sora.DataChannel()
            { Direction = Sora.Direction.Sendrecv, Label = dataChannelLabel});
        sora = new Sora();
        Sora.Config config = new Sora.Config()
        {
            ChannelId = ChannelID,
            SignalingUrl = SignalingUrl,
            Multistream = true,
            Role = Sora.Role.Recvonly,
            Video = false,
            Audio = true,
            UnityAudioOutput = false,
            UnityAudioInput = true,
            DataChannelSignaling = true,
            EnableDataChannelSignaling = true,
            UnityCamera = Camera.main,
            CapturerType = Sora.CapturerType.UnityCamera,
            UnityCameraRenderTargetDepthBuffer = 16,
            DataChannels = dataChannels,
            Insecure = true
        };
           sora.OnNotify = (message) => { Debug.Log(message); };

        //sora.SendMessage(label, buf);

        //sora.OnMessage = (label, data) =>
        //{
        //};
        sora.Connect(config);
        StartCoroutine(GetStats());
        StartCoroutine(receiveValue());

    }
    public void SendMessage(int[] data)
    {
        var spanData = MemoryMarshal.Cast<int, byte>(data.AsSpan());
        sora.SendMessage(dataChannelLabel, spanData.ToArray());
    }
    bool isStart = false;
    // Update is called once per frame
    int send_interval = 0;

    public int[,,] ReceiveMessage(byte[] data)
    {
        //éÛêM
        byte[] recvRawData = data;
        var recvIntData = new List<int>();
        for (int i = 0; i <= data.Length - 4; i += 4)
        {
            byte[] bytes4 =  recvRawData.AsSpan().Slice(i,4).ToArray();
            int x = BitConverter.ToInt32(bytes4, 0);
            recvIntData.Add(x);
        }
        //Debug.Log(recvIntData.Count);

        //int[] recvIntData = MemoryMarshal.Cast<byte, int>(recvRawData.AsSpan()).ToArray();

        //Debug.Log("recvIntdata" + recvIntData.Length);
        //Debug.Log("recvIntdata" + recvIntData[10] + recvIntData[100] + recvIntData[200]);

        int count = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                for (int k = 0; k < 10; k++)
                {
                    recvIntMultiData[i, j, k] = recvIntData[count];
                    //Debug.Log(recvIntData[count]);
                    count++;
                }
            }
        }
        //Debug.Log("Length : " +recvIntMultiData.Length);
        
        //Debug.Log($"recvIntMultiData : {recvIntMultiData[0,5,5]} , {recvIntMultiData[1,5,5]} , {recvIntMultiData[2,5,5]}");
        return recvIntMultiData;


        //int recvIntData = new int[3, 4, 4] { recvSpanData.ToArray()}
    }


    IEnumerator GetStats()
    {
        while (true)
        {
            yield return new WaitForSeconds(10);


            //sora.GetStats((stats) => { 
            //    Debug.LogFormat("GetStats: {0}", stats);
            //});
        }
    }

    IEnumerator receiveValue()
    {
        while (true)
        {
            sora.DispatchEvents();
            sora.OnRender();
            sora.OnMessage = (dataChannels, data) =>
            {
                //Debug.Log(data.Length);
                //Debug.Log(data[1] + data[100]+ data[200]);
                pressureDistribution = ReceiveMessage(data);
                //Debug.Log($"pressureDistribution : {pressureDistribution[0, 5, 5]} , {pressureDistribution[1, 5, 5]} , {pressureDistribution[2, 5, 5]}");
            };

            //Debug.Log(pressureDistribution);

            yield return new WaitForSeconds(.1f);
        }

       
    }
}