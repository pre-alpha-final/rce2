const onPushBroadcastChannel = new BroadcastChannel('on-push-also-js-is-for-stupid-people');
onPushBroadcastChannel.onmessage = e => {
    DotNet.invokeMethodAsync('PushNotificationAgent.Client', 'OnPush', e.data);
}

window.addEventListener('blur', function () {
    DotNet.invokeMethodAsync('PushNotificationAgent.Client', 'OnBlur');
}, false);

window.addEventListener('focus', function () {
    DotNet.invokeMethodAsync('PushNotificationAgent.Client', 'OnFocus');
}, false);