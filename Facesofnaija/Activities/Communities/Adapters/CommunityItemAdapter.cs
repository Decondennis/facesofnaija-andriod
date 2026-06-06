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
using Refractored.Controls;
using Facesofnaija.CustomApi.Classes.Global;
using Facesofnaija.Helpers.CacheLoaders;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;

namespace Facesofnaija.Activities.Communities.Adapters
{
    public class CommunityItemAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<CommunityItemAdapterClickEventArgs> ItemClick;
        public event EventHandler<CommunityItemAdapterClickEventArgs> ActionClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<CommunityDataObject> CommunityList { get; set; } = new ObservableCollection<CommunityDataObject>();

        public CommunityItemAdapter(Activity context)
        {
            HasStableIds = true;
            ActivityContext = context;
        }

        public override int ItemCount => CommunityList?.Count ?? 0;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_CommunityCardView, parent, false);
            return new CommunityItemViewHolder(itemView, this, OnActionClick, OnItemClick);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is not CommunityItemViewHolder viewHolder)
                return;

            var item = CommunityList[position];
            if (item == null)
                return;

            GlideImageLoader.LoadImage(ActivityContext, string.IsNullOrWhiteSpace(item.Cover) ? item.Avatar : item.Cover, viewHolder.CoverImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

            if (item.Avatar?.Contains("http") == true)
                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, viewHolder.AvatarImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
            else if (!string.IsNullOrWhiteSpace(item.Avatar))
                Glide.With(ActivityContext).Load(new File(item.Avatar)).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder_circle).Error(Resource.Drawable.ImagePlacholder_circle)).Into(viewHolder.AvatarImage);

            viewHolder.Name.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.CommunityTitle ?? item.Name ?? item.CommunityName), 28);

            var aboutText = item.About ?? item.Category ?? ActivityContext.GetString(Resource.String.Lbl_Communities);
            viewHolder.Description.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(aboutText), 65);

            var memberCount = item.MembersCount ?? item.Members;
            viewHolder.Members.Text = $"{Methods.FunString.FormatPriceValue(memberCount)} {ActivityContext.GetString(Resource.String.Lbl_Members)}";

            ApplyState(viewHolder, item);
        }

        private void ApplyState(CommunityItemViewHolder holder, CommunityDataObject item)
        {
            var state = WoWonderTools.IsJoinedCommunity(item);
            switch (state)
            {
                case "1":
                    holder.ActionButton.Text = ActivityContext.GetText(Resource.String.Btn_Joined);
                    holder.ActionButton.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                    holder.ActionButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                    holder.StateLabel.Text = ActivityContext.GetText(Resource.String.Btn_Joined);
                    break;
                case "2":
                    holder.ActionButton.Text = ActivityContext.GetText(Resource.String.Lbl_Request);
                    holder.ActionButton.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                    holder.ActionButton.SetTextColor(Color.ParseColor("#444444"));
                    holder.StateLabel.Text = ActivityContext.GetText(Resource.String.Lbl_Request);
                    break;
                default:
                    holder.ActionButton.Text = ActivityContext.GetText(Resource.String.Btn_Join_Community);
                    holder.ActionButton.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                    holder.ActionButton.SetTextColor(Color.White);
                    holder.StateLabel.Text = ActivityContext.GetText(Resource.String.Lbl_Communities);
                    break;
            }

            holder.StateLabel.Visibility = ViewStates.Visible;
        }

        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            if (ActivityContext?.IsDestroyed != false)
                return;

            if (holder is CommunityItemViewHolder viewHolder)
            {
                Glide.With(ActivityContext).Clear(viewHolder.CoverImage);
                Glide.With(ActivityContext).Clear(viewHolder.AvatarImage);
            }

            base.OnViewRecycled(holder);
        }

        public CommunityDataObject GetItem(int position) => CommunityList[position];

        public override long GetItemId(int position)
        {
            var item = CommunityList[position];
            if (item == null || string.IsNullOrWhiteSpace(item.CommunityId))
                return position;

            return long.TryParse(item.CommunityId, out var id) ? id : position;
        }

        public override int GetItemViewType(int position) => 0;

        private void OnActionClick(CommunityItemAdapterClickEventArgs args) => ActionClick?.Invoke(this, args);
        private void OnItemClick(CommunityItemAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);

        public IList GetPreloadItems(int position)
        {
            var list = new List<string>();
            var item = CommunityList[position];
            if (item == null)
                return list;

            if (!string.IsNullOrWhiteSpace(item.Avatar))
                list.Add(item.Avatar);
            if (!string.IsNullOrWhiteSpace(item.Cover))
                list.Add(item.Cover);

            return list;
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object model)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, model.ToString(), ImageStyle.CircleCrop);
        }
    }

    public class CommunityItemViewHolder : RecyclerView.ViewHolder
    {
        public View MainView { get; }
        public ImageView CoverImage { get; }
        public ImageView AvatarImage { get; }
        public TextView Name { get; }
        public TextView Description { get; }
        public TextView Members { get; }
        public TextView StateLabel { get; }
        public AppCompatButton ActionButton { get; }
        private CommunityItemAdapter ParentAdapter { get; }

        public CommunityItemViewHolder(View itemView, CommunityItemAdapter adapter, Action<CommunityItemAdapterClickEventArgs> actionClickListener, Action<CommunityItemAdapterClickEventArgs> itemClickListener) : base(itemView)
        {
            ParentAdapter = adapter;
            MainView = itemView;
            CoverImage = MainView.FindViewById<ImageView>(Resource.Id.coverGroup);
            AvatarImage = MainView.FindViewById<ImageView>(Resource.Id.avatar);
            Name = MainView.FindViewById<TextView>(Resource.Id.name);
            Description = MainView.FindViewById<TextView>(Resource.Id.description);
            Members = MainView.FindViewById<TextView>(Resource.Id.countMembers);
            StateLabel = MainView.FindViewById<TextView>(Resource.Id.stateLabel);
            ActionButton = MainView.FindViewById<AppCompatButton>(Resource.Id.JoinButton);

            ActionButton.Click += (sender, e) => {
                if (BindingAdapterPosition != RecyclerView.NoPosition && BindingAdapterPosition < ParentAdapter.CommunityList.Count)
                {
                    actionClickListener?.Invoke(new CommunityItemAdapterClickEventArgs
                    {
                        View = itemView,
                        Position = BindingAdapterPosition,
                        Button = ActionButton,
                        Item = ParentAdapter.CommunityList[BindingAdapterPosition]
                    });
                }
            };

            itemView.Click += (sender, e) => {
                if (BindingAdapterPosition != RecyclerView.NoPosition && BindingAdapterPosition < ParentAdapter.CommunityList.Count)
                {
                    itemClickListener?.Invoke(new CommunityItemAdapterClickEventArgs
                    {
                        View = itemView,
                        Position = BindingAdapterPosition,
                        Button = ActionButton,
                        Item = ParentAdapter.CommunityList[BindingAdapterPosition]
                    });
                }
            };
        }
    }

    public class CommunityItemAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public AppCompatButton Button { get; set; }
        public CommunityDataObject Item { get; set; }
    }
}