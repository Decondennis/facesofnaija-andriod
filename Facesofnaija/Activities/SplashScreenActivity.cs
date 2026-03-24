using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using System.Linq;
using System.Threading.Tasks;
using Facesofnaija.Activities.Communities.Groups;
using Facesofnaija.Activities.Communities.Pages;
using Facesofnaija.Activities.Default;
using Facesofnaija.Activities.NativePost.Pages;
using Facesofnaija.Activities.Tabbes;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace Facesofnaija.Activities
{
    [Activity(Icon = "@mipmap/icon", MainLauncher = true, NoHistory = true, Theme = "@style/SplashScreenTheme", Exported = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault }, DataSchemes = new[] { "http", "https" }, DataHost = "@string/ApplicationUrlWeb", AutoVerify = false)]
    [IntentFilter(new[] { Intent.ActionView, Intent.ActionMain }, Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault }, DataSchemes = new[] { "http", "https" }, DataHost = "@string/ApplicationUrlWeb", DataPathPrefixes = new[] { "/register", "/post/", "/index.php?link1=reset-password", "/index.php?link1=activate" }, AutoVerify = false)]
    public class SplashScreenActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                    Androidx.Core.Splashscreen.SplashScreen.InstallSplashScreen(this);

                base.OnCreate(savedInstanceState);

                Task startupWork = new Task(FirstRunExcite);
                startupWork.Start();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void FirstRunExcite()
        {
            try
            {
                switch (string.IsNullOrEmpty(AppSettings.Lang))
                {
                    case false:
                        LangController.SetApplicationLang(this, AppSettings.Lang);
                        break;
                    default:
#pragma warning disable 618
                        UserDetails.LangName = (int)Build.VERSION.SdkInt < 25 ? Resources?.Configuration?.Locale?.Language.ToLower() : Resources?.Configuration?.Locales.Get(0)?.Language.ToLower() ?? Resources?.Configuration?.Locale?.Language.ToLower();
#pragma warning restore 618
                        LangController.SetApplicationLang(this, UserDetails.LangName);
                        break;
                }

                if (Intent?.Data != null)
                {
                    if (Intent.Data.ToString()!.Contains("register") && string.IsNullOrEmpty(UserDetails.AccessToken))
                    {
                        //https://demo.wowonder.com/register?ref=waelanjo
                        var referral = Intent.Data.ToString()!.Split("?ref=")?.LastOrDefault() ?? "";

                        var intent = new Intent(this, typeof(RegisterActivity));
                        intent.PutExtra("Referral", referral);
                        StartActivity(intent);
                    }
                    else if (Intent.Data.ToString()!.Contains("index.php?link1=reset-password"))
                    {
                        //"https://demo.wowonder.com/index.php?link1=reset-password&code=161065_0e698bafecdf671358d2b507110f917e"
                        var code = Intent.Data.ToString()!.Split("&code=").LastOrDefault();

                        var intent = new Intent(this, typeof(ResetPasswordActivity));
                        intent.PutExtra("Code", code);
                        StartActivity(intent);
                    }
                    else if (Intent.Data.ToString()!.Contains("index.php?link1=activate"))
                    {
                        //https://demo.wowonder.com/index.php?link1=activate&email=wael.dev1994@gmail.com&code=7833d88964191faac34b5780e3ffe78a
                        var email = Intent.Data.ToString()!.Split("&email=")?.LastOrDefault()?.Split("&code=")?.FirstOrDefault();
                        var code = Intent.Data.ToString()!.Split("&code=")?.LastOrDefault();

                        var intent = new Intent(this, typeof(ValidationUserActivity));
                        intent.PutExtra("Code", code);
                        intent.PutExtra("Email", email);
                        StartActivity(intent);
                    }
                    else if (Intent.Data.ToString()!.Contains("post") && !string.IsNullOrEmpty(UserDetails.AccessToken))
                    {
                        //https://beta.wowonder.com/post/230744_.html
                        var postId = Intent.Data.ToString()!.Split("/")?.LastOrDefault()?.Replace("/", "")?.Split("_")?.FirstOrDefault();

                        var intent = new Intent(this, typeof(ViewFullPostActivity));
                        intent.PutExtra("Id", postId);
                        StartActivity(intent);
                    }
                    else if (!string.IsNullOrEmpty(Intent.Data.ToString()) && !string.IsNullOrEmpty(UserDetails.AccessToken)) //Other
                    {
                        var check = await CheckTypeUrl(Intent.Data.ToString());
                        if (!check)
                        {
                            switch (UserDetails.Status)
                            {
                                case "Active":
                                    StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                    break;
                                case "Pending":
                                    StartActivity(new Intent(this, typeof(LoginActivity)));
                                    break;
                                default:
                                    StartActivity(new Intent(this, typeof(LoginActivity)));
                                    break;
                            }
                        }
                    }
                    else
                    {
                        switch (UserDetails.Status)
                        {
                            case "Active":
                                StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                break;
                            case "Pending":
                                StartActivity(new Intent(this, typeof(LoginActivity)));
                                break;
                            default:
                                StartActivity(new Intent(this, typeof(LoginActivity)));
                                break;
                        }
                    }
                }
                else
                {
                    switch (UserDetails.Status)
                    {
                        case "Active":
                            StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                            break;
                        case "Pending":
                            StartActivity(new Intent(this, typeof(LoginActivity)));
                            break;
                        default:
                            StartActivity(new Intent(this, typeof(LoginActivity)));
                            break;
                    }
                }

                OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task<bool> CheckTypeUrl(string url)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var username = url.Split("/").LastOrDefault();
                    var (apiStatus, respond) = await RequestsAsync.Global.CheckTypeUrlAsync(username);
                    if (apiStatus == 200)
                    {
                        if (respond is TypeUrlObject result)
                        {
                            switch (result.Type)
                            {
                                case "user":
                                    WoWonderTools.OpenProfile(this, result.Id, null);
                                    return true;
                                case "page":
                                    {
                                        var intent = new Intent(this, typeof(PageProfileActivity));
                                        //intent.PutExtra("PageObject", JsonConvert.SerializeObject(item));
                                        intent.PutExtra("PageId", result.Id);
                                        StartActivity(intent);
                                        return true;
                                    }
                                case "group":
                                    {
                                        var intent = new Intent(this, typeof(GroupProfileActivity));
                                        //intent.PutExtra("GroupObject", JsonConvert.SerializeObject(item));
                                        intent.PutExtra("GroupId", result.Id);
                                        StartActivity(intent);
                                        return true;
                                    }
                                default:
                                    return false;
                            }
                        }
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
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