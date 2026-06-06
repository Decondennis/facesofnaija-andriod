using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using System;
using System.Linq;
using System.Reflection;
using Facesofnaija.Helpers.Utils;

namespace Facesofnaija.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class LoginActivity : SocialLoginBaseActivity
    {
        #region Variables Basic

        private EditText TxtEmail, TxtPassword;
        private TextView TxtForgotPassword;
        private Button BtnLogin;
        private ImageView ImageShowPass;
        private ProgressBar ProgressBar;
        private TextView LayoutCreateAccount;

        private CheckBox ChkRemember;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                // Create your application here
                var layoutSet = TrySetLoginLayout();
                if (!layoutSet)
                {
                    var root = new LinearLayout(this)
                    {
                        Orientation = Orientation.Vertical
                    };
                    root.SetBackgroundColor(Android.Graphics.Color.White);
                    root.SetPadding(40, 120, 40, 40);

                    var text = new TextView(this)
                    {
                        Text = "Login screen resources failed to load.\nPlease restart the app.",
                        TextSize = 16
                    };
                    text.SetTextColor(Android.Graphics.Color.Black);
                    root.AddView(text);

                    SetContentView(root);
                    return;
                }

                //Get Value And Set Toolbar
                try
                {
                    InitComponent();
                }
                catch (Exception initEx)
                {
                    Methods.DisplayReportResultTrack(initEx);
                }

                try
                {
                    InitSocialLogins();
                }
                catch (Exception socialEx)
                {
                    Methods.DisplayReportResultTrack(socialEx);
                }

                if (AppSettings.EnableSmartLockForPasswords)
                {
                    try
                    {
                        BuildClients();
                    }
                    catch (Exception clientEx)
                    {
                        Methods.DisplayReportResultTrack(clientEx);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                try
                {
                    var root = new LinearLayout(this)
                    {
                        Orientation = Orientation.Vertical
                    };
                    root.SetBackgroundColor(Android.Graphics.Color.White);
                    root.SetPadding(40, 120, 40, 40);

                    var text = new TextView(this)
                    {
                        Text = "Login initialization failed.\nError: " + e.Message,
                        TextSize = 14
                    };
                    text.SetTextColor(Android.Graphics.Color.Red);
                    root.AddView(text);

                    SetContentView(root);
                }
                catch
                {
                    // Last resort - finish activity
                    Finish();
                }
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                TxtEmail = FindViewById<EditText>(Resource.Id.EmailEditText);
                TxtPassword = FindViewById<EditText>(Resource.Id.PasswordEditText);

                ImageShowPass = FindViewById<ImageView>(Resource.Id.imageShowPass);
                if (ImageShowPass != null) ImageShowPass.Tag = "hide";

                ChkRemember = FindViewById<CheckBox>(Resource.Id.checkRememberMe);
                if (ChkRemember != null) ChkRemember.Checked = true;

                TxtForgotPassword = FindViewById<TextView>(Resource.Id.textForgotPassword);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                BtnLogin = FindViewById<Button>(Resource.Id.btnLogin);

                LayoutCreateAccount = FindViewById<TextView>(Resource.Id.layout_create_account);
                if (LayoutCreateAccount != null)
                    LayoutCreateAccount.Visibility = AppSettings.EnableRegisterSystem == false ? ViewStates.Gone : ViewStates.Visible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    if (BtnLogin != null) BtnLogin.Click += BtnLoginOnClick;
                    if (TxtForgotPassword != null) TxtForgotPassword.Click += TxtForgotPasswordOnClick;
                    if (ImageShowPass != null) ImageShowPass.Click += ImageShowPassOnClick;
                    if (LayoutCreateAccount != null) LayoutCreateAccount.Click += LayoutCreateAccountOnClick;
                }
                else
                {
                    if (BtnLogin != null) BtnLogin.Click -= BtnLoginOnClick;
                    if (TxtForgotPassword != null) TxtForgotPassword.Click -= TxtForgotPasswordOnClick;
                    if (ImageShowPass != null) ImageShowPass.Click -= ImageShowPassOnClick;
                    if (LayoutCreateAccount != null) LayoutCreateAccount.Click -= LayoutCreateAccountOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                TxtEmail = null!;
                TxtPassword = null!;
                TxtForgotPassword = null!;
                BtnLogin = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private bool TrySetLoginLayout()
        {
            try
            {
                var loginId = Resources?.GetIdentifier("loginlayout", "layout", PackageName) ?? 0;
                if (loginId > 0)
                {
                    SetContentView(loginId);
                    return true;
                }

                var safeId = Resources?.GetIdentifier("loginlayout_safe", "layout", PackageName) ?? 0;
                if (safeId > 0)
                {
                    SetContentView(safeId);
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        #endregion

        #region Events

        //Show Password 
        private void ImageShowPassOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ImageShowPass.Tag?.ToString() == "hide")
                {
                    ImageShowPass.SetImageResource(Resource.Drawable.icon_eyes_vector);
                    ImageShowPass.Tag = "show";
                    TxtPassword.InputType = Android.Text.InputTypes.TextVariationNormal | Android.Text.InputTypes.ClassText;
                    TxtPassword.SetSelection(TxtPassword.Text.Length);
                }
                else
                {
                    ImageShowPass.SetImageResource(Resource.Drawable.ic_eye_hide);
                    ImageShowPass.Tag = "hide";
                    TxtPassword.InputType = Android.Text.InputTypes.TextVariationPassword | Android.Text.InputTypes.ClassText;
                    TxtPassword.SetSelection(TxtPassword.Text.Length);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Forgot Password
        private void TxtForgotPasswordOnClick(object sender, EventArgs e)
        {
            try
            {
                HideKeyboard();
                StartActivity(new Intent(this, typeof(ForgetPasswordActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //start login 
        private async void BtnLoginOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                var emailOrUsername = TxtEmail.Text?.Trim() ?? string.Empty;
                var password = TxtPassword.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(emailOrUsername) || string.IsNullOrEmpty(password))
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                HideKeyboard();

                ToggleVisibility(true);
                await AuthApi(emailOrUsername, password, ChkRemember.Checked);
            }
            catch (Exception exception)
            {
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //CreateAccount
        private void LayoutCreateAccountOnClick(object sender, EventArgs e)
        {
            try
            {
                HideKeyboard();
                StartActivity(new Intent(this, typeof(RegisterActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        public override void ToggleVisibility(bool isLoginProgress)
        {
            try
            {
                ProgressBar.Visibility = isLoginProgress ? ViewStates.Visible : ViewStates.Gone;
                BtnLogin.Visibility = isLoginProgress ? ViewStates.Invisible : ViewStates.Visible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}