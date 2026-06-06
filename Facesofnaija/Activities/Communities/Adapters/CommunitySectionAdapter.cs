using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Helpers.Model;
using Facesofnaija.CustomApi.Classes.Global;
using Facesofnaija.Library.Anjo.IntegrationRecyclerView;

namespace Facesofnaija.Activities.Communities.Adapters
{
    public class CommunitySectionAdapter : RecyclerView.Adapter
    {
        public event EventHandler<CommunityItemAdapterClickEventArgs> ItemClick;
        public event EventHandler<CommunityItemAdapterClickEventArgs> ActionClick;
        public event EventHandler<CommunitySectionAdapterClickEventArgs> MoreClick;

        private readonly Activity ActivityContext;
        private readonly RecyclerView.RecycledViewPool RecycledViewPool = new RecyclerView.RecycledViewPool();
        public ObservableCollection<CommunityDashboardSectionModel> Sections { get; set; } = new ObservableCollection<CommunityDashboardSectionModel>();

        public CommunitySectionAdapter(Activity activity)
        {
            HasStableIds = true;
            ActivityContext = activity;
        }

        public override int ItemCount => Sections?.Count ?? 0;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
            return new CommunitySectionViewHolder(itemView, args => MoreClick?.Invoke(this, args), this);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is not CommunitySectionViewHolder viewHolder)
                return;

            var section = Sections[position];
            if (section == null)
                return;

            viewHolder.TitleText.Text = section.Title;
            viewHolder.MoreText.Text = ActivityContext.GetText(Resource.String.Lbl_SeeAll);
            viewHolder.MoreText.SetTextColor(Color.ParseColor(AppSettings.MainColor));
            viewHolder.MoreText.Visibility = ViewStates.Visible;

            if (viewHolder.SectionAdapter == null)
            {
                viewHolder.SectionAdapter = new CommunityItemAdapter(ActivityContext)
                {
                    CommunityList = section.Communities
                };
                viewHolder.SectionAdapter.ItemClick += OnItemClick;
                viewHolder.SectionAdapter.ActionClick += OnActionClick;

                var layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                viewHolder.RecyclerView.SetLayoutManager(layoutManager);
                viewHolder.RecyclerView.HasFixedSize = true;
                viewHolder.RecyclerView.SetItemViewCacheSize(12);
                viewHolder.RecyclerView.NestedScrollingEnabled = false;
                viewHolder.RecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;
                viewHolder.RecyclerView.SetRecycledViewPool(RecycledViewPool);
                viewHolder.RecyclerView.SetAdapter(viewHolder.SectionAdapter);

                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CommunityDataObject>(ActivityContext, viewHolder.SectionAdapter, sizeProvider, 10);
                viewHolder.RecyclerView.AddOnScrollListener(preLoader);
            }
            else
            {
                viewHolder.SectionAdapter.CommunityList = section.Communities;
                viewHolder.SectionAdapter.NotifyDataSetChanged();
            }
        }

        public override long GetItemId(int position) => position;
        public override int GetItemViewType(int position) => position;
        public CommunityDashboardSectionModel GetItem(int position) => Sections[position];

        private void OnItemClick(object sender, CommunityItemAdapterClickEventArgs e) => ItemClick?.Invoke(this, e);
        private void OnActionClick(object sender, CommunityItemAdapterClickEventArgs e) => ActionClick?.Invoke(this, e);
        internal void RaiseMoreClick(CommunitySectionAdapterClickEventArgs args) => MoreClick?.Invoke(this, args);
    }

    public class CommunitySectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
    {
        private readonly CommunitySectionAdapter Adapter;
        private readonly Action<CommunitySectionAdapterClickEventArgs> ClickListener;
        public View MainView { get; }
        public TextView TitleText { get; }
        public TextView MoreText { get; }
        public RecyclerView RecyclerView { get; }
        public CommunityItemAdapter SectionAdapter { get; set; }

        public CommunitySectionViewHolder(View itemView, Action<CommunitySectionAdapterClickEventArgs> clickListener, CommunitySectionAdapter adapter) : base(itemView)
        {
            MainView = itemView;
            Adapter = adapter;
            ClickListener = clickListener;
            TitleText = MainView.FindViewById<TextView>(Resource.Id.headText);
            MoreText = MainView.FindViewById<TextView>(Resource.Id.moreText);
            RecyclerView = MainView.FindViewById<RecyclerView>(Resource.Id.Recyler);
            MoreText.SetOnClickListener(this);
            TitleText.Click += (sender, e) => RaiseClick(TitleText);
            itemView.Click += (sender, e) => RaiseClick(itemView);
        }

        public void OnClick(View v)
        {
            RaiseClick(v);
        }

        private void RaiseClick(View source)
        {
            if (BindingAdapterPosition == AndroidX.RecyclerView.Widget.RecyclerView.NoPosition)
                return;

            Adapter.RaiseMoreClick(new CommunitySectionAdapterClickEventArgs { Position = BindingAdapterPosition, View = source });
        }
    }

    public class CommunitySectionAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}