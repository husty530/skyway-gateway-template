const peer = (window.peer = new window.Peer('client', {
     key: 'API_KEY',	
     debug: 3 
}));

peer.once('open', async _ => {
    const video = document.getElementById('video');
    const dataChannel = await peer.connect('server', { serialization: 'none'});
    dataChannel.on('data', data => {
        //console.log(data);
        console.log((new TextDecoder()).decode(data));
    });
    const mediaChannel = await peer.call('server');
    mediaChannel.on('stream', async stream => {
        video.muted = true;
        video.srcObject = stream;
        video.playsInline = true;
        await video.play().catch(console.error);
    });
    mediaChannel.once('close', () => peer.destroy());
});

peer.on('error', () => console.error);