using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using System;
using Facesofnaija.Activities.Base;
using Facesofnaija.Helpers.Fonts;
using Facesofnaija.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.Announcements
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class AnnouncementDetailsActivity : BaseActivity
    {
        private TextView TitleText;
        private TextView ContentText;
        private TextView TimeText;
        private string AnnouncementId;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Methods.App.FullScreenApp(this);
                SetContentView(Resource.Layout.AnnouncementDetailsLayout);

                AnnouncementId = Intent?.GetStringExtra("AnnouncementId") ?? string.Empty;

                InitComponent();
                InitToolbar();
                LoadData();
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
                TitleText = FindViewById<TextView>(Resource.Id.announcement_detail_title);
                ContentText = FindViewById<TextView>(Resource.Id.announcement_detail_content);
                TimeText = FindViewById<TextView>(Resource.Id.announcement_detail_time);
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_BreakingNews);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    SupportActionBar.SetDisplayShowTitleEnabled(true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadData()
        {
            try
            {
                var textDecode = Intent?.GetStringExtra("AnnouncementTextDecode") ?? string.Empty;
                var text = Intent?.GetStringExtra("AnnouncementText") ?? string.Empty;
                var timeText = Intent?.GetStringExtra("AnnouncementTimeText") ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(textDecode))
                    TitleText.Text = Methods.FunString.DecodeString(textDecode);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (Android.Text.Html.FromHtml(text) is Android.Text.ISpanned spanned)
                        ContentText.TextFormatted = spanned;
                    else
                        ContentText.Text = Methods.FunString.DecodeString(text);
                }

                TimeText.Text = !string.IsNullOrWhiteSpace(timeText)
                    ? Methods.FunString.DecodeString(timeText)
                    : string.Empty;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}
