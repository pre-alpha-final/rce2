const onPushBroadcastChannel = new BroadcastChannel('on-push-also-js-is-for-stupid-people');
onPushBroadcastChannel.onmessage = e => {
    DotNet.invokeMethodAsync('PushNotificationAgent.Client', 'OnPush', e.data);
}
