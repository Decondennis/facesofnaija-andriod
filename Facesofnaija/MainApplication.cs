using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using AndroidX.Lifecycle;
using Bumptech.Glide;
using Com.Aghajari.Emojiview;
using Com.Aghajari.Emojiview.Iosprovider;
// TODO: Replace ExoPlayer with MediaPlayer - ExoPlayer not compatible with .NET 9
//using Com.Google.Android.Exoplayer2.Database;
//using Com.Google.Android.Exoplayer2.Upstream.Cache;
using Firebase;
using IO.Agora.Rtc2;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using Plugin.CurrentActivity;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Facesofnaija.Activities;
using Facesofnaija.Activities.Communities.Groups;
using Facesofnaija.Activities.Communities.Pages;
using Facesofnaija.Activities.Live.Rtc;
using Facesofnaija.Activities.Live.Stats;
using Facesofnaija.Activities.Live.Utils;
using Facesofnaija.Activities.NativePost.Pages;
using Facesofnaija.Activities.SettingsPreferences;
using Facesofnaija.Activities.UserProfile;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Library.OneSignalNotif;
using Facesofnaija.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using Xamarin.Android.Net;
using Console = System.Console;
using Constants = Facesofnaija.Activities.Live.Page.Constants;
using Exception = System.Exception;
using System.Reflection;

namespace Facesofnaija
{
    //You can specify additional application information in this attribute
    [Application(UsesCleartextTraffic = true)]
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
        private static MainApplication Instance;
        public Activity Activity;
        public MainApplication(IntPtr handle, JniHandleOwnership transer) : base(handle, transer)
        {

        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                //A great place to initialize Xamarin.Insights and Dependency Services!
                RegisterActivityLifecycleCallbacks(this);
                Instance = this;

                //If you are Getting this error >>> System.Net.WebException: Error: TrustFailure /// then Set it to true
                if (AppSettings.TurnTrustFailureOnWebException)
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                //Bypass Web Errors 
                //======================================
                if (AppSettings.TurnSecurityProtocolType3072On)
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.SystemDefault;
                    ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                }

                JsonConvert.DefaultSettings = () => UserDetails.JsonSettings;

                var correctBaseUrl = AppSettings.SiteUrl;

                // Force WebsiteUrl to the correct server BEFORE SDK initialization
                try { typeof(InitializeWoWonder).GetProperty("WebsiteUrl")?.SetValue(null, "http://172.236.19.52"); } catch { }

                // Initialize SDK with the encrypted provider (this sets up HttpClient + all endpoint paths)
                InitializeWoWonder.Initialize(AppSettings.TripleDesAppServiceProvider, PackageName, AppSettings.TurnSecurityProtocolType3072On, AppSettings.SetApisReportMode);

                // ----------------------------------------------------------------
                // CRITICAL FIX: The SDK's internal endpoints (t class / f field)
                // are constructed using the URL from the encrypted TripleDes
                // data. We replace that with AppSettings.SiteUrl so every
                // SDK-based API call uses the app's configured base host.
                //
                // The WebsiteUrl property setter does NOT update the internal
                // HttpClient.BaseAddress NOR the pre-built endpoint strings in f.xxx.
                // ----------------------------------------------------------------
                try
                {
                    var asm = typeof(InitializeWoWonder).Assembly;

                    // ---- 1. Get the internal f field (type t) from InitializeWoWonder ----
                    var fField = typeof(InitializeWoWonder).GetField("f", BindingFlags.Static | BindingFlags.NonPublic);
                    if (fField != null)
                    {
                        var tInstance = fField.GetValue(null);
                        var oldDomainField = typeof(InitializeWoWonder).GetField("a", BindingFlags.Static | BindingFlags.NonPublic);
                        var oldDomain = oldDomainField?.GetValue(null) as string ?? InitializeWoWonder.WebsiteUrl;

                        if (!string.IsNullOrWhiteSpace(oldDomain) && tInstance != null)
                        {
                            // ---- 2. Replace old domain with configured base URL in every string field of t ----
                            var tType = tInstance.GetType();
                            var stringFields = tType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                            foreach (var sf in stringFields)
                            {
                                if (sf.FieldType != typeof(string)) continue;
                                var current = sf.GetValue(tInstance) as string;
                                if (string.IsNullOrWhiteSpace(current)) continue;
                                // Replace the old base URL (scheme + host + port only) with configured base URL
                                var replaced = ReplaceBaseUrl(current, oldDomain, correctBaseUrl);
                                if (replaced != current)
                                {
                                    // Use reflection to set the readonly field
                                    sf.SetValue(tInstance, replaced);
                                }
                            }

                            // ---- 3. Update the WebsiteUrl property + backing field ----
                            typeof(InitializeWoWonder).GetProperty("WebsiteUrl")?.SetValue(null, correctBaseUrl);
                            oldDomainField?.SetValue(null, correctBaseUrl);
                        }
                    }

                    // ---- 4. Update the internal HttpClient's BaseAddress ----
                    var bType = asm.GetType("b");
                    var httpClientField = bType?.GetField("m_a", BindingFlags.Static | BindingFlags.NonPublic);
                    if (httpClientField?.GetValue(null) is HttpClient hc)
                    {
                        hc.BaseAddress = new Uri(correctBaseUrl.TrimEnd('/') + "/");
                    }

                    // ---- 5. Update ServerKey from the encrypted data ----
                    // ServerKey is already set by Initialize(), keep it as-is
                    // unless it's empty, then try to decrypt it
                    if (string.IsNullOrWhiteSpace(InitializeWoWonder.ServerKey))
                    {
                        try
                        {
                            var eField = typeof(InitializeWoWonder).GetField("e", BindingFlags.Static | BindingFlags.NonPublic);
                            if (eField?.GetValue(null) is IDisposable decryptor)
                            {
                                var deType = decryptor.GetType();
                                var aMethod = deType.GetMethod("A", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                                var jsField = deType.GetField("Js", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                                if (aMethod != null && jsField != null)
                                {
                                    var jsValue = jsField.GetValue(null);
                                    var serverKey = aMethod.Invoke(decryptor, new[] { jsValue }) as string;
                                    if (!string.IsNullOrWhiteSpace(serverKey))
                                    {
                                        var serverKeyField = typeof(InitializeWoWonder).GetField("b", BindingFlags.Static | BindingFlags.NonPublic);
                                        serverKeyField?.SetValue(null, serverKey);
                                    }
                                }
                            }
                        }
                        catch (Exception inner)
                        {
                            Methods.DisplayReportResultTrack(inner);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Methods.DisplayReportResultTrack(ex);
                }

                // Keep DB preparation async to reduce startup pressure
                Task.Run(() =>
                {
                    try
                    {
                        var sqLiteDatabase = new SqLiteDatabase();
                        sqLiteDatabase.CheckTablesStatus();
                        sqLiteDatabase.Get_data_Login_Credentials();
                    }
                    catch (Exception ex)
                    {
                        Methods.DisplayReportResultTrack(ex);
                    }
                });

                SetAppMode();
                new Handler(Looper.MainLooper).Post(new Runnable(FirstRunExcite));

                // TODO: Replace ExoPlayer cache with MediaPlayer caching solution
                //ExoPlayer v2.18.3 Cach System
                //ExoDatabaseProvider = new StandaloneDatabaseProvider(this);
                //ExoCacheEvictor = new LeastRecentlyUsedCacheEvictor(90 * 1024 * 1024);
                //ExoCache = new SimpleCache(new File(CacheDir, AppSettings.ApplicationName), ExoCacheEvictor, ExoDatabaseProvider);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetAppMode()
        {
            try
            {
                switch (AppSettings.AppMode)
                {
                    //case AppMode.Instagram:
                    //    //disable
                    //    AppSettings.ShowPokes = false;
                    //    AppSettings.ShowMovies = false;
                    //    AppSettings.ShowMemories = false;
                    //    AppSettings.ShowArticles = false;
                    //    AppSettings.ShowFundings = false;
                    //    AppSettings.ShowGames = false;
                    //    AppSettings.ShowCommonThings = false;
                    //    AppSettings.ShowEvents = false;
                    //    AppSettings.ShowJobs = false;

                    //    AppSettings.ShowAlbum = false;
                    //    AppSettings.ShowLocation = false;
                    //    AppSettings.ShowFeelingActivity = false;
                    //    AppSettings.ShowFeeling = false;
                    //    AppSettings.ShowListening = false;
                    //    AppSettings.ShowPlaying = false;
                    //    AppSettings.ShowWatching = false;
                    //    AppSettings.ShowTraveling = false;
                    //    AppSettings.ShowFile = false;
                    //    AppSettings.ShowMusic = false;
                    //    AppSettings.ShowPolls = true;
                    //    AppSettings.ShowColor = false;
                    //    AppSettings.ShowVoiceRecord = false; 
                    //    AppSettings.ShowAnonymousPrivacyPost = false;

                    //    AppSettings.ShowCommentImage = false;
                    //    AppSettings.ShowCommentRecordVoice = false;

                    //    //Enable
                    //    AppSettings.ShowStory = true;
                    //    AppSettings.ShowCommunitiesPages = true;
                    //    AppSettings.ShowMarket = true;
                    //    AppSettings.ShowCommunitiesGroups = true;

                    //    AppSettings.PostButton = PostButtonSystem.Like;
                    //    break;
                    case AppMode.LinkedIn:
                        //disable
                        AppSettings.ShowPokes = false;
                        AppSettings.ShowMovies = false;
                        AppSettings.ShowMemories = false;
                        AppSettings.ShowStory = false;
                        AppSettings.ShowGames = false;
                        AppSettings.ShowCommonThings = false;
                        AppSettings.ShowMarket = false;

                        //Enable
                        AppSettings.ShowArticles = true;
                        AppSettings.ShowFundings = true;
                        AppSettings.ShowCommunitiesPages = true;
                        AppSettings.ShowEvents = true;
                        AppSettings.ShowJobs = true;
                        AppSettings.ShowCommunitiesGroups = true;
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FirstRunExcite()
        {
            try
            {
                //Init Settings
                MainSettings.Init();

                Methods.AppLifecycleObserver appLifecycleObserver = new Methods.AppLifecycleObserver();
                ProcessLifecycleOwner.Get().Lifecycle.AddObserver(appLifecycleObserver);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void SecondRunExcite()
        {
            try
            {
                AdsGoogle.InitializeAdsGoogle.Initialize(this);

                if (AppSettings.ShowFbBannerAds || AppSettings.ShowFbInterstitialAds || AppSettings.ShowFbRewardVideoAds)
                    InitializeFacebook.Initialize(this);

                //OneSignal Notification  
                //======================================
                OneSignalNotification.Instance.RegisterNotificationDevice(this);

                ClassMapper.SetMappers();

                //App restarted after crash
                AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentOnUnhandledExceptionRaiser;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
                TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

                AppCompatDelegate.CompatVectorFromResourcesEnabled = true;
                FirebaseApp.InitializeApp(this);

                AXEmojiManager.Install(this, new AXIOSEmojiProvider(this));

                //CrossCurrentActivity.Current.Init(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AndroidEnvironmentOnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            try
            {
                Intent intent = new Intent(Activity, typeof(SplashScreenActivity));
                intent.AddCategory(Intent.CategoryHome);
                intent.PutExtra("crash", true);
                intent.SetAction(Intent.ActionMain);
                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);

                PendingIntent pendingIntent = PendingIntent.GetActivity(GetInstance()?.BaseContext, 0, intent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? PendingIntentFlags.OneShot | PendingIntentFlags.Immutable : PendingIntentFlags.OneShot);
                AlarmManager mgr = (AlarmManager)GetInstance()?.BaseContext?.GetSystemService(AlarmService);
                mgr?.Set(AlarmType.Rtc, JavaSystem.CurrentTimeMillis() + 100, pendingIntent);

                Activity.Finish();
                JavaSystem.Exit(2);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                //var message = e.Exception.Message;
                var stackTrace = e.Exception.StackTrace;

                Methods.DisplayReportResult(Activity, stackTrace);
                Console.WriteLine(e.Exception);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                //var message = e;
                Methods.DisplayReportResult(Activity, e);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public static MainApplication GetInstance()
        {
            return Instance;
        }

        #region Agora

        private RtcEngine MRtcEngine;
        private readonly EngineConfig MGlobalConfig = new EngineConfig();
        private readonly AgoraEventHandler MHandler = new AgoraEventHandler();
        private readonly StatsManager MStatsManager = new StatsManager();

        public void InitConfig()
        {
            try
            {
                ISharedPreferences pref = PrefManager.GetPreferences(ApplicationContext);
                MGlobalConfig.SetVideoDimenIndex(pref.GetInt(Constants.PrefResolutionIdx, Constants.DefaultProfileIdx));

                bool showStats = pref.GetBoolean(Constants.PrefEnableStats, false);
                MGlobalConfig.SetIfShowVideoStats(showStats);
                MStatsManager.EnableStats(showStats);

                MGlobalConfig.SetMirrorLocalIndex(pref.GetInt(Constants.PrefMirrorLocal, 0));
                MGlobalConfig.SetMirrorRemoteIndex(pref.GetInt(Constants.PrefMirrorRemote, 0));
                MGlobalConfig.SetMirrorEncodeIndex(pref.GetInt(Constants.PrefMirrorEncode, 0));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InitRtcEngine()
        {
            try
            {
                MRtcEngine = IO.Agora.Rtc2.RtcEngine.Create(ApplicationContext, AppSettings.AppIdAgoraLive, MHandler);
                // Sets the channel profile of the Agora RtcEngine.
                // The Agora RtcEngine differentiates channel profiles and applies different optimization algorithms accordingly. For example, it prioritizes smoothness and low latency for a video call, and prioritizes video quality for a video broadcast.
                MRtcEngine.SetChannelProfile(IO.Agora.Rtc2.Constants.ChannelProfileLiveBroadcasting);
                MRtcEngine.EnableVideo();
                MRtcEngine.SetLogFile(FileUtil.InitializeLogFile(this));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public EngineConfig EngineConfig()
        {
            return MGlobalConfig;
        }

        public RtcEngine RtcEngine()
        {
            return MRtcEngine;
        }

        public StatsManager StatsManager()
        {
            return MStatsManager;
        }

        public void RegisterEventHandler(IEventHandler handler)
        {
            MHandler.AddHandler(handler);
        }

        public void RemoveEventHandler(IEventHandler handler)
        {
            MHandler.RemoveHandler(handler);
        }

        #endregion

        #region ExoCache - Temporarily Disabled (ExoPlayer not compatible with .NET 9)

        // TODO: Implement MediaPlayer-based caching solution
        //public StandaloneDatabaseProvider ExoDatabaseProvider;
        //public LeastRecentlyUsedCacheEvictor ExoCacheEvictor;
        //public SimpleCache ExoCache;

        #endregion
        public override void OnTerminate() // on stop
        {
            try
            {
                base.OnTerminate();
                UnregisterActivityLifecycleCallbacks(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            try
            {
                Activity = activity;
                CrossCurrentActivity.Current.Activity = activity;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivityDestroyed(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPaused(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityResumed(Activity activity)
        {
            try
            {
                Activity = activity;
                CrossCurrentActivity.Current.Activity = activity;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
            Activity = activity;
        }

        public void OnActivityStarted(Activity activity)
        {
            try
            {
                Activity = activity;
                CrossCurrentActivity.Current.Activity = activity;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivityStopped(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPostCreated(Activity activity, Bundle savedInstanceState)
        {
            Activity = activity;
        }

        public void OnActivityPostDestroyed(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPostPaused(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPostResumed(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPostSaveInstanceState(Activity activity, Bundle outState)
        {
            Activity = activity;
        }

        public void OnActivityPostStarted(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPostStopped(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPreCreated(Activity activity, Bundle savedInstanceState)
        {
            Activity = activity;
        }

        public void OnActivityPreDestroyed(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPrePaused(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPreResumed(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPreSaveInstanceState(Activity activity, Bundle outState)
        {
            Activity = activity;
        }

        public void OnActivityPreStarted(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPreStopped(Activity activity)
        {
            Activity = activity;
        }

        public override void OnLowMemory()
        {
            try
            {
                Console.WriteLine("WoLog: OnLowMemory  >> TrimMemory = ");

                base.OnLowMemory();
                Glide.With(this).OnLowMemory();
                GC.Collect(GC.MaxGeneration);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {

                Console.WriteLine("WoLog: OnTrimMemory  >> TrimMemory = " + level.ToString());
                //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                base.OnTrimMemory(level);
                Glide.With(this).OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void NavigateTo(Activity fromContext, Type toContext, dynamic passData)
        {
            try
            {
                var intent = new Intent(this, toContext);

                if (passData != null)
                {
                    if (toContext == typeof(GroupProfileActivity))
                    {
                        switch (passData)
                        {
                            case GroupDataObject groupClass:
                                intent.PutExtra("GroupObject", JsonConvert.SerializeObject(groupClass));
                                intent.PutExtra("GroupId", groupClass.GroupId);
                                break;
                        }
                    }
                    else if (toContext == typeof(PageProfileActivity))
                    {
                        switch (passData)
                        {
                            case PageDataObject pageClass:
                                intent.PutExtra("PageObject", JsonConvert.SerializeObject(pageClass));
                                intent.PutExtra("PageId", pageClass.PageId);
                                break;
                        }
                    }
                    else if (toContext == typeof(YoutubePlayerActivity))
                    {
                        switch (passData)
                        {
                            case PostDataObject postData:
                                intent.PutExtra("PostObject", JsonConvert.SerializeObject(postData));
                                intent.PutExtra("PostId", postData.PostId);
                                break;
                        }

                        fromContext.StartActivity(intent);
                        ((AppCompatActivity)fromContext).OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
                        return;
                    }
                    else if (toContext == typeof(UserProfileActivity))
                    {
                        switch (passData)
                        {
                            case UserDataObject userDataObject:
                                intent.PutExtra("UserObject", JsonConvert.SerializeObject(userDataObject));
                                intent.PutExtra("UserId", userDataObject.UserId);
                                break;
                        }

                        ((AppCompatActivity)fromContext).OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                        fromContext.StartActivity(intent);
                        return;
                    }

                    fromContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static string ReplaceBaseUrl(string url, string oldBase, string newBase)
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(oldBase) || string.IsNullOrWhiteSpace(newBase))
                return url;
            try
            {
                var cleanOld = oldBase.TrimEnd('/');
                var cleanNew = newBase.TrimEnd('/');
                if (url.StartsWith(cleanOld, StringComparison.OrdinalIgnoreCase))
                    return cleanNew + url.Substring(cleanOld.Length);
            }
            catch (Exception)
            {
            }
            return url;
        }
    }
}