using System.Diagnostics;
using Husty.SkywayGateway;

var key = "API_KEY";
var id = "server";

var gstPath = $"C:\\gstreamer\\1.0\\msvc_x86_64\\bin\\gst-launch-1.0";

var peer = await Peer.CreateNewAsync(key, id);
var dataChannel = await peer.CreateDataChannelAsync();
var mediaChannel = await peer.CreateMediaChannelAsync();

Console.WriteLine($"create peer: {id}");
Console.WriteLine("now listening for call by client ...");
Console.WriteLine();

var stream = await dataChannel.ListenAsync();
var mediaInfo = await mediaChannel.ListenAsync();
Console.WriteLine($"connected with {dataChannel.RemotePeerId}");
Console.WriteLine($"Local Video EndPoint: {mediaInfo.LocalVideoEP}");
Console.WriteLine($"Remote Video EndPoint: {mediaInfo.RemoteVideoEP}");

using var process = Process.Start(new ProcessStartInfo()
{
    FileName = gstPath,
    Arguments =
        $"-v ksvideosrc ! decodebin " +
        $"! video/x-raw,width=640,height=360 " +
        $"! x264enc tune=zerolatency ! rtph264pay " +
        $"! udpsink host={mediaInfo.RemoteVideoEP.Address} port={mediaInfo.RemoteVideoEP.Port} sync=false"
});

var cts = new CancellationTokenSource();
var task = Task.Run(async () =>
{
    var counter = 0;
    while (!cts.IsCancellationRequested)
    {
        await Task.Delay(500);
        await stream.WriteStringAsync($"Hello {counter++}");
    }
});

Console.WriteLine("Press ESC key to exit...");
while (Console.ReadKey().Key is not ConsoleKey.Escape) ;

cts.Cancel();
task.Wait();
await peer.DisposeAsync();

Console.ReadKey();