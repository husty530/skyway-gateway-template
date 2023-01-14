const peer = (window.peer = new window.Peer('server', {
    key: 'API_KEY',	
    debug: 3
}));

let _stream = null;
peer.once('open', async _ => {
    const video = document.getElementById('video');
    _stream = await navigator.mediaDevices
        .getUserMedia({ audio: false, video: true })
        .catch(console.error);
    video.muted = true;
    video.srcObject = _stream;
    video.playsInline = true;
    await video.play().catch(console.error);
});

peer.on('connection', conn => {
    let counter = 0;
    setInterval(() => {
        conn.send('Hello ' + counter++);
    }, 500)
});

peer.on('call', conn => {
    conn.answer(_stream);
    conn.once('close', () => peer.destroy());
});

peer.on('error', () => console.error);