using System;
using System.Net;
using Godot;
using LXProtocols.Acn.Rdm;
using LXProtocols.Acn.Sockets;
using LXProtocols.ArtNet;
using LXProtocols.ArtNet.Packets;
using LXProtocols.ArtNet.Sockets;

namespace GodotProject.scripts;

public class CaptureArtNetSignal : IDisposable
{

    private const uint DmxLengthPerUniverse = 512;
    
    private readonly ArtNetSocket socket = new (UId.Empty);
    private byte[] dmxData;

    public delegate void NotifyNewPacket(in byte[] signals);
    public event NotifyNewPacket OnNotifySignals;
    
    public CaptureArtNetSignal(IPAddress ipAddress, IPAddress subnetMaskAddress, uint universe)
    {
        dmxData = new byte[DmxLengthPerUniverse * universe];

        socket.NewPacket += SocketOnNewPacket;
        socket.UnhandledException += SocketOnUnhandledException;

        socket.Open(ipAddress, subnetMaskAddress);
    }

    private void SocketOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        GD.PrintErr(exception);
    }

    private void SocketOnNewPacket(object sender, NewPacketEventArgs<ArtNetPacket> e)
    {
        switch (e.Packet.OpCode) {
            case ArtNetOpCodes.Dmx:
                var dmxPacket = e.Packet as ArtNetDmxPacket;
                Array.Copy(dmxPacket.DmxData, 0, dmxData, DmxLengthPerUniverse * dmxPacket.Universe, DmxLengthPerUniverse);
                OnNotifySignals?.Invoke(dmxData);
                break;
        }
    }

    void IDisposable.Dispose()
    {
        socket.UnhandledException -= SocketOnUnhandledException;
        socket.NewPacket -= SocketOnNewPacket;
        socket.Close();
        socket.Dispose();
    }
}