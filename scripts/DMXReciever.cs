using System;
using System.Net;
using System.Text;
using Godot;

namespace GodotProject.scripts;

public partial class DMXReciever : Node
{

	[Export]
	private string ipAddressString = "127.0.0.1";
	[Export]
	private string subnetMaskAddressString = "255.0.0.0";
	[Export]
	private uint universe = 4;

	private byte[] signals;
	private StringBuilder stringBuilder = new();
	private CaptureArtNetSignal captureArtNetSignal;

	public override void _Ready()
	{
		var ipAddress = IPAddress.Parse(ipAddressString);
		var subnetMaskAddress = IPAddress.Parse(subnetMaskAddressString);
		captureArtNetSignal = new CaptureArtNetSignal(ipAddress, subnetMaskAddress, universe);
		captureArtNetSignal.OnNotifySignals += OnNotifySignals;
	}

	private void OnNotifySignals(in byte[] signals)
	{
		this.signals = signals;
		stringBuilder.Clear();
		for (var i = 0; i < this.signals.Length; i++)
		{
			stringBuilder.AppendLine($"{i}: {signals[i]}");
		}
		GD.Print(stringBuilder.ToString());
	}

	public override void _ExitTree()
	{
		captureArtNetSignal.OnNotifySignals -= OnNotifySignals;
		(captureArtNetSignal as IDisposable).Dispose();
	}
}