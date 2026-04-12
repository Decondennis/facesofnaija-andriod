using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using Google.Android.Material.BottomSheet;
using Java.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Facesofnaija.Activities.NativePost.Pages;
using Facesofnaija.Activities.Story;
using Facesofnaija.Activities.Tabbes;
using Facesofnaija.Helpers.Fonts;
using Facesofnaija.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Story;

namespace Facesofnaija.Activities.MyProfile
{
    public class OptionAvatarProfileDialogFragment : BottomSheetDialogFragment
    {
        #region Variables Basic

        private RelativeLayout ViewStoryLayout, SelectAvatarLayout, ViewAvatarLayout;
        private TextView ViewStoryIcon, SelectAvatarIcon, ViewAvatarIcon;
        private TextView ViewStoryText, SelectAvatarText, ViewAvatarText;

        private string Page;
        private UserDataObject UserData;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = WoWonderTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.BottomSheetOptionAvatarProfileLayout, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                Page = Arguments?.GetString("Page") ?? "";
                UserData = JsonConvert.DeserializeObject<UserDataObject>(Arguments?.GetString("UserData") ?? "");

                InitComponent(view);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                ViewStoryLayout = view.FindViewById<RelativeLayout>(Resource.Id.ViewStoryLayout);
                ViewStoryIcon = view.FindViewById<TextView>(Resource.Id.ViewStoryIcon);
                ViewStoryText = view.FindViewById<TextView>(Resource.Id.ViewStoryText);
                ViewStoryLayout.Click += ViewStoryLayoutOnClick;

                SelectAvatarLayout = view.FindViewById<RelativeLayout>(Resource.Id.SelectAvatarLayout);
                SelectAvatarIcon = view.FindViewById<TextView>(Resource.Id.SelectAvatarIcon);
                SelectAvatarText = view.FindViewById<TextView>(Resource.Id.SelectAvatarText);
                SelectAvatarLayout.Click += SelectAvatarLayoutOnClick;

                ViewAvatarLayout = view.FindViewById<RelativeLayout>(Resource.Id.ViewAvatarLayout);
                ViewAvatarIcon = view.FindViewById<TextView>(Resource.Id.ViewAvatarIcon);
                ViewAvatarText = view.FindViewById<TextView>(Resource.Id.ViewAvatarText);
                ViewAvatarLayout.Click += ViewAvatarLayoutOnClick;

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ViewStoryIcon, IonIconsFonts.RadioButtonOn);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, SelectAvatarIcon, IonIconsFonts.IosImages);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, ViewAvatarIcon, FontAwesomeIcon.Image);

                if (Page == "UserProfile")
                    SelectAvatarLayout.Visibility = ViewStates.Gone;

                if (UserData.Avatar.Contains("d-avatar"))
                    ViewAvatarLayout.Visibility = ViewStates.Gone;

                if (!WoWonderTools.StoryIsAvailable(UserData.UserId))
                    ViewStoryLayout.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void ViewAvatarLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (UserData.Avatar.Contains("d-avatar"))
                    return;

                if (!string.IsNullOrEmpty(UserData.AvatarPostId) && UserData.AvatarPostId != "0")
                {
                    var intent = new Intent(Activity, typeof(ViewFullPostActivity));
                    intent.PutExtra("Id", UserData.AvatarPostId);
                    //intent.PutExtra("DataItem", JsonConvert.SerializeObject(e.NewsFeedClass));
                    Activity.StartActivity(intent);
                }
                else
                {
                    var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, UserData.Avatar.Split('/').Last(), UserData.Avatar);
                    if (media.Contains("http"))
                    {
                        var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(media));
                        Activity.StartActivity(intent);
                    }
                    else
                    {
                        var file2 = new File(media);
                        var photoUri = FileProvider.GetUriForFile(Activity, Activity.PackageName + ".fileprovider", file2);

                        var intent = new Intent(Intent.ActionPick);
                        intent.SetAction(Intent.ActionView);
                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                        intent.SetDataAndType(photoUri, "image/*");
                        Activity.StartActivity(intent);
                    }
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SelectAvatarLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                MyProfileActivity.GetInstance()?.GalleryController?.OpenDialogGallery("Avatar");
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ViewStoryLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var tab = TabbedMainActivity.GetInstance()?.NewsFeedTab;
                StoryDataObject dataMyStory = tab?.PostFeedAdapter?.HolderStory?.StoryAdapter?.StoryList?.FirstOrDefault(o => o.UserId == UserData.UserId);
                if (dataMyStory != null)
                {
                    List<StoryDataObject> storyList = new List<StoryDataObject>(tab.PostFeedAdapter?.HolderStory.StoryAdapter.StoryList);
                    storyList.RemoveAll(o => o.Type == "Your" || o.Type == "Live");

                    Intent intent = new Intent(Activity, typeof(StoryDetailsActivity));
                    intent.PutExtra("UserId", UserData.UserId);
                    intent.PutExtra("IndexItem", 0);
                    intent.PutExtra("StoriesCount", storyList.Count);
                    intent.PutExtra("DataItem", JsonConvert.SerializeObject(new ObservableCollection<StoryDataObject>(storyList)));
                    Activity.StartActivity(intent);
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}