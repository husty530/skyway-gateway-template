using System.Diagnostics;
using Husty.SkywayGateway;
using OpenCvSharp;

var key = "API_KEY";
var id = "client";
var target = "server";

// var gstPath = "C:\\gstreamer\\1.0\\mingw_x86_64\\bin\\gst-launch-1.0";

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

// Option1: GStreamerで受信
//using var process = Process.Start(new ProcessStartInfo()
//{
//    FileName = gstPath,
//    Arguments =
//        $"-v udpsrc port={mediaInfo.LocalVideoEP.Port} " +
//        $"! application/x-rtp,media=video " +
//        $"! queue ! rtph264depay ! avdec_h264 ! videoconvert ! autovideosink"
//});
//process.EnableRaisingEvents = true;
//process.Exited += (s, e) => process.Start();


// Option2: OpenCV(ffmpeg backend)で受信 ... なんかちょっと遅い
//Environment.SetEnvironmentVariable("OPENCV_FFMPEG_CAPTURE_OPTIONS", "protocol_whitelist;file,rtp,udp", EnvironmentVariableTarget.User);
//File.WriteAllText(".sdp",
//    $"v=0\n" +
//    $"c=IN IP4 {mediaInfo.LocalVideoEP.Address}/{mediaInfo.LocalVideoEP.Port}\n" +
//    $"m=video {mediaInfo.LocalVideoEP.Port} RTP/AVP 100\n" +
//    $"a=rtpmap:100 H264/90000"
//);
//using var cap = new VideoCapture(".sdp");

// Option3: OpenCV(gstreamer backend)で受信 ... けっこう速い
using var cap = new VideoCapture(
    $"udpsrc port={mediaInfo.LocalVideoEP.Port} " +
    $"! application/x-rtp,media=video " +
    $"! queue ! rtph264depay ! avdec_h264 ! videoconvert ! appsink"
);

_ = Task.Run(() =>
{
    var frame = new Mat();
    while (cap.Read(frame))
    {
        Cv2.ImShow(" ", frame);
        Cv2.WaitKey(1);
    }
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