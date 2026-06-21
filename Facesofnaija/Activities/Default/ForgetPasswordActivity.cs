using Android.App;
using static Facesofnaija.AppSettings;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Facesofnaija.Activities.Base;
using Facesofnaija.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Newtonsoft.Json.Linq;

namespace Facesofnaija.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ForgetPasswordActivity : BaseActivity
    {
        #region Variables Basic

        private EditText TxtEmail;
        private AppCompatButton BtnSend;
        private ProgressBar ProgressBar;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Window?.SetSoftInputMode(SoftInput.AdjustResize);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.ForgetPassword_Layout);

                //Get Value And Set Toolbar
                InitComponent();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                BtnSend = FindViewById<AppCompatButton>(Resource.Id.btnSend);
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
                    BtnSend.Click += BtnSendOnClick;
                }
                else
                {
                    BtnSend.Click -= BtnSendOnClick;
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
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //send 
        private async void BtnSendOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (string.IsNullOrEmpty(TxtEmail.Text.Replace(" ", "")))
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                if (!check)
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                HideKeyboard();

                ToggleVisibility(true);

                var (apiStatus, respond) = await TryResetPasswordDirectAsync(TxtEmail.Text.Replace(" ", ""));
                if (apiStatus == 200 && respond is MessageObject auth)
                {
                    ToggleVisibility(false);
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Email_Has_Been_Send), GetText(Resource.String.Lbl_Ok));
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
            catch (Exception exception)
            {
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private async Task<(long, dynamic)> TryResetPasswordDirectAsync(string email)
        {
            try
            {
                var url = AppSettings.SiteUrl + "/app_api.php?type=reset_pass&application=phone";
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
                    return (apiStatus == 0 ? 400 : apiStatus, (dynamic)(error != null ? (object)error : json));
                }

                return (404, "Server returned empty or invalid response");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FON_AUTH: TryResetPasswordDirectAsync exception: {ex.Message}");
                return (404, ex.Message);
            }
        }

        private void HideKeyboard()
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

        private void ToggleVisibility(bool isLoginProgress)
        {
            try
            {
                ProgressBar.Visibility = isLoginProgress ? ViewStates.Visible : ViewStates.Gone;
                BtnSend.Visibility = isLoginProgress ? ViewStates.Invisible : ViewStates.Visible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}