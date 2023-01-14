using System.Diagnostics;
using Husty.SkywayGateway;

var key = "API_KEY";
var id = "server";

var gstPath = $"C:\\gstreamer\\1.0\\msvc_x86_64\\bin\\gst-launch-1.0";
var videoPath = "HOGE.mp4";

var peer = await Peer.CreateNewAsync(key, id);
var dataChannel = await peer.CreateDataChannelAsync();
var mediaChannel = await peer.CreateMediaChannelAsync();

var stream = await dataChannel.ListenAsync();
var mediaInfo = await mediaChannel.ListenAsync();
Console.WriteLine($"Local Video EndPoint: {mediaInfo.LocalVideoEP}");
Console.WriteLine($"Remote Video EndPoint: {mediaInfo.RemoteVideoEP}");

using var process = Process.Start(new ProcessStartInfo()
{
    FileName = gstPath,
    Arguments =
        $"-v filesrc location={videoPath} " +
        $"! decodebin " +
        $"! videoscale ! video/x-raw,width=640,height=480 " +
        $"! x264enc ! rtph264pay " +
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