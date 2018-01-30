Flurry Analytics Plugin
version 1.5.1

Current Android Flurry SDK version 6.8.0
Current iOS Flurry SDK version 7.9.2

For technical support, send your questions to:
kelevra39@gmail.com

Flurry Analytics Documentation And Site:
http://flurry.com
https://developer.yahoo.com/flurry/docs/analytics/

System Requirements:
-- iOS 6.0 and above, Xcode 7.0 and above
-- Android 4.0 and above

Plugin structure:
FlurryAnalytics/Scripts
FlurryAnalytics/Plugin - os dependend files. Android folder contains google-play-services.jar file.
                         Feel free to replace it with your existing GPGS library.
FlurryAnalytics/Example - contains example scene to test plugin.

Usage:
First you need obtain API Key for you application. If you don't have
account yet please register at https://dev.flurry.com/secure/signup.do
Create application and get your API Key.

Integration:

First you need to start session.
No coding is required.

In your first scene, create empty GameObject and add FlurryAnalyticsHelper script.
Fill API keys and thats all, SDK will do the rest.

You can start session directly from code. Add this code at start of your application
KHD.FlurryAnalytics.Instance.StartSession(IOS_API_KEY, ANDROID_API_KEY, true /* set false if you don't want send crash reports to flurry */);

The Flurry SDK automatically transfers the data captured during the session once the SDK determines the session completed.
In case the device is not connected, the data is saved on the device and transferred once the device is connected again.
The SDK manages the entire process. Currently, there is no way for the app to schedule the data transfer.

Important
A unique API Key must be used for each distinct app. Using the same API Key across distinct apps is not supported
and will result in erroneous data being displayed in Flurry Analytics

Custom Events:

To track custom events, simple add this code to your script:
KHD.FlurryAnalytics.Instance.LogEvent("YourEventName");

You can track up to 300 unique Events names for each app. There is no limit on the number of times any event
can be triggered across time. Once you have added Events to your app, Flurry will automatically build
User Paths based on this data, so you can see how a user navigates through your app.
You can also use Events to build conversion funnels and user segments.

You can capture Event parameters (which include the Event itself) with 1 line of code:
KHD.FlurryAnalytics.Instance.LogEventWithParameters("YourEventName",
    new Dictionary<string, string>() {
      { "Param1", "Value1" }
    });

You can also add the dimension of time to any Event that you track.
Flurry will automatically record the duration of the Event and provide you metrics
for the average Event length overall, by session and by user.

// Start timed event.
KHD.FlurryAnalytics.Instance.LogEvent("YourTimedEventName", true);
// End timed event.
KHD.FlurryAnalytics.Instance.EndTimedEvent("YourTimedEventName");

Replicate data to Unity Analytics (Unity 5.2 or newer):

To setup Unity Analytics please follow this guide http://docs.unity3d.com/Manual/UnityAnalyticsOverview.html.
After setup all you need to do is set ReplicateDataToUnityAnalytics in FlurryAnalyticsHelper to true.
Or use this line of code:
KHD.FlurryAnalytics.Instance.replicateDataToUnityAnalytics = true;


Revenue API:
For some reasons API is different for platforms.
iOS:
Just call FlurryAnalyticsIOS.SetIAPReportingEnabled(true) and all your
purchases will be tracked automaticly.
This method should be called after StartSession!
Android:
You need manually track each purchase with FlurryAnalyticsAndroid.LogPayment.

Other:

You can find all unified methods and their description in FlurryAnalytics/Scripts/FlurryAnalytics.cs.
Some methods should be called before StartSession, please read methods summary.

Android Manifest file:

FlurryAnalytics/Plugin/Android contains AndroidManifest.xml, copy it to folder "Assets/Plugins/Android".
If you already have your AndroidManifest.xml, add the necessary permissions:
<!-- Flurry Permissions -->
<!-- Required permission -->
<uses-permission android:name="android.permission.INTERNET" />
<!-- Optional permission - highly recommended-->
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE"/>
<!-- Optional permission (location tracking) -->
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />

Don't forget about GPGS meta. If you don't want to include play services res/ folder,
replace @integer/google_play_services_version in manifest with version of your GPGS library.
You can find library version in res/values/version.xml.

Android only:
FlurryAnalytics/Scripts/FlurryAnalyticsAndroid.cs

Initialization example:
KHD.FlurryAnalyticsAndroid.Init(ANDROID_API_KEY, true);
KHD.FlurryAnalyticsAndroid.OnStartSession();

iOS only:
FlurryAnalytics/Scripts/FlurryAnalyticsIOS.cs

Initialization example:
FlurryAnalyticsIOS.StartSession(IOS_API_KEY, true);

Detailed information about Flurry Analytics can be found here
https://developer.yahoo.com/flurry/docs/analytics/




