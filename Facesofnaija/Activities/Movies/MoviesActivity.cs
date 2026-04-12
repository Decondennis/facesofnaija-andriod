using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Dialog; 
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Facesofnaija.Activities.Base;
using Facesofnaija.Activities.Movies.Adapters;
using Facesofnaija.Activities.Videos;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Movies;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.Movies
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MoviesActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private MoviesAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private AdView MAdView;
        private FloatingActionButton BtnFilter;
        private string CategoryId;


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
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                LoadMovies();
                AdsGoogle.Ad_RewardedVideo(this);
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
                MAdView?.Resume();

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
                MAdView?.Pause();

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
                ListUtils.ListCachedDataMovie = MAdapter.MoviesList.Count switch
                {
                    > 0 => MAdapter.MoviesList,
                    _ => ListUtils.ListCachedDataMovie
                };

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
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));


                BtnFilter = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                BtnFilter.Visibility = ViewStates.Visible;

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
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
                    toolBar.Title = GetText(Resource.String.Lbl_Movies);
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
                MAdapter = new MoviesAdapter(this)
                {
                    MoviesList = new ObservableCollection<MoviesDataObject>()
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<MoviesDataObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
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
                        MAdapter.ItemClick += MAdapterOnItemClick;
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        BtnFilter.Click += BtnFilterOnClick;
                        break;
                    default:
                        MAdapter.ItemClick -= MAdapterOnItemClick;
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        BtnFilter.Click -= BtnFilterOnClick;
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
                MAdView?.Destroy();


                MAdapter = null!;
                SwipeRefreshLayout = null!;
                MRecycler = null!;
                EmptyStateLayout = null!;
                Inflated = null!;
                MainScrollEvent = null!;
                BtnFilter = null!;
                CategoryId = null!;
                MAdView = null!;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.MoviesList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.MoviesList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, MoviesAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(this, typeof(VideoViewerActivity));
                    intent.PutExtra("Viewer_Video", JsonConvert.SerializeObject(item));
                    intent.PutExtra("VideoId", item.Id);
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnFilterOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (CategoriesController.ListCategoriesMovies.Count)
                {
                    case > 0:
                        {
                            var dialogList = new MaterialAlertDialogBuilder(this);

                            var arrayAdapter = CategoriesController.ListCategoriesMovies.Select(item => item.CategoriesName).ToList();

                            arrayAdapter.Insert(0, GetString(Resource.String.Lbl_Default));

                            dialogList.SetTitle(GetText(Resource.String.Lbl_SelectCategories));
                            dialogList.SetMessage(GetText(Resource.String.Lbl_GetMoviesByCategories));
                            dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                            dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                            
                            dialogList.Show();
                            break;
                        }
                    default:
                        Methods.DisplayReportResult(this, "Not have List Categories Movies");
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Movies 

        private void LoadMovies()
        {
            try
            {
                switch (ListUtils.ListCachedDataMovie.Count)
                {
                    case > 0:
                        {
                            MAdapter.MoviesList = ListUtils.ListCachedDataMovie;
                            MAdapter.NotifyDataSetChanged();

                            var item = MAdapter.MoviesList.LastOrDefault();
                            if (item != null && !string.IsNullOrEmpty(item.Id))
                                StartApiService(item.Id);
                            break;
                        }
                    default:
                        Task.Factory.StartNew(() => StartApiService());
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadMoviesAsync(offset) });
        }

        private async Task LoadMoviesAsync(string offset = "0")
        {
            switch (MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                var countList = MAdapter.MoviesList.Count;
                var (apiStatus, respond) = await RequestsAsync.Movies.GetMoviesAsync("10", offset, "", CategoryId);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case GetMoviesObject result:
                                    {
                                        var respondList = result.Movies.Count;
                                        switch (respondList)
                                        {
                                            case > 0 when countList > 0:
                                                {
                                                    foreach (var item in from item in result.Movies let check = MAdapter.MoviesList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                                    {
                                                        MAdapter.MoviesList.Add(item);
                                                    }

                                                    RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.MoviesList.Count - countList); });
                                                    break;
                                                }
                                            case > 0:
                                                MAdapter.MoviesList = new ObservableCollection<MoviesDataObject>(result.Movies);
                                                RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                                break;
                                            default:
                                                {
                                                    switch (MAdapter.MoviesList.Count)
                                                    {
                                                        case > 10 when !MRecycler.CanScrollVertically(1):
                                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreMovies), ToastLength.Short);
                                                            break;
                                                    }

                                                    break;
                                                }
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        Methods.DisplayReportResult(this, respond);
                        break;
                }

                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                switch (x.EmptyStateButton.HasOnClickListeners)
                {
                    case false:
                        x.EmptyStateButton.Click += null!;
                        x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                        break;
                }

                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                MainScrollEvent.IsLoading = false;
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                switch (MAdapter.MoviesList.Count)
                {
                    case > 0:
                        MRecycler.Visibility = ViewStates.Visible;
                        EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    default:
                        {
                            MRecycler.Visibility = ViewStates.Gone;

                            Inflated ??= EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoMovies);
                            switch (x.EmptyStateButton.HasOnClickListeners)
                            {
                                case false:
                                    x.EmptyStateButton.Click += null!;
                                    break;
                            }
                            EmptyStateLayout.Visibility = ViewStates.Visible;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

       public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                CategoryId = itemString == GetString(Resource.String.Lbl_Default) ? "" : CategoriesController.ListCategoriesMovies.FirstOrDefault(categories => categories.CategoriesName == itemString)?.CategoriesId;

                MAdapter.MoviesList.Clear();
                MAdapter.NotifyDataSetChanged();

                SwipeRefreshLayout.Refreshing = true;

                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

    }
}