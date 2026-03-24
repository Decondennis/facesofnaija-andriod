using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using Refractored.Controls;
using Facesofnaija.Helpers.CacheLoaders;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;
using Facesofnaija.CustomApi.Classes.Global;

namespace Facesofnaija.Activities.Search.Adapters
{
    public class SearchCommunityAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<SearchCommunityAdapterClickEventArgs> JoinButtonItemClick;
        public event EventHandler<SearchCommunityAdapterClickEventArgs> ItemClick;
        public event EventHandler<SearchCommunityAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;

        public ObservableCollection<CommunityDataObject> CommunityList = new ObservableCollection<CommunityDataObject>();
        public SearchCommunityAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => CommunityList?.Count ?? 0;
 
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HPage_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HContactView, parent, false);
                var vh = new SearchCommunityAdapterViewHolder(itemView, JoinButtonClick, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                switch (viewHolder)
                {
                    case SearchCommunityAdapterViewHolder holder:
                    {
                        var item = CommunityList[position];
                        if (item != null)
                        {
                            Initialize(holder, item);
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

        private void Initialize(SearchCommunityAdapterViewHolder holder, CommunityDataObject item)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                   
                if (!string.IsNullOrEmpty(item.CommunityTitle) || !string.IsNullOrWhiteSpace(item.CommunityTitle))
                {
                    holder.Name.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.CommunityTitle), 20);
                }
                else
                {
                    holder.Name.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.CommunityName), 20);
                } 

                CategoriesController cat = new CategoriesController();
                holder.About.Text = item.Category;

                //var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(30).EndConfig().BuildRound("", Color.ParseColor("#1A237E"));
                holder.CivBg.SetColorFilter(Color.ParseColor("#FCA65C"));
                holder.CivBg.Visibility = ViewStates.Visible;

                holder.SmallIcon.Visibility = ViewStates.Visible;
                holder.SmallIcon.SetImageResource(Resource.Drawable.ic_small_group);

                if (/*item.IsOwner != null && item.IsOwner.Value ||*/ item.UserId == UserDetails.UserId)
                {
                    holder.Button.Visibility = ViewStates.Gone;
                }
                else
                {
                    //Set style Btn Joined Group 
                    if (WoWonderTools.IsJoinedCommunity(item) == "1") //joined
                    {
                        holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                        holder.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        holder.Button.Text = ActivityContext.GetText(Resource.String.Btn_Joined);
                        holder.Button.Tag = "1";
                    }
                    else if (WoWonderTools.IsJoinedCommunity(item) == "2") //requested
                    {
                        holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                        holder.Button.SetTextColor(Color.ParseColor("#444444"));
                        holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Request);
                        holder.Button.Tag = "2";
                    }
                    else //not joined
                    {
                        holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                        holder.Button.SetTextColor(Color.White);
                        holder.Button.Text = ActivityContext.GetText(Resource.String.Btn_Join_Community);
                        holder.Button.Tag = "0";

                        holder.Button.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                        return;

                switch (holder)
                {
                    case SearchGroupAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public CommunityDataObject GetItem(int position)
        {
            return CommunityList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void JoinButtonClick(SearchCommunityAdapterClickEventArgs args)
        {
            JoinButtonItemClick?.Invoke(this, args);
        }

        private void Click(SearchCommunityAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(SearchCommunityAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = CommunityList[p0];
                switch (item)
                {
                    case null:
                        return d;
                    default:
                    {
                        switch (string.IsNullOrEmpty(item.Avatar))
                        {
                            case false:
                                d.Add(item.Avatar);
                                break;
                        }

                        return d;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }


    }

    public class SearchCommunityAdapterViewHolder : RecyclerView.ViewHolder
    {
        public SearchCommunityAdapterViewHolder(View itemView, Action<SearchCommunityAdapterClickEventArgs> joinButtonClickListener, Action<SearchCommunityAdapterClickEventArgs> clickListener,Action<SearchCommunityAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                Button = MainView.FindViewById<AppCompatButton>(Resource.Id.cont);
                CivBg = MainView.FindViewById<CircleImageView>(Resource.Id.ImageLastseen);
                SmallIcon = MainView.FindViewById<ImageView>(Resource.Id.smallIcon);

                //Event 

                Button.Click += (sender, e) => joinButtonClickListener(new SearchCommunityAdapterClickEventArgs{View = itemView, Position = BindingAdapterPosition , Button = Button });
                itemView.Click += (sender, e) => clickListener(new SearchCommunityAdapterClickEventArgs{View = itemView, Position = BindingAdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new SearchCommunityAdapterClickEventArgs{View = itemView, Position = BindingAdapterPosition});

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }
         
        public ImageView Image { get; private set; }

        public TextView Name { get; private set; }
        public TextView About { get; private set; }
        public AppCompatButton Button { get; private set; }
        public CircleImageView CivBg { get; private set; }
        public ImageView SmallIcon { get; private set; }

        #endregion
    }

    public class SearchCommunityAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public AppCompatButton Button { get; set; }
    }
}