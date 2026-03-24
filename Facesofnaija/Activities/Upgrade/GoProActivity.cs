using Android.App;
using Android.BillingClient.Api;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using Com.Razorpay;
using InAppBilling.Lib;
using Google.Android.Material.Dialog; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Facesofnaija.Activities.Suggested.User;
using Facesofnaija.Activities.Tabbes;
using Facesofnaija.Activities.Upgrade.Adapters;
using Facesofnaija.Activities.WalkTroutPage;
using Facesofnaija.Helpers.Fonts;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Payment;
using Facesofnaija.PaymentGoogle;
using Facesofnaija.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Payments;
using WoWonderClient.Requests;
using BaseActivity = Facesofnaija.Activities.Base.BaseActivity;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.Upgrade
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
    public class GoProActivity : BaseActivity, IDialogListCallBack, IDialogInputCallBack, IPaymentResultWithDataListener, IBillingPaymentListener
    {
        #region Variables Basic

        private RecyclerView MainRecyclerView, MainPlansRecyclerView;
        private GridLayoutManager LayoutManagerView;
        private LinearLayoutManager PlansLayoutManagerView;
        private GoProFeaturesAdapter FeaturesAdapter;
        private UpgradeGoProAdapter PlansAdapter;
        private InitPayPalPayment InitPayPalPayment;
        private BillingSupport BillingSupport;
        private InitRazorPayPayment InitRazorPay;
        private InitPayStackPayment PayStackPayment;
        private InitCashFreePayment CashFreePayment;
        private InitPaySeraPayment PaySeraPayment;
        private ImageView IconClose;
        private string Caller, PayId, Price, PayType;
        private UpgradeGoProClass ItemUpgrade;
        private string TypeDialog;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.GoProLayout);

                Caller = Intent?.GetStringExtra("class") ?? "";

                //Get Value And Set Toolbar
                InitBuy();
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
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

        protected override void OnDestroy()
        {
            try
            {
                if (AppSettings.ShowInAppBilling && InitializeWoWonder.IsExtended)
                    BillingSupport?.Destroy();

                switch (AppSettings.ShowRazorPay)
                {
                    case true:
                        InitRazorPay?.StopRazorPay();
                        break;
                }

                switch (AppSettings.ShowPayStack)
                {
                    case true:
                        PayStackPayment?.StopPayStack();
                        break;
                }

                switch (AppSettings.ShowCashFree)
                {
                    case true:
                        CashFreePayment?.StopCashFree();
                        break;
                }

                switch (AppSettings.ShowPaySera)
                {
                    case true:
                        PaySeraPayment?.StopPaySera();
                        break;
                }

                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    FinishPage();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitBuy()
        {
            try
            {
                if (AppSettings.ShowInAppBilling && InitializeWoWonder.IsExtended)
                    BillingSupport = new BillingSupport(this, InAppBillingGoogle.ProductId, AppSettings.TripleDesAppServiceProvider, InAppBillingGoogle.ListProductSku, this);

                InitPayPalPayment = AppSettings.ShowPaypal switch
                {
                    true => new InitPayPalPayment(this),
                    _ => InitPayPalPayment
                };

                InitRazorPay = AppSettings.ShowRazorPay switch
                {
                    true => new InitRazorPayPayment(this),
                    _ => InitRazorPay
                };

                PayStackPayment = AppSettings.ShowPayStack switch
                {
                    true => new InitPayStackPayment(this),
                    _ => PayStackPayment
                };

                CashFreePayment = AppSettings.ShowCashFree switch
                {
                    true => new InitCashFreePayment(this),
                    _ => CashFreePayment
                };

                PaySeraPayment = AppSettings.ShowPaySera switch
                {
                    true => new InitPaySeraPayment(this),
                    _ => PaySeraPayment
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitComponent()
        {
            try
            {
                MainRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler);
                MainPlansRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler2);
                IconClose = FindViewById<ImageView>(Resource.Id.iv1);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_Go_Pro);
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(false);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);


                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                FeaturesAdapter = new GoProFeaturesAdapter(this);
                LayoutManagerView = new GridLayoutManager(this, 3);
                MainRecyclerView.SetLayoutManager(LayoutManagerView);
                MainRecyclerView.HasFixedSize = true;
                MainRecyclerView.SetAdapter(FeaturesAdapter);

                PlansAdapter = new UpgradeGoProAdapter(this);
                PlansLayoutManagerView = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MainPlansRecyclerView.SetLayoutManager(PlansLayoutManagerView);
                MainPlansRecyclerView.HasFixedSize = true;
                MainPlansRecyclerView.SetAdapter(PlansAdapter);
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
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        PlansAdapter.UpgradeButtonItemClick += PlansAdapterOnItemClick;
                        IconClose.Click += IconCloseOnClick;
                        break;
                    default:
                        PlansAdapter.UpgradeButtonItemClick -= PlansAdapterOnItemClick;
                        IconClose.Click -= IconCloseOnClick;
                        break;
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
                MainRecyclerView = null!;
                MainPlansRecyclerView = null!;
                LayoutManagerView = null!;
                PlansLayoutManagerView = null!;
                FeaturesAdapter = null!;
                PlansAdapter = null!;
                InitPayPalPayment = null!;
                InitRazorPay = null!;
                PayStackPayment = null!;
                PayStackPayment = null!;
                BillingSupport = null!;
                IconClose = null!;
                PayId = null!;
                Price = null!;
                PayType = null!;
                ItemUpgrade = null!;
                PaySeraPayment = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void PlansAdapterOnItemClick(object sender, UpgradeGoProAdapterClickEventArgs e)
        {
            try
            {
                switch (e.Position)
                {
                    case > -1:
                        {
                            ItemUpgrade = PlansAdapter.GetItem(e.Position);
                            if (ItemUpgrade != null)
                            {
                                var arrayAdapter = new List<string>();
                                var dialogList = new MaterialAlertDialogBuilder(this);

                                switch (AppSettings.ShowInAppBilling)
                                {
                                    case true when InitializeWoWonder.IsExtended:
                                        arrayAdapter.Add(GetString(Resource.String.Btn_GooglePlay));
                                        break;
                                }

                                switch (AppSettings.ShowPaypal)
                                {
                                    case true:
                                        arrayAdapter.Add(GetString(Resource.String.Btn_Paypal));
                                        break;
                                }

                                switch (AppSettings.ShowCreditCard)
                                {
                                    case true:
                                        arrayAdapter.Add(GetString(Resource.String.Lbl_CreditCard));
                                        break;
                                }

                                switch (AppSettings.ShowBankTransfer)
                                {
                                    case true:
                                        arrayAdapter.Add(GetString(Resource.String.Lbl_BankTransfer));
                                        break;
                                }

                                switch (AppSettings.ShowRazorPay)
                                {
                                    case true:
                                        arrayAdapter.Add(GetString(Resource.String.Lbl_RazorPay));
                                        break;
                                }

                                switch (AppSettings.ShowPayStack)
                                {
                                    case true:
                                        arrayAdapter.Add(GetString(Resource.String.Lbl_PayStack));
                                        break;
                                }

                                switch (AppSettings.ShowCashFree)
                                {
                                    case true:
                                        arrayAdapter.Add(GetString(Resource.String.Lbl_CashFree));
                                        break;
                                }

                                switch (AppSettings.ShowPaySera)
                                {
                                    case true:
                                        arrayAdapter.Add(GetString(Resource.String.Lbl_PaySera));
                                        break;
                                }

                                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                                
                                dialogList.Show();
                            }

                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Close
        private void IconCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                FinishPage();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public async void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == GetString(Resource.String.Btn_Paypal))
                {
                    Price = ItemUpgrade.PlanPrice;
                    PayType = "membership";
                    PayId = ItemUpgrade.Id;
                    InitPayPalPayment.BtnPaypalOnClick(Price, "membership", ItemUpgrade.Id);
                }
                else if (text == GetString(Resource.String.Btn_GooglePlay))
                {
                    Price = ItemUpgrade.PlanPrice;
                    PayId = ItemUpgrade.Id;

                    string type = "";
                    switch (PayId)
                    {
                        case "1":
                            type = InAppBillingGoogle.MembershipStar;
                            break;
                        case "2":
                            type = InAppBillingGoogle.MembershipHot;
                            break;
                        case "3":
                            type = InAppBillingGoogle.MembershipUltima;
                            break;
                        case "4":
                            type = InAppBillingGoogle.MembershipVip;
                            break;
                    }

                    BillingSupport?.PurchaseNow(type);
                }
                else if (text == GetString(Resource.String.Lbl_CreditCard))
                {
                    OpenIntentCreditCard();
                }
                else if (text == GetString(Resource.String.Lbl_BankTransfer))
                {
                    OpenIntentBankTransfer();
                }
                else if (text == GetString(Resource.String.Lbl_RazorPay))
                {
                    Price = ItemUpgrade.PlanPrice;
                    PayId = ItemUpgrade.Id;

                    InitRazorPay?.BtnRazorPayOnClick(Price, "membership", PayId);
                }
                else if (text == GetString(Resource.String.Lbl_PayStack))
                {
                    Price = ItemUpgrade.PlanPrice;
                    PayId = ItemUpgrade.Id;

                    TypeDialog = "PayStack";
                    var dialogBuilder = new MaterialAlertDialogBuilder(this);
                    dialogBuilder.SetTitle(Resource.String.Lbl_PayStack);

                    EditText input = new EditText(this);
                    input.SetHint(Resource.String.Lbl_Email);
                    input.InputType = InputTypes.TextVariationEmailAddress;
                    LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                    input.LayoutParameters = lp;

                    dialogBuilder.SetView(input);

                    dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_PayNow), new MaterialDialogUtils(input, this));
                    dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                    
                    dialogBuilder.Show();
                }
                else if (text == GetString(Resource.String.Lbl_CashFree))
                {
                    OpenCashFreeDialog();
                }
                else if (text == GetString(Resource.String.Lbl_PaySera))
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short);

                    await PaySera();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnInput(IDialogInterface dialog, string input)
        {
            try
            {
                switch (input.Length)
                {
                    case <= 0:
                        return;
                }

                if (TypeDialog == "PayStack")
                {
                    try
                    { 
                        var check = Methods.FunString.IsEmailValid(input.Replace(" ", ""));
                        switch (check)
                        {
                            case false:
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                return;
                            default:
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short);

                                await PayStack(input);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentCreditCard()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                intent.PutExtra("Id", ItemUpgrade.Id);
                intent.PutExtra("Price", ItemUpgrade.PlanPrice);
                intent.PutExtra("payType", "membership");
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentBankTransfer()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PaymentLocalActivity));
                intent.PutExtra("Id", ItemUpgrade.Id);
                intent.PutExtra("Price", ItemUpgrade.PlanPrice);
                intent.PutExtra("payType", "membership");
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPaymentError(int code, string razorpayPaymentId, PaymentData paymentData)
        {
            try
            {
                Console.WriteLine("razorpay : Payment failed: " + code + " " + paymentData.Data);
                Toast.MakeText(this, "Payment failed: " + paymentData.Data, ToastLength.Long)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnPaymentSuccess(string razorpayPaymentId, PaymentData paymentData)
        {
            try
            {
                Console.WriteLine("razorpay : Payment Successful:" + razorpayPaymentId);

                switch (string.IsNullOrEmpty(razorpayPaymentId))
                {
                    case false when Methods.CheckConnectivity():
                        {
                            var type = PayId switch
                            {
                                "1" => "week",
                                "2" => "month",
                                "3" => "year",
                                "4" => "life-time",
                                _ => ""
                            };
                            var keyValues = new Dictionary<string, string>
                        {
                            {"type", type}, //week,year,month,life-time 
                        };

                            var (apiStatus, respond) = await RequestsAsync.Payments.RazorPayAsync(razorpayPaymentId, "upgrade", keyValues).ConfigureAwait(false);
                            switch (apiStatus)
                            {
                                case 200:
                                    RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                            if (dataUser != null)
                                            {
                                                dataUser.IsPro = "1";

                                                var sqlEntity = new SqLiteDatabase();
                                                sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);

                                            }

                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Upgraded), ToastLength.Long);
                                            FinishPage();
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                    break;
                                default:
                                    Methods.DisplayReportResult(this, respond);
                                    break;
                            }

                            break;
                        }
                    case false:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PayStack(string email)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"email", email},
                    };

                    var type = PayId switch
                    {
                        "1" => "week",
                        "2" => "month",
                        "3" => "year",
                        "4" => "life-time",
                        _ => ""
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializePayStackAsync(type, keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case InitializePaymentObject result:
                                        {
                                            PayStackPayment ??= new InitPayStackPayment(this);
                                            PayStackPayment.DisplayPayStackPayment(result.Url, "membership", Price, PayId);
                                            break;
                                        }
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private EditText TxtName, TxtEmail, TxtPhone;
        private void OpenCashFreeDialog()
        {
            try
            {
                var dialog = new MaterialAlertDialogBuilder(this);

                dialog.SetTitle(Resource.String.Lbl_CashFree);

                View view = LayoutInflater.Inflate(Resource.Layout.CashFreePaymentLayout, null);
                dialog.SetView(view);
                dialog.SetPositiveButton(GetText(Resource.String.Lbl_PayNow), async (o, args) =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                            return;
                        }

                        var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                        switch (check)
                        {
                            case false:
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                return;
                        }

                        if (string.IsNullOrEmpty(TxtPhone.Text) || string.IsNullOrWhiteSpace(TxtPhone.Text))
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short);
                            return;
                        }

                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short);

                        await CashFree(TxtName.Text, TxtEmail.Text, TxtPhone.Text);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                var iconName = view.FindViewById<TextView>(Resource.Id.IconName);
                TxtName = view.FindViewById<EditText>(Resource.Id.NameEditText);

                var iconEmail = view.FindViewById<TextView>(Resource.Id.IconEmail);
                TxtEmail = view.FindViewById<EditText>(Resource.Id.EmailEditText);

                var iconPhone = view.FindViewById<TextView>(Resource.Id.IconPhone);
                TxtPhone = view.FindViewById<EditText>(Resource.Id.PhoneEditText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconName, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconEmail, FontAwesomeIcon.PaperPlane);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconPhone, FontAwesomeIcon.Mobile);

                Methods.SetColorEditText(TxtName, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEmail, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPhone, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                var local = ListUtils.MyProfileList?.FirstOrDefault();
                if (local != null)
                {
                    TxtName.Text = WoWonderTools.GetNameFinal(local);
                    TxtEmail.Text = local.Email;
                    TxtPhone.Text = local.PhoneNumber;
                }

                dialog.Show(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task CashFree(string name, string email, string phone)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var type = PayId switch
                    {
                        "1" => "week",
                        "2" => "month",
                        "3" => "year",
                        "4" => "life-time",
                        _ => ""
                    };

                    var keyValues = new Dictionary<string, string>
                    {
                        {"name", name},
                        {"phone", phone},
                        {"email", email},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializeCashFreeAsync(type, AppSettings.CashFreeCurrency, ListUtils.SettingsSiteList?.CashfreeSecretKey ?? "", ListUtils.SettingsSiteList?.CashfreeMode, keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case CashFreeObject result:
                                        CashFreePayment ??= new InitCashFreePayment(this);
                                        CashFreePayment.DisplayCashFreePayment(result, "membership", Price, PayId);
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PaySera()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var type = PayId switch
                    {
                        "1" => "week",
                        "2" => "month",
                        "3" => "year",
                        "4" => "life-time",
                        _ => ""
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializePaySeraAsync(type, new Dictionary<string, string>());
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case InitializePaymentObject result:
                                        PaySeraPayment ??= new InitPaySeraPayment(this);
                                        PaySeraPayment.DisplayPaySeraPayment(result.Url, "membership", Price, PayId);
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        private void FinishPage()
        {
            try
            {
                switch (Caller)
                {
                    case "register" when AppSettings.ShowSuggestedUsersOnRegister:
                        {
                            Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                            newIntent?.PutExtra("class", "register");
                            StartActivity(newIntent);
                            break;
                        }
                    case "register":
                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                        break;
                    case "login" when AppSettings.ShowWalkTroutPage:
                        {
                            Intent newIntent = new Intent(this, typeof(WalkTroutActivity));
                            newIntent?.PutExtra("class", "login");
                            StartActivity(newIntent);
                            break;
                        }
                    case "login":
                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                        break;
                }

                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async Task SetProAsync()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.SetProAsync(PayId);
                    switch (apiStatus)
                    {
                        case 200:
                            var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                            if (dataUser != null)
                            {
                                dataUser.IsPro = "1";

                                var sqlEntity = new SqLiteDatabase();
                                sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);

                            }

                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Upgraded), ToastLength.Long);
                            FinishPage();
                            break;
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Billing

        public void OnPaymentError(string error)
        {
            Console.WriteLine(error);
        }

        public async void OnPaymentSuccess(IList<Purchase> result)
        {
            await SetProAsync();
        }

        #endregion

    }
}