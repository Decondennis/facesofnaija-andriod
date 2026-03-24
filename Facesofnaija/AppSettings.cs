//##############################################
//Cᴏᴘʏʀɪɢʜᴛ 2020 DᴏᴜɢʜᴏᴜᴢLɪɢʜᴛ Codecanyon Item 19703216
//Elin Doughouz >> https://www.facebook.com/Elindoughouz
//====================================================

//For the accuracy of the icon and logo, please use this website " https://appicon.co " and add images according to size in folders " mipmap " 

using System.Collections.Generic;
using Facesofnaija.Helpers.Model;
using static Facesofnaija.Activities.NativePost.Extra.WRecyclerView;

namespace Facesofnaija
{
    internal static class AppSettings
    {
        /// <summary>
        /// Deep Links To App Content
        /// you should add your website without http in the analytic.xml file >> ../values/analytic.xml .. line 5
        /// <string name="ApplicationUrlWeb">demo.wowonder.com</string>
        /// </summary>
        public static readonly string TripleDesAppServiceProvider = "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";

        //Main Settings >>>>>
        //*********************************************************
        public static string Version = "2.2";
        public static readonly string ApplicationName = "Facesofnaija";
        public static readonly string DatabaseName = "Facesofnaija";

        // Friend system = 0 , follow system = 1
        public static readonly int ConnectivitySystem = 0;

        /// <summary>
        /// When you select the application mode type.. some settings will be changed and some features will be deactivated..
        /// ==============================================
        /// AppMode.Default : (Facebook) Mode
        /// AppMode.LinkedIn : (Jobs) Mode
        /// 
        /// AppMode.Instagram >> #Next Version 
        /// ==============================================
        /// </summary>
        public static readonly AppMode AppMode = AppMode.Default; //#New

        //Main Colors >>
        //*********************************************************
        public static readonly string MainColor = "#82b53f";

        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string Lang = ""; //Default language ar

        //Set Language User on site from phone 
        public static readonly bool SetLangUser = true;

        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "3da31a7d-9fc1-4546-8bd8-5bd85563f5a3";

        // WalkThrough Settings >>
        //*********************************************************
        public static readonly bool ShowWalkTroutPage = true;

        //Main Messenger settings
        //*********************************************************
        public static readonly bool MessengerIntegration = true;
        public static readonly bool ShowDialogAskOpenMessenger = true;
        public static readonly string MessengerPackageName = "com.facesofnaija.xpression"; //APK name on Google Play

        //AdMob >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static readonly ShowAds ShowAds = ShowAds.UnProfessional;

        //Three times after entering the ad is displayed
        public static readonly int ShowAdInterstitialCount = 5;
        public static readonly int ShowAdRewardedVideoCount = 5;
        public static readonly int ShowAdNativeCount = 40;
        public static readonly int ShowAdAppOpenCount = 3;

        public static readonly bool ShowAdMobBanner = false;
        public static readonly bool ShowAdMobInterstitial = false;
        public static readonly bool ShowAdMobRewardVideo = false;
        public static readonly bool ShowAdMobNative = false;
        public static readonly bool ShowAdMobNativePost = false;
        public static readonly bool ShowAdMobAppOpen = false;
        public static readonly bool ShowAdMobRewardedInterstitial = false;

        public static readonly string AdInterstitialKey = "ca-app-pub-5135691635931982/3584502890";
        public static readonly string AdRewardVideoKey = "ca-app-pub-5135691635931982/2518408206";
        public static readonly string AdAdMobNativeKey = "ca-app-pub-5135691635931982/2280543246";
        public static readonly string AdAdMobAppOpenKey = "ca-app-pub-5135691635931982/2813560515";
        public static readonly string AdRewardedInterstitialKey = "ca-app-pub-5135691635931982/7842669101";

        //FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static readonly bool ShowFbBannerAds = false;
        public static readonly bool ShowFbInterstitialAds = false;
        public static readonly bool ShowFbRewardVideoAds = false;
        public static readonly bool ShowFbNativeAds = false;

        //YOUR_PLACEMENT_ID
        public static readonly string AdsFbBannerKey = "250485588986218_554026418632132";
        public static readonly string AdsFbInterstitialKey = "250485588986218_554026125298828";
        public static readonly string AdsFbRewardVideoKey = "250485588986218_554072818627492";
        public static readonly string AdsFbNativeKey = "250485588986218_554706301897477";

        //Colony Ads >> Please add the code ad in the Here 
        //*********************************************************  
        public static readonly bool ShowColonyBannerAds = false;
        public static readonly bool ShowColonyInterstitialAds = false;
        public static readonly bool ShowColonyRewardAds = false;

        public static readonly string AdsColonyAppId = "appff22269a7a0a4be8aa";
        public static readonly string AdsColonyBannerId = "vz85ed7ae2d631414fbd";
        public static readonly string AdsColonyInterstitialId = "vz39712462b8634df4a8";
        public static readonly string AdsColonyRewardedId = "vz32ceec7a84aa4d719a";
        //********************************************************* 

        public static readonly bool EnableRegisterSystem = true;

        /// <summary>
        /// true => Only over 18 years old
        /// false => all 
        /// </summary> 
        public static readonly bool ShowBirthdayInRegister = true; //#New
        public static readonly bool IsUserYearsOld = false;
        public static readonly bool AddAllInfoPorfileAfterRegister = true;

        //Set Theme Full Screen App
        //*********************************************************
        public static readonly bool EnableFullScreenApp = false;

        //Code Time Zone (true => Get from Internet , false => Get From #CodeTimeZone )
        //*********************************************************
        public static readonly bool AutoCodeTimeZone = true;
        public static readonly string CodeTimeZone = "UTC";

        //Error Report Mode
        //*********************************************************
        public static readonly bool SetApisReportMode = false;

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file 
        //Facebook >> ../values/analytic.xml .. line 10-11 
        //Google >> ../values/analytic.xml .. line 15 
        //*********************************************************
        public static readonly bool EnableSmartLockForPasswords = true;

        public static readonly bool ShowFacebookLogin = true;
        public static readonly bool ShowGoogleLogin = false;

        public static readonly string ClientId = "421353499628-sh4hrbi9adj9m2slnpsv19a75edrq87p.apps.googleusercontent.com";

        //########################### 

        //Main Slider settings
        //*********************************************************
        public static readonly PostButtonSystem PostButton = PostButtonSystem.ReactionDefault;
        public static readonly ToastTheme ToastTheme = ToastTheme.Custom;

        /// <summary>
        /// None : To disable Reels video on the app 
        /// </summary>
        public static readonly ReelsPosition ReelsPosition = ReelsPosition.Tab;
        public static readonly bool ShowYouTubeReels = false; //#New
        public static readonly bool ShowUsernameReels = false; //#New


        public static readonly bool ShowBottomAddOnTab = true;

        public static readonly long RefreshAppAPiSeconds = 30000; //#New

        public static readonly bool ShowAlbum = true;
        public static bool ShowArticles = false;
        public static bool ShowPokes = true;
        public static bool ShowCommunitiesGroups = true;
        public static bool ShowCommunities = true;
        public static bool ShowCommunitiesPages = true;
        public static bool ShowMarket = true;
        public static readonly bool ShowPopularPosts = true;
        /// <summary>
        /// if selected false will remove boost post and get list Boosted Posts
        /// </summary>
        public static readonly bool ShowBoostedPosts = true;
        public static readonly bool ShowBoostedPages = true;
        public static bool ShowMovies = true;
        public static readonly bool ShowNearBy = true;
        public static bool ShowStory = true;
        public static readonly bool ShowSavedPost = true;
        public static readonly bool ShowUserContacts = true;
        public static bool ShowJobs = true;
        public static bool ShowVirtualTour = true;
        public static bool ShowCommonThings = true;
        public static bool ShowFundings = true;
        public static readonly bool ShowMyPhoto = true;
        public static readonly bool ShowMyVideo = true;
        public static bool ShowGames = true;
        public static bool ShowMemories = true;
        public static readonly bool ShowOffers = true;
        public static readonly bool ShowNearbyShops = true;

        public static readonly bool ShowSuggestedPage = true;
        public static readonly bool ShowSuggestedGroup = true;
        public static readonly bool ShowSuggestedUser = true;

        public static readonly bool ShowCommentImage = true; //#New
        public static readonly bool ShowCommentRecordVoice = true; //#New

        //count times after entering the Suggestion is displayed
        public static readonly int ShowSuggestedPageCount = 90;
        public static readonly int ShowSuggestedGroupCount = 70;
        public static readonly int ShowSuggestedUserCount = 50;
        public static readonly int ShowSuggestedCommunityCount = 110;

        public static string JobsUrl = "https://job.facesofnaija.com/";
        public static string VirtualTourUrl = "https://virtualtour.facesofnaija.com/";

        //allow download or not when share
        public static readonly bool AllowDownloadMedia = true;

        public static readonly bool ShowAdvertise = true;

        /// <summary>
        /// https://rapidapi.com/api-sports/api/covid-193
        /// you can get api key and host from here https://prnt.sc/wngxfc 
        /// </summary>
        public static readonly bool ShowInfoCoronaVirus = false;
        public static readonly string KeyCoronaVirus = "164300ef98msh0911b69bed3814bp131c76jsneaca9364e61f";
        public static readonly string HostCoronaVirus = "covid-193.p.rapidapi.com";

        public static readonly bool ShowLive = true;
        public static readonly string AppIdAgoraLive = "36ae9c3b6741465b8360fbe117486b41";

        //Events settings
        //*********************************************************  
        public static bool ShowEvents = true;
        public static readonly bool ShowEventGoing = true;
        public static readonly bool ShowEventInvited = true;
        public static readonly bool ShowEventInterested = true;
        public static readonly bool ShowEventPast = true;

        // Story >>
        //*********************************************************
        //Set a story duration >> Sec
        public static readonly long StoryImageDuration = 30;
        public static readonly long StoryVideoDuration = 30;

        /// <summary>
        /// If it is false, it will appear only for the specified time in the value of the StoryVideoDuration
        /// </summary>
        public static readonly bool ShowFullVideo = false;

        public static readonly bool EnableStorySeenList = true;
        public static readonly bool EnableReplyStory = true;

        /// <summary>
        /// https://dashboard.stipop.io/
        /// you can get api key from here https://prnt.sc/26ofmq9
        /// </summary>
        public static readonly string StickersApikey = "0a441b19287cad752e87f6072bb914c0";

        //*********************************************************

        /// <summary>
        ///  Currency
        /// CurrencyStatic = true : get currency from app not api 
        /// CurrencyStatic = false : get currency from api (default)
        /// </summary>
        public static readonly bool CurrencyStatic = false;
        public static readonly string CurrencyIconStatic = "₦";
        public static readonly string CurrencyCodeStatic = "NGN";
        public static readonly string CurrencyFundingPriceStatic = "₦";

        //Profile settings
        //*********************************************************
        public static readonly bool ShowGift = true;
        public static readonly bool ShowWallet = false;
        public static readonly bool ShowGoPro = true;
        public static readonly bool ShowAddToFamily = true;

        public static readonly bool ShowUserGroup = false;
        public static readonly bool ShowUserCommunity = true;
        public static readonly bool ShowUserPage = true;
        public static readonly bool ShowUserImage = true;
        public static readonly bool ShowUserSocialLinks = true;

        public static readonly CoverImageStyle CoverImageStyle = CoverImageStyle.CenterCrop;

        /// <summary>
        /// The default value comes from the site .. in case it is not available, it is taken from these values
        /// </summary>
        public static readonly string WeeklyPrice = "3";
        public static readonly string MonthlyPrice = "8";
        public static readonly string YearlyPrice = "89";
        public static readonly string LifetimePrice = "259";

        //Native Post settings
        //********************************************************* 
        public static readonly bool ShowTextWithSpace = true;

        public static readonly bool ShowTextShareButton = true;
        public static readonly bool ShowShareButton = true;

        public static readonly int AvatarPostSize = 60;
        public static readonly int ImagePostSize = 200;
        public static readonly string PostApiLimitOnScroll = "8";

        public static readonly string PostApiLimitOnBackground = "10";

        public static readonly bool AutoPlayVideo = true;

        public static readonly bool EmbedDeepSoundPostType = true;
        public static readonly VideoPostTypeSystem EmbedFacebookVideoPostType = VideoPostTypeSystem.EmbedVideo;
        public static readonly VideoPostTypeSystem EmbedVimeoVideoPostType = VideoPostTypeSystem.EmbedVideo;
        public static readonly VideoPostTypeSystem EmbedPlayTubeVideoPostType = VideoPostTypeSystem.EmbedVideo;
        public static readonly VideoPostTypeSystem EmbedTikTokVideoPostType = VideoPostTypeSystem.Link;
        public static readonly VideoPostTypeSystem EmbedTwitterPostType = VideoPostTypeSystem.Link;
        public static readonly bool ShowSearchForPosts = true;
        public static readonly bool EmbedLivePostType = true;

        //new posts users have to scroll back to top
        public static readonly bool ShowNewPostOnNewsFeed = true;
        public static readonly bool ShowAddPostOnNewsFeed = false;
        public static readonly bool ShowCountSharePost = true;

        /// <summary>
        /// Post Privacy
        /// ShowPostPrivacyForAllUser = true : all posts user have icon Privacy 
        /// ShowPostPrivacyForAllUser = false : just my posts have icon Privacy (default)
        /// </summary>
        public static readonly bool ShowPostPrivacyForAllUser = false;

        public static readonly bool EnableVideoCompress = true;
        public static readonly bool EnableFitchOgLink = true;

        /// <summary>
        /// On : if the length of the text is more than 50 characters will be text is bigger
        /// Off : all text same size
        /// </summary>
        public static readonly VolumeState TextSizeDescriptionPost = VolumeState.On;//#New

        //Trending page
        //*********************************************************   
        public static readonly bool ShowTrendingPage = true;

        public static readonly bool ShowProUsersMembers = true;
        public static readonly bool ShowPromotedPages = true;
        public static readonly bool ShowTrendingHashTags = true;
        public static readonly bool ShowLastActivities = true;
        public static readonly bool ShowShortcuts = true;
        public static readonly bool ShowFriendsBirthday = true;
        public static readonly bool ShowAnnouncement = true;

        /// <summary>
        /// https://www.weatherapi.com
        /// </summary>
        public static readonly WeatherType WeatherType = WeatherType.Celsius;
        public static readonly bool ShowWeather = true;
        public static readonly string KeyWeatherApi = "a413d0bf31a44369a16140106221804";

        /// <summary>
        /// https://openexchangerates.org
        /// #Currency >> Your currency
        /// #Currencies >> you can use just 3 from those : USD,EUR,DKK,GBP,SEK,NOK,CAD,JPY,TRY,EGP,SAR,JOD,KWD,IQD,BHD,DZD,LYD,AED,QAR,LBP,OMR,AFN,ALL,ARS,AMD,AUD,BYN,BRL,BGN,CLP,CNY,MYR,MAD,ILS,TND,YER
        /// </summary>
        public static readonly bool ShowExchangeCurrency = false;
        public static readonly string KeyCurrencyApi = "644761ef2ba94ea5aa84767109d6cf7b";
        public static readonly string ExCurrency = "NGN";
        public static readonly string ExCurrencies = "USD,EUR,GBP";
        public static readonly List<string> ExCurrenciesIcons = new List<string>() { "$", "€", "£" };

        //********************************************************* 

        /// <summary>
        /// you can edit video using FFMPEG 
        /// </summary>
        public static readonly bool EnableVideoEditor = true;


        public static readonly bool ShowUserPoint = false;

        //Add Post
        public static readonly AddPostSystem AddPostSystem = AddPostSystem.AllUsers;
        public static readonly GalleryIntentSystem GalleryIntentSystem = GalleryIntentSystem.Matisse; //#New

        public static readonly bool ShowGalleryImage = true;
        public static readonly bool ShowGalleryVideo = true;
        public static readonly bool ShowMention = true;
        public static readonly bool ShowLocation = true;
        public static readonly bool ShowFeelingActivity = true;
        public static readonly bool ShowFeeling = true;
        public static readonly bool ShowListening = true;
        public static readonly bool ShowPlaying = true;
        public static readonly bool ShowWatching = true;
        public static readonly bool ShowTraveling = true;
        public static readonly bool ShowGif = true;
        public static readonly bool ShowFile = true;
        public static readonly bool ShowMusic = true;
        public static readonly bool ShowPolls = true;
        public static readonly bool ShowColor = true;
        public static readonly bool ShowVoiceRecord = true;

        public static readonly bool ShowAnonymousPrivacyPost = true;

        //Advertising 
        public static readonly bool ShowAdvertisingPost = true;

        //Settings Page >> General Account
        public static readonly bool ShowSettingsGeneralAccount = true;
        public static readonly bool ShowSettingsAccount = true;
        public static readonly bool ShowSettingsSocialLinks = true;
        public static readonly bool ShowSettingsPassword = true;
        public static readonly bool ShowSettingsBlockedUsers = true;
        public static readonly bool ShowSettingsDeleteAccount = true;
        public static readonly bool ShowSettingsTwoFactor = true;
        public static readonly bool ShowSettingsManageSessions = true;
        public static readonly bool ShowSettingsVerification = true;

        public static readonly bool ShowSettingsSocialLinksFacebook = true;
        public static readonly bool ShowSettingsSocialLinksTwitter = true;
        public static readonly bool ShowSettingsSocialLinksGoogle = true;
        public static readonly bool ShowSettingsSocialLinksVkontakte = true;
        public static readonly bool ShowSettingsSocialLinksLinkedin = true;
        public static readonly bool ShowSettingsSocialLinksInstagram = true;
        public static readonly bool ShowSettingsSocialLinksYouTube = true;

        //Settings Page >> Privacy
        public static readonly bool ShowSettingsPrivacy = true;
        public static readonly bool ShowSettingsNotification = true;

        //Settings Page >> Tell a Friends (Earnings)
        public static readonly bool ShowSettingsInviteFriends = true;

        public static readonly bool ShowSettingsShare = true;
        public static readonly bool ShowSettingsMyAffiliates = false;
        public static readonly bool ShowWithdrawals = false;

        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// Just replace it with this 5 lines of code
        /// <uses-permission android:name="android.permission.READ_CONTACTS" />
        /// <uses-permission android:name="android.permission.READ_PHONE_NUMBERS" />
        /// </summary>
        public static readonly bool InvitationSystem = true;

        //Settings Page >> Help && Support
        public static readonly bool ShowSettingsHelpSupport = true;

        public static readonly bool ShowSettingsHelp = true;
        public static readonly bool ShowSettingsReportProblem = true;
        public static readonly bool ShowSettingsAbout = true;
        public static readonly bool ShowSettingsPrivacyPolicy = true;
        public static readonly bool ShowSettingsTermsOfUse = true;

        public static readonly bool ShowSettingsRateApp = true;
        public static readonly int ShowRateAppCount = 5;

        public static readonly bool ShowSettingsUpdateManagerApp = false;

        public static readonly bool ShowSettingsInvitationLinks = false;
        public static readonly bool ShowSettingsMyInformation = true;

        public static readonly bool ShowSuggestedUsersOnRegister = true;

        //Set Theme Tab
        //*********************************************************
        public static TabTheme SetTabDarkTheme = TabTheme.Light;
        public static readonly MoreTheme MoreTheme = MoreTheme.Grid;

        //Bypass Web Errors  
        //*********************************************************
        public static readonly bool TurnTrustFailureOnWebException = true;
        public static readonly bool TurnSecurityProtocolType3072On = true;

        //*********************************************************
        public static readonly bool RenderPriorityFastPostLoad = false;

        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// <uses-permission android:name="com.android.vending.BILLING" />
        /// </summary>
        public static readonly bool ShowInAppBilling = false;

        /// <summary>
        /// Paypal and google pay using Braintree Gateway https://www.braintreepayments.com/
        /// 
        /// Add info keys in Payment Methods : https://prnt.sc/1z5bffc - https://prnt.sc/1z5b0yj
        /// To find your merchant ID :  https://prnt.sc/1z59dy8
        ///
        /// Tokenization Keys : https://prnt.sc/1z59smv
        /// </summary>
        public static readonly bool ShowPaypal = true;
        public static readonly string MerchantAccountId = "test";

        public static readonly string SandboxTokenizationKey = "sandbox_kt2f6mdh_hf4c******";
        public static readonly string ProductionTokenizationKey = "production_t2wns2y2_dfy45******";

        public static readonly bool ShowBankTransfer = true;
        public static readonly bool ShowCreditCard = false;

        //********************************************************* 
        public static readonly bool ShowCashFree = true;

        /// <summary>
        /// Currencies : http://prntscr.com/u600ok
        /// </summary>
        public static readonly string CashFreeCurrency = "INR";

        //********************************************************* 

        /// <summary>
        /// If you want RazorPay you should change id key in the analytic.xml file
        /// razorpay_api_Key >> .. line 24 
        /// </summary>
        public static readonly bool ShowRazorPay = true;

        /// <summary>
        /// Currencies : https://razorpay.com/accept-international-payments
        /// </summary>
        public static readonly string RazorPayCurrency = "USD";

        public static readonly bool ShowPayStack = true;
        public static readonly bool ShowPaySera = false;  //#Next Version   

        //********************************************************* 

    }
}