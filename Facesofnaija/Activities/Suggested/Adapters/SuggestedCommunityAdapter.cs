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
using Bumptech.Glide.Request;
using Java.IO;
using Java.Util;
using Facesofnaija.Helpers.CacheLoaders;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;
using Facesofnaija.CustomApi.Classes.Global;

namespace Facesofnaija.Activities.Suggested.Adapters
{
    public class SuggestedCommunityAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<SuggestedCommunityAdapterClickEventArgs> JoinButtonItemClick;
        public event EventHandler<SuggestedCommunityAdapterClickEventArgs> ItemClick;
        public event EventHandler<SuggestedCommunityAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<CommunityDataObject> CommunityList = new ObservableCollection<CommunityDataObject>();

        public SuggestedCommunityAdapter(Activity context)
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
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_SuggestedGroupView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_SuggestedGroupView, parent, false);
                var vh = new SuggestedCommunityAdapterViewHolder(itemView, JoinButtonClick, Click, LongClick);
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
                    case SuggestedCommunityAdapterViewHolder holder:
                    {
                        var item = CommunityList[position];
                        if (item != null)
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.Cover, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                            holder.Name.Text = Methods.FunString.DecodeString(item.Name);
                            holder.CountMembers.Text = Methods.FunString.FormatPriceValue(item.Members) +  " " +ActivityContext.GetString(Resource.String.Lbl_Members);

                            if (item.Avatar.Contains("http"))
                                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Avatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                            else
                                Glide.With(ActivityContext).Load(new File(item.Avatar)).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder_circle).Error(Resource.Drawable.ImagePlacholder_circle)).Into(holder.Avatar);

                            if (item.UserId == UserDetails.UserId)
                            {
                                holder.JoinButton.Visibility = ViewStates.Gone;
                            }
                            else
                            {
                                //Set style Btn Joined Group 
                                if (WoWonderTools.IsJoinedCommunity(item) == "1") //joined
                                {
                                    holder.JoinButton.SetBackgroundResource(Resource.Drawable.buttonFlatGray);
                                    holder.JoinButton.SetTextColor(Color.White);
                                    holder.JoinButton.Text = ActivityContext.GetText(Resource.String.Btn_Joined);
                                    holder.JoinButton.Tag = "1";
                                }
                                else if (WoWonderTools.IsJoinedCommunity(item) == "2") //requested
                                {
                                    holder.JoinButton.SetBackgroundResource(Resource.Drawable.buttonFlatGray);
                                    holder.JoinButton.SetTextColor(Color.White);
                                    holder.JoinButton.Text = ActivityContext.GetText(Resource.String.Lbl_Request);
                                    holder.JoinButton.Tag = "2";
                                }
                                else //not joined
                                {
                                    holder.JoinButton.SetBackgroundResource(Resource.Drawable.buttonFlat);
                                    holder.JoinButton.SetTextColor(Color.White);
                                    holder.JoinButton.Text = ActivityContext.GetString(Resource.String.Btn_Join_Community);
                                    holder.JoinButton.Tag = "0";
                                } 
                            } 
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

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                switch (holder)
                {
                    case SuggestedGroupAdapterViewHolder viewHolder:
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
        public override int ItemCount => CommunityList?.Count ?? 0;

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

        void JoinButtonClick(SuggestedCommunityAdapterClickEventArgs args) => JoinButtonItemClick?.Invoke(this, args);
        void Click(SuggestedCommunityAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(SuggestedCommunityAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);


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

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }

    }

    public class SuggestedCommunityAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic
         
        public View MainView { get; private set; }
         
        public ImageView Image { get; private set; }

        public TextView Name { get; private set; }
        public TextView CountMembers { get; private set; }
        public AppCompatButton JoinButton { get; private set; }
        public ImageView Avatar { get; private set; }

        #endregion

        public SuggestedCommunityAdapterViewHolder(View itemView, Action<SuggestedCommunityAdapterClickEventArgs> joinButtonClickListener, Action<SuggestedCommunityAdapterClickEventArgs> clickListener, Action<SuggestedCommunityAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.coverGroup);
                Name = MainView.FindViewById<TextView>(Resource.Id.name);
                CountMembers = MainView.FindViewById<TextView>(Resource.Id.countMembers);
                JoinButton = MainView.FindViewById<AppCompatButton>(Resource.Id.JoinButton);
                Avatar = MainView.FindViewById<ImageView>(Resource.Id.avatar);

                //Event
                JoinButton.Click += (sender, e) => joinButtonClickListener(new SuggestedCommunityAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition , JoinButton = JoinButton });
                itemView.Click += (sender, e) => clickListener(new SuggestedCommunityAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                System.Console.WriteLine(longClickListener);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class SuggestedCommunityAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public AppCompatButton JoinButton { get; set; }
    } 
}