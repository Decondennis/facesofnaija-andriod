using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Activity.Result;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Com.Canhub.Cropper;
using Google.Android.Material.Dialog; 
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Facesofnaija.Activities.AddPost.Adapters;
using Facesofnaija.Activities.Base;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Fonts;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.Market
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditProductActivity : BaseActivity, View.IOnFocusChangeListener, IDialogListCallBack, IActivityResultCallback
    {
        #region Variables Basic

        private TextView TxtAdd, IconTitle, IconPrice, IconLocation, IconCategories, IconAbout, IconType;
        private EditText TxtTitle, TxtPrice, TxtCurrency, TxtLocation, TxtAbout, TxtCategory;
        private RadioButton RbNew, RbUsed;
        private string CategoryId = "", CurrencyId = "", ProductType = "", PlaceText = "", TypeDialog = "", DeletedImagesIds = "";
        private PublisherAdView PublisherAdView;
        private ProductDataObject ProductData;
        private AttachmentsAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private DialogGalleryController GalleryController;

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
                SetContentView(Resource.Layout.EditProductLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                GalleryController = new DialogGalleryController(this, this);

                Get_Data_Product();
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
                PublisherAdView?.Resume();
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
                PublisherAdView?.Pause();
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
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                TxtAdd = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtAdd.Text = GetText(Resource.String.Lbl_Save);

                IconTitle = FindViewById<TextView>(Resource.Id.IconTitle);
                TxtTitle = FindViewById<EditText>(Resource.Id.TitleEditText);
                IconPrice = FindViewById<TextView>(Resource.Id.IconPrice);
                TxtPrice = FindViewById<EditText>(Resource.Id.PriceEditText);
                TxtCurrency = FindViewById<EditText>(Resource.Id.CurrencyEditText);
                IconLocation = FindViewById<TextView>(Resource.Id.IconLocation);
                TxtLocation = FindViewById<EditText>(Resource.Id.LocationEditText);
                IconCategories = FindViewById<TextView>(Resource.Id.IconCategories);
                TxtCategory = FindViewById<EditText>(Resource.Id.CategoriesEditText);
                IconAbout = FindViewById<TextView>(Resource.Id.IconAbout);
                TxtAbout = FindViewById<EditText>(Resource.Id.AboutEditText);
                IconType = FindViewById<TextView>(Resource.Id.IconType);
                RbNew = FindViewById<RadioButton>(Resource.Id.radioNew);
                RbUsed = FindViewById<RadioButton>(Resource.Id.radioUsed);

                MRecycler = (RecyclerView)FindViewById(Resource.Id.imageRecyler);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTitle, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconLocation, FontAwesomeIcon.MapMarkedAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconPrice, FontAwesomeIcon.MoneyBillAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconAbout, FontAwesomeIcon.Paragraph);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeBrands, IconCategories, FontAwesomeIcon.Buromobelexperte);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconType, FontAwesomeIcon.LayerPlus);

                Methods.SetColorEditText(TxtTitle, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPrice, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCurrency, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocation, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCategory, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAbout, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtCategory);
                Methods.SetFocusable(TxtCurrency);

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);
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
                    toolBar.Title = GetText(Resource.String.Lbl_EditProduct);
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
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
                MAdapter = new AttachmentsAdapter(this) { AttachmentList = new ObservableCollection<Attachments>() };
                LayoutManager = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);

                MRecycler.Visibility = ViewStates.Visible;

                // Add first image Default 
                var attach = new Attachments
                {
                    Id = MAdapter.AttachmentList.Count + 1,
                    TypeAttachment = "Default",
                    FileSimple = "addImage",
                    FileUrl = "addImage"
                };

                MAdapter.Add(attach);
                MAdapter.NotifyDataSetChanged();
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
                        MAdapter.DeleteItemClick += MAdapterOnDeleteItemClick;
                        MAdapter.ItemClick += MAdapterOnItemClick;
                        RbNew.CheckedChange += RbNewOnCheckedChange;
                        RbUsed.CheckedChange += RbUsedOnCheckedChange;
                        TxtAdd.Click += TxtAddOnClick;
                        TxtLocation.OnFocusChangeListener = this;
                        TxtCategory.Touch += TxtCategoryOnClick;
                        TxtCurrency.Touch += TxtCurrencyOnTouch;
                        break;
                    default:
                        MAdapter.DeleteItemClick -= MAdapterOnDeleteItemClick;
                        MAdapter.ItemClick -= MAdapterOnItemClick;
                        RbNew.CheckedChange -= RbNewOnCheckedChange;
                        RbUsed.CheckedChange -= RbUsedOnCheckedChange;
                        TxtAdd.Click -= TxtAddOnClick;
                        TxtLocation.OnFocusChangeListener = null!;
                        TxtCategory.Touch -= TxtCategoryOnClick;
                        TxtCurrency.Touch -= TxtCurrencyOnTouch;
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
                PublisherAdView?.Destroy();

                TxtAdd = null!;
                IconTitle = null!;
                TxtTitle = null!;
                IconPrice = null!;
                TxtPrice = null!;
                TxtCurrency = null!;
                IconLocation = null!;
                TxtLocation = null!;
                IconCategories = null!;
                TxtCategory = null!;
                IconAbout = null!;
                TxtAbout = null!;
                IconType = null!;
                RbNew = null!;
                RbUsed = null!;
                MAdapter = null!;
                MRecycler = null!;
                LayoutManager = null!;
                PublisherAdView = null!;
                CategoryId = "";
                CurrencyId = "";
                ProductType = "";
                PlaceText = "";
                TypeDialog = "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void MAdapterOnDeleteItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                switch (position)
                {
                    case >= 0:
                        {
                            var item = MAdapter.GetItem(position);
                            if (item != null)
                            {
                                DeletedImagesIds += item.Id + ",";
                                MAdapter.Remove(item);
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


        private void MAdapterOnItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                switch (position)
                {
                    case >= 0:
                        {
                            var item = MAdapter.GetItem(position);
                            switch (item)
                            {
                                case null:
                                    return;
                            }
                            if (item.TypeAttachment != "Default") return;
                            GalleryController?.OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtCurrencyOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                if (ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList != null)
                {
                    TypeDialog = "Currency";

                    var arrayAdapter = WoWonderTools.GetCurrencySymbolList();
                    switch (arrayAdapter?.Count)
                    {
                        case > 0:
                            {
                                var dialogList = new MaterialAlertDialogBuilder(this);

                                dialogList.SetTitle(GetText(Resource.String.Lbl_SelectCurrency));
                                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                                
                                dialogList.Show();
                                break;
                            }
                    }
                }
                else
                {
                    Methods.DisplayReportResult(this, "Not have List Currency Products");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtCategoryOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                switch (CategoriesController.ListCategoriesProducts.Count)
                {
                    case > 0:
                        {
                            TypeDialog = "Categories";

                            var dialogList = new MaterialAlertDialogBuilder(this);

                            var arrayAdapter = CategoriesController.ListCategoriesProducts.Select(item => item.CategoriesName).ToList();

                            dialogList.SetTitle(GetText(Resource.String.Lbl_SelectCategories));
                            dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                            dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                            
                            dialogList.Show();
                            break;
                        }
                    default:
                        Methods.DisplayReportResult(this, "Not have List Categories Products");
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtLocationOnFocusChange()
        {
            try
            {
                switch ((int)Build.VERSION.SdkInt)
                {
                    // Check if we're running on Android 5.0 or higher
                    case < 23:
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                        break;
                    default:
                        {
                            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted && ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                            {
                                //Open intent Location when the request code of result is 502
                                new IntentController(this).OpenIntentLocation();
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(105);
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

        private async void TxtAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtTitle.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtPrice.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_price), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtLocation.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Location), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtAbout.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_about), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtCurrency.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_Currency), ToastLength.Short);
                        return;
                    }

                    var list = MAdapter.AttachmentList.Where(a => a.TypeAttachment != "Default").ToList();
                    switch (list.Count)
                    {
                        case 0:
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                            break;
                        default:
                            {
                                //Show a progress
                                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                                DeletedImagesIds = string.IsNullOrEmpty(DeletedImagesIds) switch
                                {
                                    false => DeletedImagesIds.Remove(DeletedImagesIds.Length - 1, 1),
                                    _ => DeletedImagesIds
                                };

                                var (currency, currencyIcon) = WoWonderTools.GetCurrency(ProductData.Currency);
                                Console.WriteLine(currency);
                                var price = TxtPrice.Text.Replace(currencyIcon, "").Replace(" ", "");
                                var (apiStatus, respond) = await RequestsAsync.Market.EditProductAsync(ProductData.Id, TxtTitle.Text, TxtAbout.Text, TxtLocation.Text, price, CurrencyId, CategoryId, ProductType, list, DeletedImagesIds);
                                switch (apiStatus)
                                {
                                    case 200:
                                        {
                                            switch (respond)
                                            {
                                                case MessageObject result:
                                                    {
                                                        AndHUD.Shared.Dismiss(this);
                                                        Console.WriteLine(result.Message);
                                                        var listImage = list.Select(productPathImage => new Images { Id = "", ProductId = ProductData.Id, Image = productPathImage.FileSimple, ImageOrg = productPathImage.FileSimple }).ToList();

                                                        var user = ListUtils.MyProfileList?.FirstOrDefault();

                                                        var instance = TabbedMarketActivity.GetInstance();
                                                        var data = instance?.MyProductsTab?.MAdapter?.MarketList?.FirstOrDefault(a => a.Product.Id == ProductData.Id && a.Type == Classes.ItemType.MyProduct);
                                                        if (data != null)
                                                        {
                                                            data.Product.Id = ProductData.Id;
                                                            data.Product.Name = TxtTitle.Text;
                                                            data.Product.UserId = UserDetails.UserId;
                                                            data.Product.Location = TxtLocation.Text;
                                                            data.Product.Description = TxtAbout.Text;
                                                            data.Product.Category = CategoryId;
                                                            data.Product.Images = listImage;
                                                            data.Product.Price = TxtPrice.Text;
                                                            data.Product.Type = ProductType;
                                                            data.Product.Seller = user;

                                                            instance.MyProductsTab.MAdapter.NotifyDataSetChanged();

                                                            Intent intent = new Intent();
                                                            intent.PutExtra("itemData", JsonConvert.SerializeObject(data));
                                                            SetResult(Result.Ok, intent);
                                                        }

                                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ProductSuccessfullyEdited), ToastLength.Short);

                                                        Finish();
                                                        break;
                                                    }
                                            }

                                            break;
                                        }
                                    default:
                                        Methods.DisplayAndHudErrorResult(this, respond);
                                        break;
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RbUsedOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var isChecked = RbUsed.Checked;
                switch (isChecked)
                {
                    case true:
                        RbNew.Checked = false;
                        ProductType = "1";
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RbNewOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var isChecked = RbNew.Checked;
                switch (isChecked)
                {
                    case true:
                        RbUsed.Checked = false;
                        ProductType = "0";
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions 

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                switch (requestCode)
                {
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        GalleryController?.OpenDialogGallery();
                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 105 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                        break;
                    case 105:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

       public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "Categories":
                        CategoryId = CategoriesController.ListCategoriesProducts.FirstOrDefault(categories => categories.CategoriesName == itemString)?.CategoriesId;
                        TxtCategory.Text = itemString;
                        break;
                    default:
                        TxtCurrency.Text = itemString;
                        CurrencyId = WoWonderTools.GetIdCurrency(itemString);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        private void GetPlaceFromPicker(Intent data)
        {
            try
            {
                var placeAddress = data.GetStringExtra("Address") ?? "";
                switch (string.IsNullOrEmpty(placeAddress))
                {
                    //var placeLatLng = data.GetStringExtra("latLng") ?? "";
                    case false:
                        {
                            PlaceText = string.IsNullOrEmpty(PlaceText) switch
                            {
                                false => string.Empty,
                                _ => PlaceText
                            };

                            PlaceText = placeAddress;
                            TxtLocation.Text = PlaceText;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Resul Gallery

        public void OnActivityResult(Java.Lang.Object p0)
        {
            try
            {
                if (p0 is CropImageView.CropResult result)
                {
                    if (result.IsSuccessful)
                    {
                        var resultUri = result.UriContent;
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, resultUri);
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            //Do something with your Uri
                            var productPathImage = filepath;
                            var attach = new Attachments
                            {
                                Id = MAdapter.AttachmentList.Count + 1,
                                TypeAttachment = "postPhotos[]",
                                FileSimple = productPathImage,
                                FileUrl = productPathImage
                            };

                            MAdapter.Add(attach);
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void Get_Data_Product()
        {
            try
            {
                ProductData = JsonConvert.DeserializeObject<ProductDataObject>(Intent?.GetStringExtra("ProductView") ?? "");
                if (ProductData != null)
                {
                    var list = ProductData.Images;
                    foreach (var attach in list.Select(productPathImage => new Attachments
                    {
                        Id = Convert.ToInt32(productPathImage.Id),
                        TypeAttachment = "",
                        FileSimple = productPathImage.Image,
                        FileUrl = productPathImage.Image,
                    }))
                    {
                        MAdapter.Add(attach);
                    }
                    TxtTitle.Text = ProductData.Name;

                    var (currency, currencyIcon) = WoWonderTools.GetCurrency(ProductData.Currency);
                    Console.WriteLine(currency);
                    TxtPrice.Text = ProductData.Price;
                    TxtCurrency.Text = currencyIcon;
                    CurrencyId = ProductData.Currency;

                    TxtLocation.Text = ProductData.Location;
                    TxtAbout.Text = ProductData.Description;

                    CategoryId = ProductData.Category;
                    TxtCategory.Text = CategoriesController.ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == ProductData.Category)?.CategoriesName;

                    switch (ProductData.Type)
                    {
                        // New
                        case "0":
                            RbNew.Checked = true;
                            RbUsed.Checked = false;
                            ProductType = "0";
                            break;
                        // Used
                        default:
                            RbNew.Checked = false;
                            RbUsed.Checked = true;
                            ProductType = "1";
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnFocusChange(View v, bool hasFocus)
        {
            if (v?.Id == TxtLocation.Id && hasFocus)
            {
                TxtLocationOnFocusChange();
            }
        }

    }
}