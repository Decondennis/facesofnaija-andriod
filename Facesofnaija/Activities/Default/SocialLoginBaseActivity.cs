using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api.Identity;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Extensions;
using Android.Gms.Tasks;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.Content;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facesofnaija.Activities.Base;
using Facesofnaija.Activities.MyProfile;
using Facesofnaija.Activities.Tabbes;
using Facesofnaija.Activities.Upgrade;
using Facesofnaija.Activities.WalkTroutPage;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.SocialLogins;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Library.OneSignalNotif;
using Facesofnaija.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Auth;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using Task = System.Threading.Tasks.Task;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Reflection;

namespace Facesofnaija.Activities.Default
{
    public abstract class SocialLoginBaseActivity : BaseActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback, IOnSuccessListener, IOnFailureListener
    {
        #region Variables Basic

        private ICallbackManager MFbCallManager;
        private FbMyProfileTracker ProfileTracker;
        public LinearLayout FbLoginButton;
        public LinearLayout GoogleSignInButton;
        public LinearLayout TwitterLoginButton;
        public LinearLayout LinkedinLoginButton;
        public static GoogleSignInClient MGoogleSignInClient;
        public static SocialLoginBaseActivity Instance;

        private string TimeZone = "";
        private bool IsActiveUser = true;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

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

                //Set Full screen 
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Window?.SetSoftInputMode(SoftInput.AdjustResize);

                Methods.App.FullScreenApp(this);

                Instance = this;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) == Permission.Granted)
                    {
                        if (string.IsNullOrEmpty(UserDetails.DeviceId))
                            OneSignalNotification.Instance.RegisterNotificationDevice(this);
                    }
                    else
                    {
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.PostNotifications
                        }, 16248);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(UserDetails.DeviceId))
                        OneSignalNotification.Instance.RegisterNotificationDevice(this);
                }

                // Disabled on startup to avoid ANR and blocking UI thread
                //if (Methods.CheckConnectivity())
                //    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });

                GetTimezone();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        #region Events

        private void BtnFacebookOnClick(object sender, EventArgs e)
        {
            try
            {
                HideKeyboard();
                LoginManager.Instance.LogInWithReadPermissions(this, new List<string>
                {
                    "email",
                    "public_profile"
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void ProfileTrackerOnMOnProfileChanged(object sender, ProfileChangedEventArgs e)
        {
            try
            {
                HideKeyboard();
                if (e.MProfile != null)
                {
                    //FbFirstName = e.MProfile.FirstName;
                    //FbLastName = e.MProfile.LastName;
                    //FbName = e.MProfile.Name;
                    //FbProfileId = e.MProfile.Id;

                    var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
                    var parameters = new Bundle();
                    parameters.PutString("fields", "id,name,age_range,email");
                    request.Parameters = parameters;
                    request.ExecuteAndWait();
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region Functions

        public void InitSocialLogins()
        {
            try
            {
                //#Facebook
                if (AppSettings.ShowFacebookLogin)
                {
                    //FacebookSdk.SdkInitialize(this);
                    LoginButton loginButton = new LoginButton(this);
                    ProfileTracker = new FbMyProfileTracker();
                    ProfileTracker.StartTracking();

                    FbLoginButton = FindViewById<LinearLayout>(Resource.Id.bntLoginFacebook);
                    FbLoginButton.Visibility = ViewStates.Visible;
                    FbLoginButton.Click += BtnFacebookOnClick;

                    ProfileTracker.MOnProfileChanged += ProfileTrackerOnMOnProfileChanged;
                    loginButton.SetPermissions(new List<string>
                    {
                        "email",
                        "public_profile"
                    });

                    MFbCallManager = CallbackManagerFactory.Create();
                    LoginManager.Instance.RegisterCallback(MFbCallManager, this);

                    //FB accessToken
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }

                    string hash = Methods.App.GetKeyHashesConfigured(this);
                    Console.WriteLine(hash);
                }
                else
                {
                    FbLoginButton = FindViewById<LinearLayout>(Resource.Id.bntLoginFacebook);
                    FbLoginButton.Visibility = ViewStates.Gone;
                }

                //#Google
                if (AppSettings.ShowGoogleLogin)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient = GoogleSignIn.GetClient(this, gso);

                    GoogleSignInButton = FindViewById<LinearLayout>(Resource.Id.bntLoginGoogle);
                    if (GoogleSignInButton != null)
                        GoogleSignInButton.Click += GoogleSignInButtonOnClick;
                }
                else
                {
                    GoogleSignInButton = FindViewById<LinearLayout>(Resource.Id.bntLoginGoogle);
                    if (GoogleSignInButton != null)
                        GoogleSignInButton.Visibility = ViewStates.Gone;
                }

                //#Twitter (browser-based OAuth)
                TwitterLoginButton = FindViewById<LinearLayout>(Resource.Id.bntLoginTwitter);
                if (TwitterLoginButton != null)
                    TwitterLoginButton.Click += (sender, args) =>
                    {
                        try
                        {
                            var uri = Android.Net.Uri.Parse("http://172.236.19.52/login-with.php?provider=Twitter");
                            var intent = new Intent(Intent.ActionView, uri);
                            StartActivity(intent);
                        }
                        catch (Exception ex)
                        {
                            Methods.DisplayReportResultTrack(ex);
                        }
                    };

                //#LinkedIn (browser-based OAuth)
                LinkedinLoginButton = FindViewById<LinearLayout>(Resource.Id.bntLoginLinkedin);
                if (LinkedinLoginButton != null)
                    LinkedinLoginButton.Click += (sender, args) =>
                    {
                        try
                        {
                            var uri = Android.Net.Uri.Parse("http://172.236.19.52/login-with.php?provider=LinkedIn");
                            var intent = new Intent(Intent.ActionView, uri);
                            StartActivity(intent);
                        }
                        catch (Exception ex)
                        {
                            Methods.DisplayReportResultTrack(ex);
                        }
                    };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetDataLogin(AuthObject auth, string email, string password, bool chkRemember = true)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.Username = email;
                UserDetails.FullName = email;
                UserDetails.Password = password;
                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = email;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UserDetails.Email,
                    Password = UserDetails.Password,
                    Status = "Pending",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId,
                    Email = UserDetails.Email,
                };

                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                if (chkRemember)
                {
                    var dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdateLogin_Credentials(user);
                }

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(this) });

                if (auth.IsNew != null && auth.IsNew.Value)
                {
                    if (AppSettings.ShowWalkTroutPage)
                    {
                        Intent newIntent = new Intent(this, typeof(WalkTroutActivity));
                        newIntent?.PutExtra("class", "register");
                        StartActivity(newIntent);
                    }
                    else
                    {
                        if (ListUtils.SettingsSiteList?.MembershipSystem == "1")
                        {
                            var intent = new Intent(this, typeof(GoProActivity));
                            intent.PutExtra("class", "register");
                            StartActivity(intent);
                        }
                        else
                        {
                            if (AppSettings.AddAllInfoPorfileAfterRegister)
                            {
                                Intent newIntent = new Intent(this, typeof(AddAllInfoProfileActivity));
                                StartActivity(newIntent);
                            }
                            else
                            {
                                StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                            }
                        }
                    }
                }
                else
                {
                    if (auth.Membership != null && auth.Membership.Value)
                    {
                        var intent = new Intent(this, typeof(GoProActivity));
                        intent.PutExtra("class", "login");
                        StartActivity(intent);
                    }
                    else
                    {
                        if (AppSettings.ShowWalkTroutPage)
                        {
                            Intent newIntent = new Intent(this, typeof(WalkTroutActivity));
                            newIntent?.PutExtra("class", "login");
                            StartActivity(newIntent);
                        }
                        else
                        {
                            StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetDataLogin(CreatAccountObject auth, string Username, string email, string password)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.Username = Username;
                UserDetails.FullName = Username;
                UserDetails.Password = password;
                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = email;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UserDetails.Username,
                    Password = UserDetails.Password,
                    Status = "Pending",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId,
                    Email = UserDetails.Email,
                };

                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(this) });

                if (AppSettings.ShowWalkTroutPage)
                {
                    Intent newIntent = new Intent(this, typeof(WalkTroutActivity));
                    newIntent?.PutExtra("class", "register");
                    StartActivity(newIntent);
                }
                else
                {
                    if (ListUtils.SettingsSiteList?.MembershipSystem == "1")
                    {
                        var intent = new Intent(this, typeof(GoProActivity));
                        intent.PutExtra("class", "register");
                        StartActivity(intent);
                    }
                    else
                    {
                        if (AppSettings.AddAllInfoPorfileAfterRegister)
                        {
                            Intent newIntent = new Intent(this, typeof(AddAllInfoProfileActivity));
                            StartActivity(newIntent);
                        }
                        else
                        {
                            StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Abstract members
        public abstract void ToggleVisibility(bool isLoginProgress);

        #endregion

        #region Social Logins

        private string FbAccessToken, GAccessToken;

        #region Facebook

        public void OnCancel()
        {
            try
            {
                ToggleVisibility(false);

                //SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnError(FacebookException error)
        {
            try
            {

                ToggleVisibility(false);

                // Handle exception
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

                //SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        void IFacebookCallback.OnSuccess(Object result)
        {
            try
            {
                //var loginResult = result as LoginResult;
                //var id = AccessToken.CurrentAccessToken.UserId;

                ToggleVisibility(false);

                //SetResult(Result.Ok);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void OnCompleted(JSONObject json, GraphResponse response)
        {
            try
            {
                ToggleVisibility(true);

                var accessToken = AccessToken.CurrentAccessToken;
                if (accessToken != null)
                {
                    FbAccessToken = accessToken.Token;

                    //Login Api 
                    var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(FbAccessToken, "facebook", UserDetails.DeviceId);
                    if (apiStatus == 200)
                    {
                        if (respond is AuthObject auth)
                        {
                            //if (!string.IsNullOrEmpty(json?.ToString()))
                            //{
                            //    var data = json.ToString();
                            //    var result = JsonConvert.DeserializeObject<FacebookResult>(data);
                            //    //FbEmail = result.Email;
                            //}

                            SetDataLogin(auth, "", "");

                            ToggleVisibility(false);
                            Finish();
                        }
                    }
                    else if (apiStatus == 400)
                    {
                        if (respond is ErrorObject error)
                        {
                            ToggleVisibility(false);
                            var errorText = error.Error.ErrorText;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                        }
                    }
                    else
                    {
                        ToggleVisibility(false);
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    ToggleVisibility(false);
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //======================================================

        #region Google

        //Event Click login using google
        private void GoogleSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (MGoogleSignInClient == null)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient ??= GoogleSignIn.GetClient(this, gso);
                }

                var signInIntent = MGoogleSignInClient.SignInIntent;
                StartActivityForResult(signInIntent, 0);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void SetContentGoogle(GoogleSignInAccount acct)
        {
            try
            {
                //Successful log in hooray!!
                if (acct != null)
                {
                    ToggleVisibility(true);

                    //var GAccountName = acct.Account.Name;
                    //var GAccountType = acct.Account.Type;
                    //var GDisplayName = acct.DisplayName;
                    //var GFirstName = acct.GivenName;
                    //var GLastName = acct.FamilyName;
                    //var GProfileId = acct.Id;
                    //var GEmail = acct.Email;
                    //var GImg = acct.PhotoUrl.Path;
                    GAccessToken = acct.IdToken;
                    //GServerCode = acct.ServerAuthCode;
                    //Console.WriteLine(GServerCode);

                    if (!string.IsNullOrEmpty(GAccessToken))
                    {
                        var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(GAccessToken, "google", UserDetails.DeviceId);
                        if (apiStatus == 200)
                        {
                            if (respond is AuthObject auth)
                            {
                                SetDataLogin(auth, "", "");

                                ToggleVisibility(false);
                                Finish();
                            }
                        }
                        else if (apiStatus == 400)
                        {
                            if (respond is ErrorObject error)
                            {
                                ToggleVisibility(false);
                                var errorText = error.Error.ErrorText;
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                            }
                        }
                        else
                        {
                            ToggleVisibility(false);
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                        }
                    }
                    else
                    {
                        ToggleVisibility(false);
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception exception)
            {
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #endregion

        #region Result & Permissions

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                // Logins Facebook
                MFbCallManager?.OnActivityResult(requestCode, (int)resultCode, data);
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 0)
                {
                    GoogleSignInAccount credential = await GoogleSignIn.GetSignedInAccountFromIntentAsync(data);

                    if (credential != null)
                        SetContentGoogle(credential);
                }
                else if (requestCode == RcCredentialsHint)
                {
                    if (resultCode == Result.Ok)
                    {
                        SignInCredential credential = OneTapClient.GetSignInCredentialFromIntent(data);
                        string idToken = credential.GoogleIdToken;
                        string username = credential.Id;
                        string password = credential.Password;

                        if (!string.IsNullOrEmpty(credential?.Id) && !string.IsNullOrEmpty(credential?.Password))
                        {
                            // Email/password account
                            Console.WriteLine("Signed in as {0}", credential.Id);

                            //send api auth  
                            HideKeyboard();

                            ToggleVisibility(true);

                            await AuthApi(credential.Id, credential.Password);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case 16248 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                    if (string.IsNullOrEmpty(UserDetails.DeviceId))
                        OneSignalNotification.Instance.RegisterNotificationDevice(this);
                    break;
                case 16248:
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    break;
            }
        }

        #endregion

        #region Cross App Authentication

        private static readonly int RcCredentialsHint = 10;

        private ISignInClient OneTapClient;
        private BeginSignInRequest SignInRequest;

        public async void BuildClients()
        {
            try
            {
                OneTapClient = Identity.GetSignInClient(this);
                SignInRequest = new BeginSignInRequest.Builder()
                    .SetPasswordRequestOptions(new BeginSignInRequest.PasswordRequestOptions.Builder().SetSupported(true).Build())
                    .SetGoogleIdTokenRequestOptions(new BeginSignInRequest.GoogleIdTokenRequestOptions.Builder()
                        .SetSupported(true)
                        // Your server's client ID, not your Android client ID.
                        .SetServerClientId(AppSettings.ClientId)
                        // true : Only show accounts previously used to sign in.
                        // false : Show all accounts on the device.
                        .SetFilterByAuthorizedAccounts(false)
                        .Build())
                    // true : Automatically sign in when exactly one credential is retrieved.
                    //.SetAutoSelectEnabled(true)
                    .Build();

                await OneTapClient.BeginSignIn(SignInRequest).AddOnSuccessListener(this).AddOnFailureListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        void IOnSuccessListener.OnSuccess(Object result)
        {
            try
            {
                if (result is BeginSignInResult results)
                {
                    StartIntentSenderForResult(results.PendingIntent.IntentSender, RcCredentialsHint, null, 0, 0, 0);
                }
            }
            catch (IntentSender.SendIntentException e)
            {
                Console.WriteLine("Couldn't start One Tap UI: " + e.LocalizedMessage);
            }
        }

        public void OnFailure(Java.Lang.Exception e)
        {

        }

        #endregion

        public void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async Task AuthApi(string email, string password, bool chkRemember = true)
        {
            var (apiStatus, respond) = await TryAuthDirectAsync(email, password);
            if (apiStatus == 200)
            {
                if (respond is AuthObject auth)
                {
                    var emailValidation = ListUtils.SettingsSiteList?.EmailValidation ?? "0";
                    if (emailValidation == "1")
                        IsActiveUser = await CheckIsActiveUser(auth.UserId);

                    if (IsActiveUser)
                    {
                        SetDataLogin(auth, email, password, chkRemember);

                        ToggleVisibility(false);
                        FinishAffinity();
                    }
                    else
                    {
                        ToggleVisibility(false);
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ThisUserNotActive), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else if (respond is AuthMessageObject messageObject)
                {
                    ToggleVisibility(false);

                    UserDetails.Username = email;
                    UserDetails.FullName = email;
                    UserDetails.Password = password;
                    UserDetails.UserId = messageObject.UserId;
                    UserDetails.Status = "Pending";
                    UserDetails.Email = email;

                    //Insert user data to database
                    var user = new DataTables.LoginTb
                    {
                        UserId = UserDetails.UserId,
                        AccessToken = "",
                        Cookie = "",
                        Username = email,
                        Password = password,
                        Status = "Pending",
                        Lang = "",
                        DeviceId = UserDetails.DeviceId,
                    };
                    ListUtils.DataUserLoginList.Add(user);

                    var dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdateLogin_Credentials(user);

                    Intent newIntent = new Intent(this, typeof(VerificationCodeActivity));
                    newIntent?.PutExtra("TypeCode", "TwoFactor");
                    StartActivity(newIntent);
                }
                else
                {
                    ToggleVisibility(false);
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond?.ToString() ?? "Login failed", GetText(Resource.String.Lbl_Ok));
                }
            }
            else if (apiStatus == 400)
            {
                ToggleVisibility(false);

                if (respond is ErrorObject error)
                {
                    var errorText = error.Error.ErrorText ?? "Login failed";
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                }
                else if (respond is JObject jobj)
                {
                    // Handle raw JSON response for error details
                    var errorMessage = jobj["message"]?.ToString() ?? jobj["error"]?.ToString() ?? "Login failed";
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorMessage, GetText(Resource.String.Lbl_Ok));
                }
                else
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond?.ToString() ?? "Login failed", GetText(Resource.String.Lbl_Ok));
                }
            }
            else
            {
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond?.ToString() ?? "Login failed. Please check your connection.", GetText(Resource.String.Lbl_Ok));
            }
        }

        private async Task<(long, dynamic)> TryAuthDirectAsync(string email, string password)
        {
            try
            {
                var requestsUrl = "http://172.236.19.52/requests.php?f=login";
                var phoneApiUrl = "http://172.236.19.52/app_api.php?type=user_login";
                var webUrls = new[]
                {
                    "http://172.236.19.52/api/v2/endpoints/auth.php",
                    "http://172.236.19.52/api/auth",
                };

                using var handler = new Xamarin.Android.Net.AndroidMessageHandler();
                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Linux; Android 16; Mobile) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36");

                long lastStatus = 404;
                dynamic lastRespond = "Server authentication failed";

                // Try web requests.php endpoint first (same as web login form)
                try
                {
                    using var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("username", email),
                        new KeyValuePair<string, string>("password", password),
                    });

                    var response = await client.PostAsync(requestsUrl, content).ConfigureAwait(false);
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine($"FON_AUTH: requests.php -> Status: {response.StatusCode}");

                    if (!string.IsNullOrWhiteSpace(json) && !json.TrimStart().StartsWith("<"))
                    {
                        var jObject = JObject.Parse(json);
                        var status = jObject["status"]?.Value<int>() ?? 0;
                        var apiStatus = jObject["api_status"]?.Value<int>() ?? 0;

                        if (status == 200 || apiStatus == 200)
                        {
                            var auth = jObject.ToObject<AuthObject>();
                            return auth != null ? (200, (dynamic)auth) : (200, (dynamic)json);
                        }

                        if (jObject["errors"] != null)
                        {
                            lastStatus = 400;
                            var errMsg = jObject["errors"]?.First?.ToString() ?? "Login failed";
                            lastRespond = new ErrorObject { Error = new ErrorObject.Errors { ErrorText = errMsg } };
                        }
                        else
                        {
                            var error = jObject.ToObject<ErrorObject>();
                            lastStatus = apiStatus == 0 ? 400 : apiStatus;
                            lastRespond = error ?? (dynamic)json;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FON_AUTH: requests.php exception: {ex.Message}");
                }

                // Phone API: only accepts username field, not email
                try
                {
                    using var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey ?? ""),
                        new KeyValuePair<string, string>("username", email),
                        new KeyValuePair<string, string>("password", password),
                        new KeyValuePair<string, string>("timezone", TimeZone ?? "UTC"),
                    });

                    var response = await client.PostAsync(phoneApiUrl, content).ConfigureAwait(false);
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine($"FON_AUTH: phone API -> Status: {response.StatusCode}");

                    if (!string.IsNullOrWhiteSpace(json) && !json.TrimStart().StartsWith("<"))
                    {
                        var jObject = JObject.Parse(json);
                        var statusText = jObject["api_status"]?.ToString();
                        long.TryParse(statusText, out long status);

                        if (status == 200)
                        {
                            var auth = jObject.ToObject<AuthObject>();
                            return auth != null ? (200, (dynamic)auth) : (200, (dynamic)json);
                        }

                        var error = jObject.ToObject<ErrorObject>();
                        lastStatus = status == 0 ? 400 : status;
                        lastRespond = error ?? (dynamic)json;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FON_AUTH: Phone API exception: {ex.Message}");
                }

                // Fallback: web API endpoints with username/email
                foreach (var authUrl in webUrls)
                {
                    foreach (var credentialKey in new[] { "username", "email" })
                    {
                        try
                        {
                            using var content = new FormUrlEncodedContent(new[]
                            {
                                new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey ?? ""),
                                new KeyValuePair<string, string>(credentialKey, email),
                                new KeyValuePair<string, string>("password", password),
                                new KeyValuePair<string, string>("timezone", TimeZone ?? "UTC"),
                                new KeyValuePair<string, string>("device_type", "phone"),
                                new KeyValuePair<string, string>("android_m_device_id", UserDetails.DeviceId ?? string.Empty),
                            });

                            var response = await client.PostAsync(authUrl, content).ConfigureAwait(false);
                            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            Console.WriteLine($"FON_AUTH: {authUrl} with '{credentialKey}' -> Status: {response.StatusCode}");

                            if (string.IsNullOrWhiteSpace(json) || json.TrimStart().StartsWith("<"))
                                continue;

                            var jObject = JObject.Parse(json);
                            var statusText = jObject["api_status"]?.ToString();
                            long.TryParse(statusText, out long status);

                            if (status == 200)
                            {
                                var auth = jObject.ToObject<AuthObject>();
                                return auth != null ? (200, (dynamic)auth) : (200, (dynamic)json);
                            }

                            var error = jObject.ToObject<ErrorObject>();
                            lastStatus = status == 0 ? 400 : status;
                            lastRespond = error ?? (dynamic)json;

                            if (credentialKey == "username" && (status == 3 || status == 5))
                                break;
                        }
                        catch (Exception attemptEx)
                        {
                            Console.WriteLine($"FON_AUTH: Auth attempt exception: {attemptEx.Message}");
                        }
                    }
                }

                return (lastStatus, lastRespond);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FON_AUTH: TryAuthDirectAsync exception: {ex.Message}");
                return (404, ex.Message);
            }
        }

        private async void GetTimezone()
        {
            try
            {
                if (Methods.CheckConnectivity())
                    TimeZone = await ApiRequest.GetTimeZoneAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async Task<(long, dynamic)> TryRegisterDirectAsync(string username, string password, string confirmPassword, string email, string gender, string phoneNum, string referral, string deviceId)
        {
            try
            {
                var url = "http://172.236.19.52/app_api.php?type=user_registration";
                using var handler = new Xamarin.Android.Net.AndroidMessageHandler();
                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                using var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey ?? ""),
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("email", email),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("confirm_password", confirmPassword),
                    new KeyValuePair<string, string>("gender", gender),
                    new KeyValuePair<string, string>("device_id", deviceId),
                    new KeyValuePair<string, string>("timezone", TimeZone ?? "UTC"),
                });

                var response = await client.PostAsync(url, content).ConfigureAwait(false);
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Console.WriteLine($"FON_AUTH: register direct -> Status: {response.StatusCode}");

                if (!string.IsNullOrWhiteSpace(json) && !json.TrimStart().StartsWith("<"))
                {
                    var jObject = JObject.Parse(json);
                    var apiStatus = jObject["api_status"]?.Value<int>() ?? 0;

                    if (apiStatus == 200)
                    {
                        var auth = jObject.ToObject<CreatAccountObject>();
                        return auth != null ? (200, (dynamic)auth) : (200, (dynamic)json);
                    }

                    if (jObject["errors"] != null)
                    {
                        var errObj = jObject["errors"];
                        var errText = errObj?.SelectToken("error_text")?.ToString() ?? errObj?.First?.ToString() ?? "Registration failed";
                        return (400, new ErrorObject { Error = new ErrorObject.Errors { ErrorText = errText } });
                    }

                    var error = jObject.ToObject<ErrorObject>();
                    return (apiStatus == 0 ? 400 : apiStatus, (dynamic)(error != null ? error : json));
                }

                return (404, "Server returned empty or invalid response");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FON_AUTH: TryRegisterDirectAsync exception: {ex.Message}");
                return (404, ex.Message);
            }
        }

        public async Task<(long, dynamic)> TryResetPasswordEmailDirectAsync(string email)
        {
            try
            {
                var url = "http://172.236.19.52/app_api.php?type=reset_pass&application=phone";
                using var handler = new Xamarin.Android.Net.AndroidMessageHandler();
                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                using var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("email", email),
                });

                var response = await client.PostAsync(url, content).ConfigureAwait(false);
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Console.WriteLine($"FON_AUTH: forgot pass direct -> Status: {response.StatusCode}");

                if (!string.IsNullOrWhiteSpace(json) && !json.TrimStart().StartsWith("<"))
                {
                    var jObject = JObject.Parse(json);
                    var apiStatus = jObject["api_status"]?.Value<int>() ?? 0;

                    if (apiStatus == 200)
                    {
                        return (200, jObject.ToObject<MessageObject>() ?? (dynamic)json);
                    }

                    if (jObject["errors"] != null)
                    {
                        var errObj = jObject["errors"];
                        var errText = errObj?.SelectToken("error_text")?.ToString() ?? errObj?.First?.ToString() ?? "Request failed";
                        return (400, new ErrorObject { Error = new ErrorObject.Errors { ErrorText = errText } });
                    }

                    var error = jObject.ToObject<ErrorObject>();
                    return (apiStatus == 0 ? 400 : apiStatus, (dynamic)(error != null ? error : json));
                }

                return (404, "Server returned empty or invalid response");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FON_AUTH: TryResetPasswordEmailDirectAsync exception: {ex.Message}");
                return (404, ex.Message);
            }
        }

        private async Task<bool> CheckIsActiveUser(string userId)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Auth.IsActiveUserAsync(userId);
                if (apiStatus == 200 && respond is MessageObject auth)
                {
                    Console.WriteLine(auth);
                    return true;
                }
                else if (apiStatus == 400)
                {
                    if (respond is ErrorObject error)
                    {
                        var errorText = error.Error.ErrorText;
                        var errorId = error.Error.ErrorId;
                        if (errorId == "5")
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ThisUserNotActive), GetText(Resource.String.Lbl_Ok));
                        else if (errorId == "4")
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_UserNotFound), GetText(Resource.String.Lbl_Ok));
                        else
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                    }
                }
                else if (apiStatus == 404)
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                }

                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

    }
}

































































































































































































































































































































































































































































































































































































































