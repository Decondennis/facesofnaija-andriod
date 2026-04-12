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
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    // Use custom handler that forces HTTP for local development (10.0.2.2)
                    var client = new HttpClient(new Helpers.Utils.LocalHttpHandler());
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.SystemDefault;
                    Console.WriteLine(client);
                }

                JsonConvert.DefaultSettings = () => UserDetails.JsonSettings;

                // Configure HTTP client to force HTTP for local XAMPP testing
                // Set custom DNS resolver that redirects HTTPS to HTTP for 10.0.2.2
                if (AppSettings.TurnSecurityProtocolType3072On)
                {
                    try
                    {
                        // Create a custom service point for 10.0.2.2 that forces HTTP
                        ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                        {
                            // For local development, accept all certificates
                            return true;
                        };

                        Console.WriteLine("[MainApplication] Configured for local HTTP testing");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[MainApplication] Configuration error: {ex.Message}");
                    }
                }

                // Ensure API config is ready before any screen starts making requests
                InitializeWoWonder.Initialize(AppSettings.TripleDesAppServiceProvider, PackageName, AppSettings.TurnSecurityProtocolType3072On, AppSettings.SetApisReportMode);
                try
                {
                    var field = typeof(InitializeWoWonder).GetField("<WebsiteUrl>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
                    field?.SetValue(null, "http://172.236.19.52");
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
    }
}