using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Facesofnaija.Helpers.Utils;
using WoWonderClient.Classes.Global;

namespace Facesofnaija.Activities.Announcements.Adapters
{
    public class AnnouncementAdapter : RecyclerView.Adapter
    {
        private readonly Activity ActivityContext;
        public ObservableCollection<AnnouncementDataObject> AnnouncementList = new ObservableCollection<AnnouncementDataObject>();

        public event EventHandler<AnnouncementAdapterClickEventArgs> ItemClick;

        public AnnouncementAdapter(Activity context)
        {
            HasStableIds = true;
            ActivityContext = context;
        }

        public override int ItemCount => AnnouncementList?.Count ?? 0;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AnnouncementView, parent, false);
            return new AnnouncementAdapterViewHolder(itemView, Click);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is AnnouncementAdapterViewHolder holder)
                {
                    var item = AnnouncementList[position];
                    if (item != null)
                    {
                        holder.TitleText.Text = Methods.FunString.DecodeString(item.TextDecode);
                        holder.DescriptionText.Text = Methods.FunString.DecodeString(item.TextDecode);
                        holder.TimeText.Text = item.TimeText ?? string.Empty;

                        if (!string.IsNullOrEmpty(item.TextDecode))
                        {
                            var displayText = item.TextDecode.Length > 120
                                ? item.TextDecode.Substring(0, 120) + "..."
                                : item.TextDecode;
                            holder.DescriptionText.Text = Methods.FunString.DecodeString(displayText);
                        }

                        Glide.With(ActivityContext)
                            .Load(Resource.Drawable.icon_announcement_vector)
                            .Apply(new RequestOptions().CenterCrop())
                            .Into(holder.IconImage);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Click(AnnouncementAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }
    }

    public class AnnouncementAdapterViewHolder : RecyclerView.ViewHolder
    {
        public View MainView { get; }
        public TextView TitleText { get; }
        public TextView DescriptionText { get; }
        public TextView TimeText { get; }
        public ImageView IconImage { get; }

        public AnnouncementAdapterViewHolder(View itemView, Action<AnnouncementAdapterClickEventArgs> clickListener) : base(itemView)
        {
            MainView = itemView;
            TitleText = itemView.FindViewById<TextView>(Resource.Id.announcement_title);
            DescriptionText = itemView.FindViewById<TextView>(Resource.Id.announcement_description);
            TimeText = itemView.FindViewById<TextView>(Resource.Id.announcement_time);
            IconImage = itemView.FindViewById<ImageView>(Resource.Id.announcement_icon);

            itemView.Click += (sender, e) => clickListener(new AnnouncementAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
        }
    }

    public class AnnouncementAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}
