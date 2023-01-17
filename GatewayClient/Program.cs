using System.Diagnostics;
using Husty.SkywayGateway;

var key = "API_KEY";
var id = "client";
var target = "server";

var gstPath = "C:\\gstreamer\\1.0\\mingw_x86_64\\bin\\gst-launch-1.0";

var peer = await Peer.CreateNewAsync(key, id);
var dataChannel = await peer.CreateDataChannelAsync();
var mediaChannel = await peer.CreateMediaChannelAsync();

Console.WriteLine($"create peer: {id}"); 
Console.WriteLine("now calling server ...");
Console.WriteLine();

var stream = await dataChannel.CallConnectionAsync(target);
var mediaInfo = await mediaChannel.CallConnectionAsync(target);
Console.WriteLine($"connected with {dataChannel.RemotePeerId}");
Console.WriteLine($"Local Video EndPoint: {mediaInfo.LocalVideoEP}");
Console.WriteLine($"Remote Video EndPoint: {mediaInfo.RemoteVideoEP}");

using var process = Process.Start(new ProcessStartInfo()
{
    FileName = gstPath,
    Arguments =
        $"-v udpsrc port={mediaInfo.LocalVideoEP.Port} " +
        $"! application/x-rtp,media=video " +
        $"! queue ! rtph264depay ! avdec_h264 ! videoconvert ! autovideosink"
});

var cts = new CancellationTokenSource();
var task = Task.Run(() =>
{
    while (!cts.IsCancellationRequested)
        Console.WriteLine(stream.ReadString());
});

Console.WriteLine("Press ESC key to exit...");
while (Console.ReadKey().Key is not ConsoleKey.Escape) ;

cts.Cancel();
await peer.DisposeAsync();

Console.ReadKey();