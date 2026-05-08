using Android.App;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Library.Anjo.Stories.StoriesProgressView;
using WoWonderClient.Classes.Story;

namespace Facesofnaija.Activities.Story.Adapters
{
    public class StoryShowAdapter : RecyclerView.Adapter
    {
        public readonly Activity ActivityContext;
        public readonly StoriesProgressView StoriesProgress;
        public readonly ViewStoryFragment StoryFragment;
        public ObservableCollection<StoryDataObject.Story> StoryList = new ObservableCollection<StoryDataObject.Story>();

        // Kept for compatibility with old cleanup paths.
        public static object ExoController { get; set; }
        public static object PlayerView { get; set; }

        public StoryShowAdapter(Activity activityContext, StoriesProgressView storiesProgress, ViewStoryFragment storyFragment)
        {
            ActivityContext = activityContext;
            StoriesProgress = storiesProgress;
            StoryFragment = storyFragment;
        }

        public override int ItemCount => StoryList?.Count ?? 0;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewStoryLayout, parent, false);
            return new StoryShowAdapterViewHolder(itemView, this);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            try
            {
                if (holder is not StoryShowAdapterViewHolder vh)
                    return;

                var item = StoryList[position];
                if (item == null)
                    return;

                StoryFragment?.SetLastSeenTextView(item);

                var caption = !string.IsNullOrWhiteSpace(item.Description) ? item.Description : item.Title;
                if (string.IsNullOrWhiteSpace(caption))
                {
                    vh.CaptionStoryTextView.Visibility = ViewStates.Gone;
                }
                else
                {
                    vh.CaptionStoryTextView.Visibility = ViewStates.Visible;
                    vh.CaptionStoryTextView.Text = Methods.FunString.DecodeString(caption);
                }

                if (item.UserId == UserDetails.UserId)
                {
                    vh.OpenReply.Visibility = ViewStates.Gone;
                    vh.OpenSeenListLayout.Visibility = ViewStates.Visible;
                    vh.SeenCounterTextView.Visibility = ViewStates.Visible;
                    vh.SeenCounterTextView.Text = item.ViewCount;
                }
                else
                {
                    vh.OpenReply.Visibility = ViewStates.Visible;
                    vh.OpenSeenListLayout.Visibility = ViewStates.Gone;
                    vh.SeenCounterTextView.Visibility = ViewStates.Gone;
                }

                var thumbnail = item.Thumbnail ?? string.Empty;
                var videoUrl = item.Videos?.FirstOrDefault()?.Filename ?? string.Empty;
                var mediaFile = !thumbnail.Contains("avatar") && (item.Videos?.Count ?? 0) == 0 ? thumbnail : videoUrl;

                // Fallback to thumbnail if video URL is absent.
                if (string.IsNullOrWhiteSpace(mediaFile))
                    mediaFile = thumbnail;

                if (!string.IsNullOrWhiteSpace(mediaFile))
                {
                    if (vh.StoryImageView == null)
                    {
                        Log.Warn("FON_STORY_VIEW", "StoryImageView is null in ViewStoryLayout");
                        return;
                    }

                    Log.Warn("FON_STORY_VIEW", $"Bind story id={item.Id} media={mediaFile}");
                    Glide.With(ActivityContext).Load(mediaFile).Apply(new RequestOptions()).Into(vh.StoryImageView);
                    Glide.With(ActivityContext).Load(mediaFile).Apply(new RequestOptions()).Into(vh.ImageBlurView);
                }
                else
                {
                    vh.StoryImageView.SetImageDrawable(null);
                    vh.ImageBlurView.SetImageDrawable(null);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public class StoryShowAdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener, View.IOnLongClickListener
        {
            public View MainView { get; }
            public ImageView StoryImageView { get; }
            public View ReverseView { get; }
            public View CenterView { get; }
            public View SkipView { get; }
            public TextView CaptionStoryTextView { get; }
            public LinearLayout OpenReply { get; }
            public LinearLayout OpenSeenListLayout { get; }
            public TextView SeenCounterTextView { get; }
            public ImageView ImageBlurView { get; }

            private readonly StoryShowAdapter Adapter;
            private bool IsPaused;

            public StoryShowAdapterViewHolder(View itemView, StoryShowAdapter adapter) : base(itemView)
            {
                Adapter = adapter;
                MainView = itemView;

                StoryImageView = itemView.FindViewById<ImageView>(Resource.Id.imagstoryDisplay);
                ReverseView = itemView.FindViewById<View>(Resource.Id.reverse);
                CenterView = itemView.FindViewById<View>(Resource.Id.center);
                SkipView = itemView.FindViewById<View>(Resource.Id.skip);
                CaptionStoryTextView = itemView.FindViewById<TextView>(Resource.Id.story_body);
                OpenReply = itemView.FindViewById<LinearLayout>(Resource.Id.open_reply);
                OpenSeenListLayout = itemView.FindViewById<LinearLayout>(Resource.Id.open_seen_list_layout);
                SeenCounterTextView = itemView.FindViewById<TextView>(Resource.Id.seen_counter);
                ImageBlurView = itemView.FindViewById<ImageView>(Resource.Id.imageBlur);

                ReverseView?.SetOnClickListener(this);
                SkipView?.SetOnClickListener(this);
                CenterView?.SetOnClickListener(this);
                CenterView?.SetOnLongClickListener(this);
            }

            public void OnClick(View v)
            {
                try
                {
                    if (v == ReverseView)
                    {
                        Adapter.StoriesProgress?.Reverse();
                        return;
                    }

                    if (v == SkipView)
                    {
                        Adapter.StoriesProgress?.Skip();
                        return;
                    }

                    if (v == CenterView)
                    {
                        if (IsPaused)
                        {
                            Adapter.StoriesProgress?.Resume();
                            IsPaused = false;
                        }
                        else
                        {
                            Adapter.StoriesProgress?.Pause();
                            IsPaused = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public bool OnLongClick(View v)
            {
                try
                {
                    Adapter.StoriesProgress?.Pause();
                    IsPaused = true;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                return true;
            }
        }
    }
}
