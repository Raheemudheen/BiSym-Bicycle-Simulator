For Mobile VR (Oculus Quest) and Android/iOS devices

1. Download and install Bluetooth LE package here: https://assetstore.unity.com/packages/tools/network/bluetooth-le-for-ios-tvos-and-android-26661
Make sure to install the package from the folder after download.

2. Install mobilesupport.unitypackage in this Plugins/Blync/Core folder. This will Add 3 scripts in the Core folder, 1 BlyncControllerMobile prefab in the Prefab folder and it will overwrite AndroidManifest.xml in Plugins/Android folder.

3. In your current scene with BlyncController prefab, drag in the BlyncControllerAndroid prefab and delete BlyncController prefab. 

4. Disable the BlyncControllerAndroid prefab.

5. Replace the reference in your GameController Logic to use this BlyncControllerAndroid prefab instead of BlyncController prefab.

6. Set the MainCamera reference on BlyncControllerAndroid prefab

7. Drag MobiloPermission script from Plugins/Blync/Core to your GameController GameObject. This is a helper to request for Bluetooth permission when your app starts.

8. You can now build and run your application.

Note:
If you are building for Oculus Quest devices, you have to use Legacy XR system which means you need to Download Unity 2019.4.10f1 LTS (https://unity3d.com/get-unity/download/archive). You also need to download Oculus SDK package with this version  https://developer.oculus.com/downloads/package/unity-integration-archive/16.0/
Oculus removed support for bluetooth in their latest SDK.

If you are testing on Android device, backup AndroidManifest.xml (which is for Oculus vr) and copy the content of AndroidManifest-Cardboard.xml into AndroidManifest.xml