using Android.App;
using Android.OS;
using Android.Views;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Facesofnaija.Activities.NativePost.Extra;
using Facesofnaija.Activities.Tabbes;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.MediaPlayers.Exo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoWonderClient;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using Android.Util;
using Exception = System.Exception;
using Uri = Android.Net.Uri;
using Facesofnaija.Helpers.Model;

namespace Facesofnaija.Activities.NativePost.Post
{
    public class ApiPostAsync
    {
        private readonly Activity ActivityContext;
        private readonly NativePostAdapter NativeFeedAdapter;
        private readonly WRecyclerView WRecyclerView;
        private static bool ShowFindMoreAlert;
        private static PostModelType LastAdsType = PostModelType.AdMob3;
        public static List<PostDataObject> PostCacheList { private set; get; }
        public string CurrentFeedOffset = "0";

        public ApiPostAsync(WRecyclerView recyclerView, NativePostAdapter adapter)
        {
            try
            {
                ActivityContext = adapter.ActivityContext;
                NativeFeedAdapter = adapter;
                WRecyclerView = recyclerView;
                PostCacheList = new List<PostDataObject>();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Api V2

        Random rand = new Random();
        private Task task;

        public void ExcuteDataToMainThread(string offset = "0", string typeRun = "Add", string hash = "")
        {
            try
            {
                var beforeList = NativeFeedAdapter.ListDiffer.Count;

                if (beforeList > 150)
                {
                    NativeFeedAdapter.ListDiffer.RemoveRange(10, System.Math.Min(30, beforeList));
                    NativeFeedAdapter.NotifyItemRangeRemoved(10, System.Math.Min(30, beforeList));

                    Console.WriteLine("API = Ended with offset " + offset + "With count of " + NativeFeedAdapter.ListDiffer.Count);
                }

                task = Task.Run(async () => await FetchFeedPostsApi(offset, typeRun, hash)).ContinueWith(task =>
                {
                    // Executes in UI thread.
                    var NewPostsList = task.Result;
                    if (NewPostsList == null || NewPostsList.Count <= 0)
                        return;

                    NativeFeedAdapter.ListDiffer.AddRange(NewPostsList);

                    var recyclerScrollFixer = new Runnable(() =>
                    {
                        if (beforeList == 0)
                            NativeFeedAdapter.NotifyDataSetChanged();

                        //WRecyclerView.SetItemAnimator(null);
                        NativeFeedAdapter.NotifyItemRangeInserted(beforeList, NewPostsList.Count);
                        Console.WriteLine("API = Ended with offset " + offset + "With count of " + NativeFeedAdapter.ListDiffer.Count);
                    });

                    WRecyclerView.Post(recyclerScrollFixer);
                    WRecyclerView.MainScrollEvent.IsLoading = false;

                    WRecyclerView.Visibility = ViewStates.Visible;
                    WRecyclerView?.ShimmerInflater?.Hide();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public async Task<List<AdapterModelsClass>> FetchFeedPostsApi(string offset = "0", string typeRun = "Add", string hash = "")
        {
            try
            {
                // FIX: Corrected task status check - was preventing feed loads
                // Old buggy logic: if ((task == null) && (task?.IsCompleted == false || task?.Status == TaskStatus.Running))
                // Problem: (task == null) means task?.IsCompleted returns null (not false), so whole condition always fails
                if (task != null && (task.IsCompleted == false || task.Status == TaskStatus.Running))
                {
                    Console.WriteLine("API = Task already running, returning...");
                    return null;
                }

                int apiStatus;
                dynamic respond;
                WRecyclerView.Hash = hash;

                if (WRecyclerView.MainScrollEvent.IsLoading)
                    return null;


                //=================
                var adId = NativeFeedAdapter.ListDiffer.LastOrDefault(a => a.TypeView == PostModelType.AdsPost && a.PostData.PostType == "ad")?.PostData?.Id ?? "";
                //var adId = "0";

                //=================
                Console.WriteLine("API = Started FetchNewsFeedApi " + offset);
                Trace.BeginSection("API = Started FetchNewsFeedApi " + offset);
                WRecyclerView.MainScrollEvent.IsLoading = true;

                switch (NativeFeedAdapter.NativePostType)
                {
                    case NativeFeedType.Global:
                        (apiStatus, respond) = await GetGlobalPostDirect(offset, adId);

                        if (offset == "0" && (apiStatus != 200 || respond is not PostObject directFirstPage || !HasRenderablePosts(directFirstPage)))
                            (apiStatus, respond) = await GetGlobalPostDirect("0", adId);
                        break;
                    case NativeFeedType.User:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_user_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                        break;
                    case NativeFeedType.Group:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_group_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                        break;
                    case NativeFeedType.Page:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_page_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                        break;
                    case NativeFeedType.Event:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_event_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                        break;
                    case NativeFeedType.Saved:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "saved", "", "", "", adId);
                        break;
                    case NativeFeedType.HashTag:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "hashtag", "", hash, "", adId);
                        break;
                    case NativeFeedType.Video:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost("5", offset, "get_random_videos", "", "", "", adId);
                        break;
                    case NativeFeedType.Popular:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetPopularPost(AppSettings.PostApiLimitOnScroll, offset);
                        break;
                    case NativeFeedType.Boosted:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetBoostedPost();
                        break;
                    case NativeFeedType.Live:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetLivePost();
                        break;
                    case NativeFeedType.Advertise:
                        (apiStatus, respond) = await RequestsAsync.Advertise.GetAdvertisePost(AppSettings.PostApiLimitOnScroll, offset);
                        break;
                    default:
                        (apiStatus, respond) = (400, null);
                        break;
                }

                Trace.EndSection();

                if (WRecyclerView.SwipeRefreshLayoutView is { Refreshing: true })
                    WRecyclerView.SwipeRefreshLayoutView.Refreshing = false;

                var countList2 = NativeFeedAdapter.ListDiffer.Count;

                //Load fake data =================
                Trace.BeginSection("LoadDataApi Start " + offset);
                int randomNumber = rand.Next(1212, 1122334499);

                List<AdapterModelsClass> TemporaryPostList = new List<AdapterModelsClass>();

                Console.WriteLine("API = LoadDataApi Start " + offset);

                var extractedResult = TryGetPostObject(respond);
                if (apiStatus != 200 || extractedResult?.Data == null)
                {
                    WRecyclerView.MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(ActivityContext, respond);

                    ActivityContext?.RunOnUiThread(() =>
                    {
                        try
                        {
                            if (WRecyclerView.SwipeRefreshLayoutView is { Refreshing: true })
                                WRecyclerView.SwipeRefreshLayoutView.Refreshing = false;

                            WRecyclerView.Visibility = ViewStates.Visible;
                            WRecyclerView?.ShimmerInflater?.Hide();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                }
                else
                {
                    respond = extractedResult;
                    if (typeRun == "FirstInsert")
                    {
                        InsertTopDataApi(apiStatus, respond);
                    }
                    else
                    {
                        await Task.Run(() => LoadDataApi(apiStatus, respond, offset, typeRun)).ConfigureAwait(false);
                    }
                }

                return TemporaryPostList;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
                return null;
            }
        }

        public async Task<int> LoadDataApiAsync(int apiStatus, dynamic respond, string offset, string typeRun = "Add")
        {
            //offset = "10";


            if (respond is PostObject results)
            {
                await Task.Run(() =>
                {
                    Trace.BeginSection("LoadDataApiAsync For Each Simulation");
                    foreach (PostDataObject post in from post in results.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                    {

                        // add = true;
                        var combiner = new FeedCombiner(null, NativeFeedAdapter.ListDiffer, ActivityContext);

                        if (NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                        {
                            if (results.Data.Count < 6 && NativeFeedAdapter.ListDiffer.Count < 6)
                                if (!ShowFindMoreAlert)
                                {
                                    ShowFindMoreAlert = true;

                                    combiner.AddFindMoreAlertPostView("Pages");
                                    combiner.AddFindMoreAlertPostView("Groups");
                                }

                            var check1 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedGroupsBox);
                            if (check1 == null && AppSettings.ShowSuggestedGroup && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedGroupCount == 0 && ListUtils.SuggestedGroupList.Count > 0)
                                combiner.AddSuggestedBoxPostView(PostModelType.SuggestedGroupsBox);

                            var check2 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedUsersBox);
                            if (check2 == null && AppSettings.ShowSuggestedUser && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedUserCount == 0 && ListUtils.SuggestedUserList.Count > 0)
                                combiner.AddSuggestedBoxPostView(PostModelType.SuggestedUsersBox);

                            var check3 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedPagesBox);
                            if (check3 == null && AppSettings.ShowSuggestedPage && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedPageCount == 0 && ListUtils.SuggestedPageList.Count > 0)
                                combiner.AddSuggestedBoxPostView(PostModelType.SuggestedPagesBox);
                        }
                        else if (NativeFeedAdapter.NativePostType == NativeFeedType.Advertise)
                        {
                            post.PostType = "ad";
                        }

                        if (NativeFeedAdapter.ListDiffer.Count % (AppSettings.ShowAdNativeCount * 10) == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowAdMobNativePost)
                            if (LastAdsType == PostModelType.AdMob1)
                            {
                                LastAdsType = PostModelType.AdMob2;
                                combiner.AddAdsPostView(PostModelType.AdMob1);
                            }
                            else if (LastAdsType == PostModelType.AdMob2)
                            {
                                LastAdsType = PostModelType.AdMob3;
                                combiner.AddAdsPostView(PostModelType.AdMob2);
                            }
                            else if (LastAdsType == PostModelType.AdMob3)
                            {
                                LastAdsType = PostModelType.AdMob1;
                                combiner.AddAdsPostView(PostModelType.AdMob3);
                            }

                        var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext);
                        if (post.PostType == "ad" && AppSettings.ShowAdvertise)
                        {
                            combine.AddAdsPost();
                        }
                        else
                        {
                            bool isPromoted = post.IsPostBoosted == "1" || post.SharedInfo.SharedInfoClass != null && post.SharedInfo.SharedInfoClass?.IsPostBoosted == "1";
                            if (isPromoted)
                            {
                                if (NativeFeedAdapter.ListDiffer.Count == 0)
                                    combine.CombineDefaultPostSections();
                                else
                                {
                                    var p = NativeFeedAdapter.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.PromotePost);
                                    if (p != null)
                                        combine.CombineDefaultPostSections();
                                    else
                                        combine.CombineDefaultPostSections("Top");

                                }
                            }
                            else
                            {
                                combine.CombineDefaultPostSections();
                            }
                        }

                        if (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowAdNativeCount == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowFbNativeAds)
                            combiner.AddAdsPostView(PostModelType.FbAdNative);
                    }

                });
            }

            return NativeFeedAdapter.ItemCount;




        }

        #endregion


        #region Api

        public async Task FetchNewsFeedApiPosts(string offset = null, string typeRun = "Add", string hash = "")
        {
            try
            {
            offset ??= CurrentFeedOffset;
            if (typeRun == "Refresh") CurrentFeedOffset = "0";

            Log.Warn("FON_TIMELINE", $"FetchNewsFeedApiPosts called: offset={offset} typeRun={typeRun}");

            if (!Methods.CheckConnectivity())
            {
                Log.Warn("FON_TIMELINE", "No connectivity — aborting feed fetch");
                WRecyclerView.MainScrollEvent.IsLoading = false;
                return;
            }

            WRecyclerView.Hash = hash;
            int apiStatus;
            dynamic respond;

            WRecyclerView.MainScrollEvent.IsLoading = true;
            var adId = NativeFeedAdapter.ListDiffer.LastOrDefault(a => a.TypeView == PostModelType.AdsPost && a.PostData != null && a.PostData.PostType == "ad")?.PostData?.Id ?? "";
            Log.Warn("FON_TIMELINE", $"Calling GetGlobalPostDirect offset={offset}");
            
            switch (NativeFeedAdapter.NativePostType)
            {
                case NativeFeedType.Global:
                    (apiStatus, respond) = await GetGlobalPostDirect(offset, adId);
                    if (apiStatus != 200 || respond is not PostObject firstPage || !HasRenderablePosts(firstPage))
                        (apiStatus, respond) = await GetGlobalPostDirect("0", adId);
                    break;
                case NativeFeedType.User:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_user_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                    break;
                case NativeFeedType.Group:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_group_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                    break;
                case NativeFeedType.Page:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_page_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                    break;
                case NativeFeedType.Event:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_event_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                    break;
                case NativeFeedType.Saved:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "saved", "", "", "", adId);
                    break;
                case NativeFeedType.HashTag:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "hashtag", "", hash, "", adId);
                    break;
                case NativeFeedType.Video:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost("5", offset, "get_random_videos", "", "", "", adId);
                    break;
                case NativeFeedType.Popular:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetPopularPost(AppSettings.PostApiLimitOnScroll, offset);
                    break;
                case NativeFeedType.Boosted:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetBoostedPost();
                    break;
                case NativeFeedType.Live:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetLivePost();
                    break;
                case NativeFeedType.Advertise:
                    (apiStatus, respond) = await RequestsAsync.Advertise.GetAdvertisePost(AppSettings.PostApiLimitOnScroll, offset);
                    break;
                default:
                    return;
            }

            var extractedResult = TryGetPostObject(respond);
            Log.Warn("FON_TIMELINE", $"GetGlobalPostDirect result: apiStatus={apiStatus} posts={extractedResult?.Data?.Count ?? -1}");
            if (apiStatus != 200 || extractedResult?.Data == null)
            {
                WRecyclerView.MainScrollEvent.IsLoading = false;
                Log.Debug("FON_TIMELINE", $"Feed failed — apiStatus={apiStatus} respond={(respond?.ToString() ?? "").Substring(0, System.Math.Min(200, (respond?.ToString() ?? "").Length))}");
                Methods.DisplayReportResult(ActivityContext, respond);

                ActivityContext?.RunOnUiThread(() =>
                {
                    try
                    {
                        if (WRecyclerView.SwipeRefreshLayoutView is { Refreshing: true })
                            WRecyclerView.SwipeRefreshLayoutView.Refreshing = false;

                        WRecyclerView.Visibility = ViewStates.Visible;
                        WRecyclerView?.ShimmerInflater?.Hide();
                        ShowEmptyPage();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            else
            {
                respond = extractedResult;
                Log.Warn("FON_TIMELINE", $"Feed OK — loading {extractedResult.Data.Count} posts into adapter typeRun={typeRun}");
                if (typeRun == "FirstInsert")
                {
                    InsertTopDataApi(apiStatus, respond);
                }
                else
                {
                    PostObject postData = (PostObject)extractedResult;
                    if (postData.Data?.Count > 0)
                    {
                        var lastPost = postData.Data.Last();
                        if (!string.IsNullOrEmpty(lastPost?.Id))
                        {
                            CurrentFeedOffset = lastPost.Id;
                            Log.Warn("FON_TIMELINE", $"CurrentFeedOffset updated to {CurrentFeedOffset}");
                        }
                    }
                    await Task.Run(() => LoadDataApi(apiStatus, postData, offset, typeRun)).ConfigureAwait(false);
                }
            }
            }
            catch (Exception ex)
            {
                Log.Error("FON_TIMELINE", $"FetchNewsFeedApiPosts EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                Methods.DisplayReportResultTrack(ex);
                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
        }

        private static bool HasRenderablePosts(PostObject result)
        {
            return result?.Data?.Any(p => !string.IsNullOrEmpty(p?.PostId) && (p.Publisher != null || p.UserData != null)) == true;
        }

        private PostObject TryGetPostObject(dynamic respond)
        {
            try
            {
                if (respond is PostObject postObject)
                    return postObject;

                if (respond is JObject jObject)
                {
                    var direct = jObject.ToObject<PostObject>();
                    if (direct?.Data != null)
                        return direct;

                    var arr = jObject["data"] as JArray ?? jObject["posts"] as JArray;
                    if (arr != null)
                    {
                        var list = arr.ToObject<List<PostDataObject>>();
                        if (list != null)
                            return new PostObject { Data = list };
                    }
                }

                if (respond is string json && !string.IsNullOrWhiteSpace(json) && json.TrimStart().StartsWith("{"))
                {
                    var parsed = JObject.Parse(json);
                    var direct = parsed.ToObject<PostObject>();
                    if (direct?.Data != null)
                        return direct;

                    var arr = parsed["data"] as JArray ?? parsed["posts"] as JArray;
                    if (arr != null)
                    {
                        var list = arr.ToObject<List<PostDataObject>>();
                        if (list != null)
                            return new PostObject { Data = list };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TryGetPostObject parse error: {ex.Message}");
            }

            return null;
        }

        public async Task FetchSearchForPosts(string offset, string id, string searchQuery, string type)
        {
            if (!Methods.CheckConnectivity())
                return;

            var (apiStatus, respond) = await RequestsAsync.Posts.SearchForPosts(AppSettings.PostApiLimitOnScroll, offset, id, searchQuery, type);
            if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
            {
                WRecyclerView.MainScrollEvent.IsLoading = false;
                Methods.DisplayReportResult(ActivityContext, respond);
            }
            else await Task.Run(() => LoadDataApi(apiStatus, respond, offset)).ConfigureAwait(false);
        }

        public void LoadDataApi(int apiStatus, dynamic respond, string offset, string typeRun = "Add")
        {
            try
            {
                if (respond is PostObject result)
                {
                    Console.WriteLine($"DEBUG LoadDataApi: apiStatus={apiStatus}, result.Data.Count={result.Data?.Count ?? 0}, offset={offset}, typeRun={typeRun}");

                    var countList = NativeFeedAdapter.ItemCount;
                    if (result.Data.Count > 0)
                    {
                        Console.WriteLine($"DEBUG LoadDataApi: Posts before RemoveAll: {result.Data.Count}");
                        result.Data.RemoveAll(a => a.Publisher == null && a.UserData == null);
                        Console.WriteLine($"DEBUG LoadDataApi: Posts after RemoveAll: {result.Data.Count}");
                        GetAllPostLive(result.Data);

                        if (offset == "0" && countList > 10 && typeRun == "Insert" && NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                        {
                            result.Data.Reverse();
                            bool add = false;

                            foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.PostData?.PostId == post.PostId && a.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                            {
                                add = true;
                                ListUtils.NewPostList.Add(post);
                            }

                            ActivityContext?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    if (add && WRecyclerView.PopupBubbleView != null && WRecyclerView.PopupBubbleView.Visibility != ViewStates.Visible && AppSettings.ShowNewPostOnNewsFeed)
                                        WRecyclerView.PopupBubbleView.Visibility = ViewStates.Visible;
                                    else
                                        WRecyclerView.PopupBubbleView.Visibility = WRecyclerView.PopupBubbleView.Visibility;
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else
                        {
                            bool add = false;
                            Trace.BeginSection("LoadDataApi Start");
                            Console.WriteLine($"DEBUG LoadDataApi: Processing {result.Data.Count} posts, current adapter count={NativeFeedAdapter.ListDiffer.Count}");
                            foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                            {
                                add = true;
                                var detectedType = PostFunctions.GetAdapterType(post);
                                Console.WriteLine($"DEBUG FEED TYPE: postId={post.PostId}, postType={post.PostType}, detected={detectedType}, fileFull={post.PostFileFull}, file={post.PostFile}, thumb={post.PostFileThumb}, textLen={(post.Orginaltext ?? post.PostText ?? string.Empty).Length}");
                                var combiner = new FeedCombiner(null, NativeFeedAdapter.ListDiffer, ActivityContext);

                                if (NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                                {
                                    if (result.Data.Count < 6 && NativeFeedAdapter.ListDiffer.Count < 6)
                                        if (!ShowFindMoreAlert)
                                        {
                                            ShowFindMoreAlert = true;

                                            combiner.AddFindMoreAlertPostView("Pages");
                                            combiner.AddFindMoreAlertPostView("Groups");
                                        }

                                    var check1 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedGroupsBox);
                                    if (check1 == null && AppSettings.ShowSuggestedGroup && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedGroupCount == 0 && ListUtils.SuggestedGroupList.Count > 0)
                                        combiner.AddSuggestedBoxPostView(PostModelType.SuggestedGroupsBox);

                                    var check2 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedUsersBox);
                                    if (check2 == null && AppSettings.ShowSuggestedUser && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedUserCount == 0 && ListUtils.SuggestedUserList.Count > 0)
                                        combiner.AddSuggestedBoxPostView(PostModelType.SuggestedUsersBox);

                                    var check3 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedPagesBox);
                                    if (check3 == null && AppSettings.ShowSuggestedPage && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedPageCount == 0 && ListUtils.SuggestedPageList.Count > 0)
                                        combiner.AddSuggestedBoxPostView(PostModelType.SuggestedPagesBox);
                                }
                                else if (NativeFeedAdapter.NativePostType == NativeFeedType.Advertise)
                                {
                                    post.PostType = "ad";
                                }

                                if (NativeFeedAdapter.ListDiffer.Count % (AppSettings.ShowAdNativeCount * 10) == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowAdMobNativePost)
                                    if (LastAdsType == PostModelType.AdMob1)
                                    {
                                        LastAdsType = PostModelType.AdMob2;
                                        combiner.AddAdsPostView(PostModelType.AdMob1);
                                    }
                                    else if (LastAdsType == PostModelType.AdMob2)
                                    {
                                        LastAdsType = PostModelType.AdMob3;
                                        combiner.AddAdsPostView(PostModelType.AdMob2);
                                    }
                                    else if (LastAdsType == PostModelType.AdMob3)
                                    {
                                        LastAdsType = PostModelType.AdMob1;
                                        combiner.AddAdsPostView(PostModelType.AdMob3);
                                    }

                                var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext);
                                if (post.PostType == "ad" && AppSettings.ShowAdvertise)
                                {
                                    combine.AddAdsPost();
                                }
                                else
                                {
                                    bool isPromoted = post.IsPostBoosted == "1" || post.SharedInfo.SharedInfoClass != null && post.SharedInfo.SharedInfoClass?.IsPostBoosted == "1";
                                    if (isPromoted)
                                    {
                                        if (NativeFeedAdapter.ListDiffer.Count == 0)
                                            combine.CombineDefaultPostSections();
                                        else
                                        {
                                            var p = NativeFeedAdapter.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.PromotePost);
                                            if (p != null)
                                                combine.CombineDefaultPostSections();
                                            else
                                                combine.CombineDefaultPostSections("Top");
                                        }
                                    }
                                    else
                                    {
                                        combine.CombineDefaultPostSections();
                                    }
                                }
                                Trace.BeginSection("LoadDataApi ForEach");
                                if (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowAdNativeCount == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowFbNativeAds)
                                    combiner.AddAdsPostView(PostModelType.FbAdNative);
                            }
                            Trace.BeginSection("LoadDataApi End");
                            Console.WriteLine($"DEBUG LoadDataApi: Final add={add}, adapter count after loop={NativeFeedAdapter.ListDiffer.Count}");
                            if (add)
                            {
                                var d = new Runnable(() =>
                                {
                                    Android.Util.Log.Warn("FON_TIMELINE", $"NotifyDataSetChanged: total={NativeFeedAdapter.ListDiffer.Count} prev={countList}");
                                    NativeFeedAdapter.NotifyDataSetChanged();
                                });
                                WRecyclerView.Post(d);
                            }
                            //else
                            //{
                            //    ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short); 
                            //}
                        }
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG LoadDataApi: result.Data.Count is 0 or null");
                    }

                    ActivityContext?.RunOnUiThread(() =>
                    {
                        try
                        {
                            if (WRecyclerView.SwipeRefreshLayoutView is { Refreshing: true })
                                WRecyclerView.SwipeRefreshLayoutView.Refreshing = false;

                            WRecyclerView.Visibility = ViewStates.Visible;
                            WRecyclerView?.ShimmerInflater?.Hide();

                            if (typeRun == "Refresh")
                                WRecyclerView.ScrollToPosition(0);

                            //if (NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                            //    WRecyclerView.DataPostJson = JsonConvert.SerializeObject(result);

                            ShowEmptyPage();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
        }

        public void LoadTopDataApi(List<PostDataObject> list)
        {
            try
            {
                NativeFeedAdapter.ListDiffer.Clear();
                NativeFeedAdapter.NotifyDataSetChanged();

                var combiner = new FeedCombiner(null, NativeFeedAdapter.ListDiffer, ActivityContext);
                combiner.AddPostBoxPostView("feed", -1);

                switch (AppSettings.ShowStory)
                {
                    case true:
                        combiner.AddStoryPostView(new List<StoryDataObject>());
                        break;
                }

                combiner.AddGreetingAlertPostView();
                combiner.AddCommunitiesAlertPostView();
                combiner.AddAnnouncementAlertPostView();

                switch (list.Count)
                {
                    case > 0:
                        {
                            bool add = false;
                            foreach (var post in from post in list let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                            {
                                add = true;
                                var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext);
                                switch (post.PostType)
                                {
                                    case "ad" when AppSettings.ShowAdvertise:
                                        combine.AddAdsPost();
                                        break;
                                    default:
                                        combine.CombineDefaultPostSections();
                                        break;
                                }
                            }

                            switch (PostCacheList?.Count)
                            {
                                case > 0:
                                    LoadBottomDataApi(PostCacheList.Take(30).ToList());
                                    break;
                            }

                            switch (add)
                            {
                                case true:
                                    ActivityContext?.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            NativeFeedAdapter.NotifyDataSetChanged();
                                            ListUtils.NewPostList.Clear();
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                    break;
                            }

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InsertTopDataApi(int apiStatus, dynamic respond)
        {
            try
            {
                if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
                {
                    WRecyclerView.MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    result.Data.RemoveAll(a => a.Publisher == null && a.UserData == null);
                    GetAllPostLive(result.Data);
                    result.Data.Reverse();

                    switch (result.Data.Count)
                    {
                        case > 0:
                            {
                                bool add = false;
                                foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                                {
                                    add = true;
                                    var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext);
                                    switch (post.PostType)
                                    {
                                        case "ad" when AppSettings.ShowAdvertise:
                                            combine.AddAdsPost("Top");
                                            break;
                                        default:
                                            combine.CombineDefaultPostSections("Top");
                                            break;
                                    }
                                }

                                switch (add)
                                {
                                    case true:
                                        ActivityContext?.RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                NativeFeedAdapter.NotifyDataSetChanged();
                                                ListUtils.NewPostList.Clear();
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                            }
                                        });
                                        break;
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void LoadMemoriesDataApi(int apiStatus, dynamic respond, List<AdapterModelsClass> diffList)
        {
            try
            {
                switch (WRecyclerView.MainScrollEvent.IsLoading)
                {
                    case true:
                        return;
                }

                WRecyclerView.MainScrollEvent.IsLoading = true;

                if (apiStatus != 200 || respond is not FetchMemoriesObject result || result.Data == null)
                {
                    WRecyclerView.MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    if (WRecyclerView.SwipeRefreshLayoutView != null && WRecyclerView.SwipeRefreshLayoutView.Refreshing)
                        WRecyclerView.SwipeRefreshLayoutView.Refreshing = false;

                    var countList = NativeFeedAdapter.ItemCount;
                    switch (result.Data.Posts.Count)
                    {
                        case > 0:
                            {
                                result.Data.Posts.RemoveAll(a => a.Publisher == null && a.UserData == null);
                                result.Data.Posts.Reverse();

                                foreach (var post in from post in result.Data.Posts let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                                {
                                    switch (post.Publisher)
                                    {
                                        case null when post.UserData == null:
                                            continue;
                                        default:
                                            {
                                                var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext);
                                                combine.CombineDefaultPostSections();
                                                break;
                                            }
                                    }
                                }

                                ActivityContext?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        WRecyclerView.Visibility = ViewStates.Visible;
                                        WRecyclerView?.ShimmerInflater?.Hide();

                                        var d = new Runnable(() => { NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.ListDiffer.Count - countList); }); d.Run();
                                        GC.Collect();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                                break;
                            }
                    }
                }

                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
        }

        public async Task FetchLoadMoreNewsFeedApiPosts()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    return;

                if (NativeFeedAdapter.NativePostType != NativeFeedType.Global)
                    return;

                switch (PostCacheList?.Count)
                {
                    case > 40:
                        return;
                }

                var diff = NativeFeedAdapter.ListDiffer;
                var list = new List<AdapterModelsClass>(diff);
                switch (list.Count)
                {
                    case <= 20:
                        return;
                }

                var item = list.LastOrDefault();

                var lastItem = list.IndexOf(item);

                item = list[lastItem];

                string offset;

                switch (item.TypeView)
                {
                    case PostModelType.Divider:
                    case PostModelType.ViewProgress:
                    case PostModelType.AdMob1:
                    case PostModelType.AdMob2:
                    case PostModelType.AdMob3:
                    case PostModelType.FbAdNative:
                    case PostModelType.AdsPost:
                    case PostModelType.SuggestedPagesBox:
                    case PostModelType.SuggestedGroupsBox:
                    case PostModelType.SuggestedUsersBox:
                    case PostModelType.CommentSection:
                    case PostModelType.AddCommentSection:
                        item = list.LastOrDefault(a => a.TypeView != PostModelType.Divider && a.TypeView != PostModelType.ViewProgress && a.TypeView != PostModelType.AdMob1 && a.TypeView != PostModelType.AdMob2 && a.TypeView != PostModelType.AdMob3 && a.TypeView != PostModelType.FbAdNative && a.TypeView != PostModelType.AdsPost && a.TypeView != PostModelType.SuggestedPagesBox && a.TypeView != PostModelType.SuggestedGroupsBox && a.TypeView != PostModelType.SuggestedUsersBox && a.TypeView != PostModelType.CommentSection && a.TypeView != PostModelType.AddCommentSection);
                        offset = item?.PostData?.PostId ?? "0";
                        Console.WriteLine(offset);
                        break;
                    default:
                        offset = item.PostData?.PostId ?? "0";
                        break;
                }

                Console.WriteLine(offset);

                int apiStatus;
                dynamic respond;

                switch (NativeFeedAdapter.NativePostType)
                {
                    case NativeFeedType.Global:
                        var adId = NativeFeedAdapter.ListDiffer.LastOrDefault(a => a.TypeView == PostModelType.AdsPost && a.PostData.PostType == "ad")?.PostData?.Id ?? "";
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_news_feed", "", "", WRecyclerView.GetFilter(), adId, WRecyclerView.GetPostType());
                        break;
                    default:
                        return;
                }

                if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
                {
                    Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    PostCacheList ??= new List<PostDataObject>();

                    var countList = PostCacheList?.Count ?? 0;
                    switch (result.Data?.Count)
                    {
                        case > 0:
                            {
                                result.Data.RemoveAll(a => a.Publisher == null && a.UserData == null);

                                switch (countList)
                                {
                                    case > 0:
                                        {
                                            foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                                            {
                                                PostCacheList.Add(post);
                                            }

                                            break;
                                        }
                                    default:
                                        PostCacheList = new List<PostDataObject>(result.Data);
                                        break;
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private bool LoadBottomDataApi(List<PostDataObject> list)
        {
            try
            {
                var countList = NativeFeedAdapter.ItemCount;
                switch (list?.Count)
                {
                    case > 0:
                        {
                            bool add = false;
                            foreach (var post in from post in list let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                            {
                                add = true;
                                var combiner = new FeedCombiner(null, NativeFeedAdapter.ListDiffer, ActivityContext);

                                switch (NativeFeedAdapter.NativePostType)
                                {
                                    case NativeFeedType.Global:
                                        {
                                            var check1 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedGroupsBox);
                                            switch (check1)
                                            {
                                                case null when AppSettings.ShowSuggestedGroup && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedGroupCount == 0 && ListUtils.SuggestedGroupList.Count > 0:
                                                    combiner.AddSuggestedBoxPostView(PostModelType.SuggestedGroupsBox);
                                                    break;
                                            }

                                            var check2 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedUsersBox);
                                            switch (check2)
                                            {
                                                case null when AppSettings.ShowSuggestedUser && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedUserCount == 0 && ListUtils.SuggestedUserList.Count > 0:
                                                    combiner.AddSuggestedBoxPostView(PostModelType.SuggestedUsersBox);
                                                    break;
                                            }

                                            var check3 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedPagesBox);
                                            switch (check3)
                                            {
                                                case null when AppSettings.ShowSuggestedPage && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedPageCount == 0 && ListUtils.SuggestedPageList.Count > 0:
                                                    combiner.AddSuggestedBoxPostView(PostModelType.SuggestedPagesBox);
                                                    break;
                                            }

                                            break;
                                        }
                                }

                                switch (NativeFeedAdapter.ListDiffer.Count % (AppSettings.ShowAdNativeCount * 10))
                                {
                                    case 0 when NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowAdMobNativePost:
                                        switch (LastAdsType)
                                        {
                                            case PostModelType.AdMob1:
                                                LastAdsType = PostModelType.AdMob2;
                                                combiner.AddAdsPostView(PostModelType.AdMob1);
                                                break;
                                            case PostModelType.AdMob2:
                                                LastAdsType = PostModelType.AdMob3;
                                                combiner.AddAdsPostView(PostModelType.AdMob2);
                                                break;
                                            case PostModelType.AdMob3:
                                                LastAdsType = PostModelType.AdMob1;
                                                combiner.AddAdsPostView(PostModelType.AdMob3);
                                                break;
                                        }

                                        break;
                                }

                                var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext);
                                switch (post.PostType)
                                {
                                    case "ad" when AppSettings.ShowAdvertise:
                                        combine.AddAdsPost();
                                        break;
                                    default:
                                        {
                                            bool isPromoted = post.IsPostBoosted == "1" || post.SharedInfo.SharedInfoClass != null && post.SharedInfo.SharedInfoClass?.IsPostBoosted == "1";
                                            if (isPromoted)
                                            {
                                                if (NativeFeedAdapter.ListDiffer.Count == 0)
                                                    combine.CombineDefaultPostSections();
                                                else
                                                {
                                                    var p = NativeFeedAdapter.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.PromotePost);
                                                    if (p != null)
                                                        combine.CombineDefaultPostSections();
                                                    else
                                                        combine.CombineDefaultPostSections("Top");
                                                }
                                            }
                                            else
                                            {
                                                combine.CombineDefaultPostSections();
                                            }

                                            break;
                                        }
                                }

                                switch (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowAdNativeCount)
                                {
                                    case 0 when NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowFbNativeAds:
                                        combiner.AddAdsPostView(PostModelType.FbAdNative);
                                        break;
                                }
                            }

                            switch (add)
                            {
                                case true:
                                    ActivityContext?.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            var d = new Runnable(() => { NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.ListDiffer.Count - countList); }); d.Run();
                                            GC.Collect();
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                    break;
                            }

                            PostCacheList.RemoveRange(0, list.Count - 1);
                            ActivityContext?.RunOnUiThread(ShowEmptyPage);

                            return add;
                        }
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                NativeFeedAdapter.SetLoaded();
                var viewProgress = NativeFeedAdapter.ListDiffer.FirstOrDefault(anjo => anjo.TypeView == PostModelType.ViewProgress);
                if (viewProgress != null)
                    WRecyclerView.RemoveByRowIndex(viewProgress);

                var emptyStateCheck = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox /*&& a.TypeView != PostModelType.SearchForPosts*/);
                if (emptyStateCheck != null)
                {
                    var emptyStateChecker = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                    if (emptyStateChecker != null && NativeFeedAdapter.ListDiffer.Count > 1)
                        WRecyclerView.RemoveByRowIndex(emptyStateChecker);
                }
                else
                {
                    var emptyStateChecker = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                    if (emptyStateChecker == null)
                    {
                        var data = new AdapterModelsClass
                        {
                            TypeView = PostModelType.EmptyState,
                            Id = 744747447,
                        };
                        NativeFeedAdapter.ListDiffer.Add(data);
                        NativeFeedAdapter.NotifyDataSetChanged();
                    }
                }

                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void GetAllPostLive(List<PostDataObject> list)
        {
            try
            {
                var listLivePost = list?.Where(a => a.LiveTime != null && a.LiveTime.Value > 0 && a.IsStillLive != null && a.IsStillLive.Value && string.IsNullOrEmpty(a.AgoraResourceId) && string.IsNullOrEmpty(a.PostFile))?.ToList();
                switch (NativeFeedAdapter.NativePostType)
                {
                    case NativeFeedType.Global:
                        var mainActivity = TabbedMainActivity.GetInstance();
                        var checkSection = mainActivity?.NewsFeedTab?.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                        if (checkSection != null)
                        {
                            if (listLivePost?.Count > 0)
                            {
                                foreach (var post in from post in listLivePost let check = checkSection.StoryList.FirstOrDefault(a => a?.DataLivePost?.PostId == post.PostId) where check == null select post)
                                {
                                    if (checkSection.StoryList.Count > 1)
                                    {
                                        checkSection.StoryList.Insert(1, new StoryDataObject
                                        {
                                            Avatar = post.Publisher.Avatar,
                                            Type = "Live",
                                            Username = ActivityContext.GetText(Resource.String.Lbl_Live),
                                            DataLivePost = post
                                        });
                                    }
                                    else
                                    {
                                        checkSection.StoryList.Add(new StoryDataObject
                                        {
                                            Avatar = post.Publisher.Avatar,
                                            Type = "Live",
                                            Username = WoWonderTools.GetNameFinal(post.Publisher),
                                            DataLivePost = post,
                                        });
                                    }
                                }

                                ActivityContext?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        var d = new Runnable(() => { mainActivity?.NewsFeedTab?.PostFeedAdapter?.NotifyItemChanged(mainActivity.NewsFeedTab.PostFeedAdapter.ListDiffer.IndexOf(checkSection)); });
                                        d.Run();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                        break;
                        //wael
                        //case NativeFeedType.User when NativeFeedAdapter.IdParameter != UserDetails.UserId:
                        //    var userProfileActivity = UserProfileActivity.GetInstance();
                        //    if (userProfileActivity != null)
                        //    {
                        //        var data = userProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.UserProfileInfoHeaderSection);
                        //        if (listLivePost?.Count > 0)
                        //        {
                        //            UserDetails.DataLivePost = listLivePost.FirstOrDefault();

                        //            if (data != null)
                        //                data.InfoUserModel.IsLive = true; 
                        //        }
                        //        else
                        //        {
                        //            UserDetails.DataLivePost = null;

                        //            if (data != null)
                        //                data.InfoUserModel.IsLive = false;
                        //        }

                        //        ActivityContext?.RunOnUiThread(() =>
                        //        {
                        //            try
                        //            {
                        //                var d = new Runnable(() => { userProfileActivity?.PostFeedAdapter?.NotifyItemChanged(userProfileActivity.PostFeedAdapter.ListDiffer.IndexOf(data)); });
                        //                d.Run();
                        //            }
                        //            catch (Exception e)
                        //            {
                        //                Methods.DisplayReportResultTrack(e);
                        //            }
                        //        });
                        //    }

                        break;
                    default:
                        return;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static PostDataObject RegexFilterText(PostDataObject item)
        {
            try
            {
                Dictionary<string, string> dataUser = new Dictionary<string, string>();

                if (string.IsNullOrEmpty(item.PostText))
                    return item;

                if (item.PostText.Contains("data-id="))
                {
                    try
                    {
                        //string pattern = @"(data-id=[""'](.*?)[""']|href=[""'](.*?)[""']|'>(.*?)a>)";

                        string pattern = @"(data-id=[""'](.*?)[""']|href=[""'](.*?)[""'])";
                        var aa = Regex.Matches(item.PostText, pattern);
                        switch (aa?.Count)
                        {
                            case > 0:
                                {
                                    for (int i = 0; i < aa.Count; i++)
                                    {
                                        string userid = "";
                                        if (aa.Count > i)
                                            userid = aa[i]?.Value?.Replace("data-id=", "").Replace('"', ' ').Replace(" ", "");

                                        string username = "";
                                        if (aa.Count > i + 1)
                                            username = aa[i + 1]?.Value?.Replace("href=", "").Replace('"', ' ').Replace(" ", "").Replace(InitializeWoWonder.WebsiteUrl, "").Replace("\n", "");

                                        if (string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(username))
                                            continue;

                                        var data = dataUser.FirstOrDefault(a => a.Key?.ToString() == userid && a.Value?.ToString() == username);
                                        if (data.Key != null)
                                            continue;

                                        i++;

                                        switch (string.IsNullOrWhiteSpace(userid))
                                        {
                                            case false when !string.IsNullOrWhiteSpace(username) && !dataUser.ContainsKey(userid):
                                                dataUser.Add(userid, username);
                                                break;
                                        }
                                    }

                                    item.RegexFilterList = new Dictionary<string, string>(dataUser);
                                    return item;
                                }
                            default:
                                return item;
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }

                return item;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return item;
            }
        }

        public static async Task GetAllPostVideo()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    return;

                var (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost("10", "0", "get_news_feed", "", "", "0", "0", "video");
                if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
                {
                    // Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    var respondList = result.Data?.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = ListUtils.VideoReelsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            var checkViewed = ListUtils.VideoReelsViewsList.FirstOrDefault(a => a.Id == item.Id);
                            if (checkViewed == null)
                            {
                                if (!AppSettings.ShowYouTubeReels && !string.IsNullOrEmpty(item.PostYoutube))
                                    continue;

                                ListUtils.VideoReelsList.Add(item);
                            }
                        }

                        var list = ListUtils.VideoReelsList.Take(5);
                        foreach (var videoObject in list)
                        {
                            new PreCachingExoPlayerVideo(Application.Context).CacheVideosFiles(Uri.Parse(videoObject.PostFileFull));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Direct API call fallback for feed endpoints when standard client fails
        /// Aligns with web app v2 API structure
        /// </summary>
        private async Task<(int, dynamic)> GetGlobalPostDirect(string offset, string adId)
        {
            try
            {
                using var handler = new Xamarin.Android.Net.AndroidMessageHandler { AllowAutoRedirect = false };
                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) };
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {UserDetails.AccessToken}");

                var urlMock = $"http://172.236.19.52/api/v2/endpoints/posts_mock.php";
                var url = $"http://172.236.19.52/api/v2/endpoints/posts.php";
                var limit = AppSettings.PostApiLimitOnScroll;
                var filter = WRecyclerView.GetFilter();
                var postType = WRecyclerView.GetPostType();

                var postData = new[]
                {
                    new KeyValuePair<string, string>("type", "get_news_feed"),
                    new KeyValuePair<string, string>("limit", limit),
                    new KeyValuePair<string, string>("after_post_id", offset),
                    new KeyValuePair<string, string>("filter", string.IsNullOrWhiteSpace(filter) ? "all" : filter),
                    new KeyValuePair<string, string>("post_type", string.IsNullOrWhiteSpace(postType) ? "all" : postType),
                    new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey ?? ""),
                    new KeyValuePair<string, string>("access_token", UserDetails.AccessToken ?? ""),
                };

                // First try compatibility endpoint used during migration.
                using (var content = new FormUrlEncodedContent(postData))
                {
                    try
                    {
                        var response = await client.PostAsync(urlMock, content).ConfigureAwait(false);
                        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(json) && !json.TrimStart().StartsWith("<"))
                        {
                            var jObject = JObject.Parse(json);
                            var statusText = jObject["api_status"]?.ToString();
                            int.TryParse(statusText, out int status);

                            if (status == 200)
                            {
                                var posts = jObject.ToObject<PostObject>();
                                if (posts?.Data?.Count > 0)
                                    return (200, (dynamic)posts);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore and continue to real endpoint.
                    }
                }

                using (var content = new FormUrlEncodedContent(postData))
                {
                    var response = await client.PostAsync(url, content).ConfigureAwait(false);
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(json) || json.TrimStart().StartsWith("<"))
                        return (400, "Invalid response");

                    try
                    {
                        var jObject = JObject.Parse(json);
                        var statusText = jObject["api_status"]?.ToString();
                        int.TryParse(statusText, out int status);

                        if (status == 200)
                        {
                            var posts = jObject.ToObject<PostObject>();
                            return (200, (dynamic)posts);
                        }

                        return (status == 0 ? 400 : status, (dynamic)jObject);
                    }
                    catch (Exception parseEx)
                    {
                        Console.WriteLine($"GetGlobalPostDirect parse error: {parseEx.Message}");
                        return (400, json);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetGlobalPostDirect exception: {ex.Message}");
                return (400, ex.Message);
            }
        }
    }
}
