using Artnet;
using Artnet.Models;
using Artnet.Packets;
using System.Net;

byte[] dmxData = new byte[511];
var artnet = new ArtNetSocket();
artnet.EnableBroadcast = true;

Console.WriteLine(artnet.BroadcastAddress.ToString());
artnet.Open(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("255.255.255.0"));

artnet.NewPacket += (sender, e) =>
{
    if (e.Packet.OpCode == ArtNetOpCode.Dmx)
    {
        var packet = e.Packet as ArtNetDmxPacket;
        Console.Clear();

        if (packet.DmxData != dmxData)
        {
            Console.WriteLine("New Packet");
            for (var i = 0; i < packet.DmxData.Length; i++)
            {
                if (packet.DmxData[i] != 0)
                    Console.WriteLine(i + " = " + packet.DmxData[i]);
            }

            ;

            dmxData = packet.DmxData;
        }
    }
};

Console.ReadLine();
