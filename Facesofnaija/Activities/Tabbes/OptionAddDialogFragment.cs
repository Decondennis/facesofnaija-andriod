using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.BottomSheet;
using System;
using System.Collections.ObjectModel;
using Facesofnaija.Activities.Advertise;
using Facesofnaija.Activities.Communities.Groups;
using Facesofnaija.Activities.Communities.Pages;
using Facesofnaija.Activities.Events;
using Facesofnaija.Activities.Market;
using Facesofnaija.Adapters;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;

namespace Facesofnaija.Activities.Tabbes
{
    public class OptionAddDialogFragment : BottomSheetDialogFragment
    {
        #region Variables Basic

        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ItemOptionAdapter MAdapter;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = WoWonderTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.BottomSheetDefaultLayout, container, false);
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
                SetRecyclerViewAdapters(view);

                LoadDataChat();
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

        private void SetRecyclerViewAdapters(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);

                MAdapter = new ItemOptionAdapter(Activity)
                {
                    ItemOptionList = new ObservableCollection<Classes.ItemOptionObject>()
                };
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.GetRecycledViewPool().Clear();
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void MAdapterOnItemClick(object sender, ItemOptionAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position > -1)
                {
                    var item = MAdapter.GetItem(position);
                    if (item?.Id == "1") //Create Ad
                    {
                        StartActivity(new Intent(Context, typeof(CreateAdvertiseActivity)));
                    }
                    else if (item?.Id == "2") //Create Events
                    {
                        StartActivity(new Intent(Context, typeof(CreateEventActivity)));
                    }
                    else if (item?.Id == "3") //Create Product
                    {
                        StartActivity(new Intent(Context, typeof(CreateProductActivity)));
                    }
                    else if (item?.Id == "4") //Create Page
                    {
                        StartActivity(new Intent(Context, typeof(CreatePageActivity)));
                    }
                    else if (item?.Id == "5") //Create Group
                    {
                        StartActivity(new Intent(Context, typeof(CreateGroupActivity)));
                    }
                    Dismiss();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void LoadDataChat()
        {
            try
            {
                if (AppSettings.ShowAdvertise)
                {
                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                    {
                        Id = "1",
                        Text = GetText(Resource.String.Lbl_Create_Ad),
                        Icon = Resource.Drawable.icon_add_advertise_vector,
                    });
                }

                if (AppSettings.ShowEvents)
                {
                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                    {
                        Id = "2",
                        Text = GetText(Resource.String.Lbl_Create_Events),
                        Icon = Resource.Drawable.icon_add_events_vector,
                    });
                }

                if (AppSettings.ShowMarket)
                {
                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                    {
                        Id = "3",
                        Text = GetText(Resource.String.Lbl_CreateNewProduct),
                        Icon = Resource.Drawable.icon_add_round_vector,
                    });
                }

                if (AppSettings.ShowCommunitiesPages)
                {
                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                    {
                        Id = "4",
                        Text = GetText(Resource.String.Lbl_Create_New_Page),
                        Icon = Resource.Drawable.icon_add_page_vector,
                    });
                }

                if (AppSettings.ShowCommunitiesGroups)
                {
                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                    {
                        Id = "5",
                        Text = GetText(Resource.String.Lbl_Create_New_Group),
                        Icon = Resource.Drawable.icon_add_group_vector,
                    });
                }

                MAdapter.NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}