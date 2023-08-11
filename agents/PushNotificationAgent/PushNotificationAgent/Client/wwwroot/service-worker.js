// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => { });

self.addEventListener('push', event => {
    const payload = event.data.json();
    event.waitUntil(
        self.registration.showNotification('Rce2Push', {
            body: payload.message,
            icon: 'icon-512.png',
            vibrate: [100, 50, 100],
            data: { url: payload.url }
        })
    );
});

const onPushBroadcastChannel = new BroadcastChannel('on-push-also-js-is-for-stupid-people');
self.addEventListener('push', event => {
    setTimeout(() => onPushBroadcastChannel.postMessage(event.data.text()), 1000);
});
