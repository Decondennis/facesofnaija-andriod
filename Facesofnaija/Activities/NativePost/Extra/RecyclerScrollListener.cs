using Android.Views;
using AndroidX.RecyclerView.Widget;
using System;
using Facesofnaija.Helpers.Utils;

namespace Facesofnaija.Activities.NativePost.Extra
{
    /// <summary>
    /// RecyclerView scroll listener for implementing infinite scroll / load more functionality
    /// </summary>
    public class RecyclerScrollListener : RecyclerView.OnScrollListener
    {
        private readonly RecyclerView RecyclerView;
        
        public bool IsLoading { get; set; }
        public event EventHandler LoadMoreEvent;

        public RecyclerScrollListener(RecyclerView recyclerView)
        {
            RecyclerView = recyclerView;
            IsLoading = false;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            try
            {
                base.OnScrolled(recyclerView, dx, dy);

                // Trigger pagination only while scrolling down.
                if (dy <= 0)
                    return;

                if (IsLoading)
                    return;

                var layoutManager = recyclerView.GetLayoutManager();
                
                if (layoutManager is LinearLayoutManager linearLayoutManager)
                {
                    var visibleItemCount = layoutManager.ChildCount;
                    var totalItemCount = layoutManager.ItemCount;
                    var firstVisibleItemPosition = linearLayoutManager.FindFirstVisibleItemPosition();
                    var lastVisibleItemPosition = linearLayoutManager.FindLastVisibleItemPosition();

                    // Load more when the last visible item is within 5 of the end
                    if (lastVisibleItemPosition >= totalItemCount - 5 && firstVisibleItemPosition >= 0)
                    {
                        IsLoading = true;
                        LoadMoreEvent?.Invoke(this, EventArgs.Empty);
                    }
                }
                else if (layoutManager is GridLayoutManager gridLayoutManager)
                {
                    var visibleItemCount = layoutManager.ChildCount;
                    var totalItemCount = layoutManager.ItemCount;
                    var firstVisibleItemPosition = gridLayoutManager.FindFirstVisibleItemPosition();
                    var lastVisibleItemPosition = gridLayoutManager.FindLastVisibleItemPosition();

                    if (lastVisibleItemPosition >= totalItemCount - 5 && firstVisibleItemPosition >= 0)
                    {
                        IsLoading = true;
                        LoadMoreEvent?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
        {
            try
            {
                base.OnScrollStateChanged(recyclerView, newState);
                
                // Optional: Handle scroll state changes if needed
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}
