using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using ImageViews.Rounded;
using Java.IO;
using Java.Util;
using Refractored.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Facesofnaija.Helpers.CacheLoaders;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.SQLite;
using WoWonderClient.Classes.Story;
using Console = System.Console;
using IList = System.Collections.IList;

namespace Facesofnaija.Activities.Story.Adapters
{
    public class StoryAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<StoryAdapterClickEventArgs> ItemClick;
        public event EventHandler<StoryAdapterClickEventArgs> ItemLongClick;
        private string YourImageUri;

        private readonly Activity ActivityContext;

        public ObservableCollection<StoryDataObject> StoryList = new ObservableCollection<StoryDataObject>();

        private static bool IsPlaceholderAvatar(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                   || value.Contains("no_profile_image", StringComparison.OrdinalIgnoreCase)
                   || value.Contains("d-avatar", StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveMyAvatar()
        {
            var initAvatar = UserDetails.Avatar;
            if (IsPlaceholderAvatar(initAvatar))
                initAvatar = ListUtils.MyProfileList?.FirstOrDefault()?.Avatar;

            if (IsPlaceholderAvatar(initAvatar))
                initAvatar = ListUtils.MyProfileList?.FirstOrDefault()?.AvatarFull;

            if (IsPlaceholderAvatar(initAvatar))
            {
                var cachedProfile = new SqLiteDatabase().Get_MyProfile();
                initAvatar = cachedProfile?.Avatar;
            }

            if (IsPlaceholderAvatar(initAvatar))
            {
                var cachedProfile = new SqLiteDatabase().Get_MyProfile();
                initAvatar = cachedProfile?.AvatarFull;
            }

            if (IsPlaceholderAvatar(initAvatar))
                initAvatar = string.Empty;

            if (IsPlaceholderAvatar(initAvatar))
                initAvatar = WoWonderTools.GetDefaultAvatar();

            if (!string.IsNullOrWhiteSpace(initAvatar) && !initAvatar.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var baseUrl = WoWonderClient.InitializeWoWonder.WebsiteUrl?.Trim().TrimEnd('/');
                if (!string.IsNullOrWhiteSpace(baseUrl))
                    initAvatar = $"{baseUrl}/{initAvatar.TrimStart('/')}";
            }

            return initAvatar ?? string.Empty;
        }

        private static string ResolveStoryAvatar(string avatar, string thumbnail = null)
        {
            var candidate = !IsPlaceholderAvatar(avatar) ? avatar : null;
            if (IsPlaceholderAvatar(candidate))
                candidate = !IsPlaceholderAvatar(thumbnail) ? thumbnail : null;

            if (IsPlaceholderAvatar(candidate))
            {
                var profileAvatar = ListUtils.MyProfileList?.FirstOrDefault()?.Avatar;
                if (IsPlaceholderAvatar(profileAvatar))
                    profileAvatar = ListUtils.MyProfileList?.FirstOrDefault()?.AvatarFull;

                candidate = profileAvatar;
            }

            if (IsPlaceholderAvatar(candidate))
                candidate = UserDetails.Avatar;
            if (IsPlaceholderAvatar(candidate))
                candidate = "no_profile_image_circle";

            return GlideImageLoader.NormalizeImageUrl(candidate);
        }

        private static string ResolveStoryCover(string thumbnail, string avatar = null)
        {
            var candidate = !IsPlaceholderAvatar(thumbnail) ? thumbnail : null;
            if (IsPlaceholderAvatar(candidate))
                candidate = !IsPlaceholderAvatar(avatar) ? avatar : null;

            if (IsPlaceholderAvatar(candidate))
                candidate = ResolveStoryAvatar(avatar, thumbnail);

            return GlideImageLoader.NormalizeImageUrl(candidate);
        }

        public StoryAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;

                var dataOwner = StoryList.FirstOrDefault(a => a.Type == "Your");
                switch (dataOwner)
                {
                    case null:
                        var initAvatar = ResolveMyAvatar();
                        StoryList.Add(new StoryDataObject
                        {
                            UserId = UserDetails.UserId,
                            Avatar = initAvatar,
                            Type = "Your",
                            Username = context.GetText(Resource.String.Lbl_YourStory),
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => StoryList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Story_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HStoryView, parent, false);
                var vh = new StoryAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Console.WriteLine("EX:ALLEN >> " + exception);
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
                    case StoryAdapterViewHolder holder:
                        {
                            // Seed both image targets to a visible local avatar to avoid recycled blank cells.
                            holder.RoundImage.Visibility = ViewStates.Visible;
                            holder.Image.Visibility = ViewStates.Visible;
                            holder.RoundImage.SetImageResource(Resource.Drawable.no_profile_image);
                            holder.Image.SetImageResource(Resource.Drawable.no_profile_image);

                            var item = StoryList[position];
                            if (item != null)
                            {
                                if (string.Equals(item.Type, "Your", StringComparison.OrdinalIgnoreCase))
                                {
                                    item.UserId = UserDetails.UserId;
                                    var latestSelfGroup = StoryApiService.GetLatestSelfStoryGroup();
                                    if (IsPlaceholderAvatar(item.Avatar) && !IsPlaceholderAvatar(latestSelfGroup?.Avatar))
                                        item.Avatar = latestSelfGroup.Avatar;

                                    var myAvatar = ResolveMyAvatar();
                                    if (!string.IsNullOrWhiteSpace(myAvatar))
                                    {
                                        if (string.IsNullOrWhiteSpace(item.Avatar) || IsPlaceholderAvatar(item.Avatar))
                                            item.Avatar = myAvatar;

                                        if (item.Stories == null || item.Stories.Count == 0)
                                        {
                                            item.Stories = new List<StoryDataObject.Story>
                                            {
                                                new StoryDataObject.Story
                                                {
                                                    Thumbnail = myAvatar,
                                                }
                                            };
                                        }
                                        else if (string.IsNullOrWhiteSpace(item.Stories[0]?.Thumbnail) || IsPlaceholderAvatar(item.Stories[0]?.Thumbnail))
                                        {
                                            item.Stories[0].Thumbnail = myAvatar;
                                        }

                                        if (item.Stories?.Count > 0 && IsPlaceholderAvatar(item.Stories[0]?.Thumbnail) && !IsPlaceholderAvatar(latestSelfGroup?.Stories?.FirstOrDefault()?.Thumbnail))
                                            item.Stories[0].Thumbnail = latestSelfGroup.Stories.FirstOrDefault()?.Thumbnail;
                                    }
                                }

                                switch (item.Stories?.Count)
                                {
                                    case > 0:
                                        var storyThumbnail = ResolveStoryCover(item.Stories[0]?.Thumbnail, item.Avatar);
                                        if (IsPlaceholderAvatar(storyThumbnail))
                                            holder.RoundImage.SetImageResource(Resource.Drawable.no_profile_image);
                                        else
                                            GlideImageLoader.LoadImage(ActivityContext, storyThumbnail, holder.RoundImage, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                        Android.Util.Log.Warn("FON_STORY_BIND", $"pos={position} type={item.Type} userId={item.UserId ?? "NULL"} thumb={storyThumbnail ?? "NULL"} avatar={item.Avatar ?? "NULL"}");
                                        break;
                                }

                                switch (item.Type)
                                {
                                    case "Your":
                                        {
                                            holder.Image.Visibility = ViewStates.Visible;
                                            holder.Image.BringToFront();

                                            // Always use avatar for "Your Story" entry (like web create button)
                                            YourImageUri = ResolveStoryCover(null, item.Avatar);
                                            var resolvedMyAvatar = ResolveMyAvatar();
                                            if (!string.IsNullOrWhiteSpace(resolvedMyAvatar)
                                                && (string.IsNullOrWhiteSpace(item.Avatar)
                                                    || IsPlaceholderAvatar(item.Avatar)))
                                            {
                                                if (string.IsNullOrWhiteSpace(item.Avatar) || IsPlaceholderAvatar(item.Avatar))
                                                    item.Avatar = resolvedMyAvatar;

                                                YourImageUri = ResolveStoryCover(null, item.Avatar);
                                            }

                                            if (IsPlaceholderAvatar(YourImageUri))
                                            {
                                                holder.RoundImage.SetImageResource(Resource.Drawable.no_profile_image);
                                                holder.Image.SetImageResource(Resource.Drawable.no_profile_image);
                                            }
                                            else
                                            {
                                                GlideImageLoader.LoadImage(ActivityContext, YourImageUri, holder.RoundImage, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                                var yourAvatar = ResolveStoryAvatar(item.Avatar, null);
                                                if (IsPlaceholderAvatar(yourAvatar))
                                                    holder.Image.SetImageResource(Resource.Drawable.no_profile_image_circle);
                                                else
                                                    GlideImageLoader.LoadImage(ActivityContext, yourAvatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                            }

                                            break;
                                        }
                                    case "Live":
                                        {
                                            var liveCover = ResolveStoryCover(item.Stories?.FirstOrDefault()?.Thumbnail, item.Avatar);
                                            var liveAvatar = ResolveStoryAvatar(item.Avatar, item.Stories?.FirstOrDefault()?.Thumbnail);
                                            if (IsPlaceholderAvatar(liveCover))
                                            {
                                                holder.RoundImage.SetImageResource(Resource.Drawable.no_profile_image);
                                                holder.Image.SetImageResource(Resource.Drawable.no_profile_image);
                                            }
                                            else
                                            {
                                                GlideImageLoader.LoadImage(ActivityContext, liveCover, holder.RoundImage, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                                GlideImageLoader.LoadImage(ActivityContext, liveAvatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                                            }

                                            break;
                                        }
                                    default:
                                        item.ProfileIndicator ??= AppSettings.MainColor;
                                        var defaultAvatar = ResolveStoryAvatar(item.Avatar, item.Stories?.FirstOrDefault()?.Thumbnail);
                                        if (IsPlaceholderAvatar(defaultAvatar))
                                            holder.Image.SetImageResource(Resource.Drawable.no_profile_image);
                                        else
                                            GlideImageLoader.LoadImage(ActivityContext, defaultAvatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                                        break;
                                }

                                holder.Name.Text = string.Equals(item.Type, "Your", StringComparison.OrdinalIgnoreCase)
                                    ? ActivityContext.GetText(Resource.String.Lbl_YourStory)
                                    : Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(item), 12);

                                if (item.DataLivePost != null && item.Type == "Live")
                                    holder.VideoStory.Visibility = ViewStates.Visible;
                                else
                                    holder.VideoStory.Visibility = ViewStates.Gone;
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
        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                switch (holder)
                {
                    case StoryAdapterViewHolder viewHolder:
                        //Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.Image);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public StoryDataObject GetItem(int position)
        {
            return StoryList[position];
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

        private void Click(StoryAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(StoryAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = StoryList[p0];
                switch (item)
                {
                    case null:
                        return d;
                    default:
                        {
                            switch (string.IsNullOrEmpty(item.Stories[0].Thumbnail))
                            {
                                case false:
                                    d.Add(item.Stories[0].Thumbnail);
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

    public class StoryAdapterViewHolder : RecyclerView.ViewHolder
    {
        public StoryAdapterViewHolder(View itemView, Action<StoryAdapterClickEventArgs> clickListener, Action<StoryAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                RoundImage = MainView.FindViewById<RoundedImageView>(Resource.Id.iv_round_story);
                Image = MainView.FindViewById<CircleImageView>(Resource.Id.civ_story_avatar);
                Name = MainView.FindViewById<TextView>(Resource.Id.Txt_Username);
                VideoStory = MainView.FindViewById<LinearLayout>(Resource.Id.ll_video_story);

                //Event
                itemView.Click += (sender, e) => clickListener(new StoryAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

                Console.WriteLine(longClickListener);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; private set; }

        public RoundedImageView RoundImage { get; set; }
        public CircleImageView Image { get; set; }
        public TextView Name { get; private set; }
        public LinearLayout VideoStory { get; private set; }

        #endregion
    }

    public class StoryAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}
