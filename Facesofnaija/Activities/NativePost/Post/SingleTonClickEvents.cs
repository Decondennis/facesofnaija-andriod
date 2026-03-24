
using Android.Views;
using AndroidX.RecyclerView.Widget;
using System;
using Facesofnaija.Activities.Articles.Adapters;
using Facesofnaija.Activities.Comment.Adapters;
using Facesofnaija.Activities.Movies.Adapters;
using WoWonderClient.Classes.Articles;
using WoWonderClient.Classes.Movies;
using WoWonderClient.Classes.Posts;

namespace Facesofnaija.Activities.NativePost.Post
{

    public class GlobalClickEventArgs : EventArgs
    {
        public int Position { get; set; }
        public AdapterHolders.SoundPostViewHolder HolderSound { get; set; }
        public View View { get; set; }
        public PostDataObject NewsFeedClass { get; set; }
        public AdapterModelsClass AdapterModelsClass { get; set; }
        public NativeFeedType NativeFeedType { get; set; }
    }

    public class CommentReplyClickEventArgs : EventArgs
    {
        public int Position { get; set; }
        public View View { get; set; }
        public CommentObjectExtra CommentObject { get; set; }
        public CommentAdapterViewHolder Holder { get; set; }
    }

    public class ProfileClickEventArgs : EventArgs
    {
        public int Position { get; set; }
        public RecyclerView.ViewHolder Holder { get; set; }
        public View View { get; set; }
        public CommentObjectExtra CommentClass { get; set; }
        public PostDataObject NewsFeedClass { get; set; }
    }

    public class CommentReplyArticlesClickEventArgs : EventArgs
    {
        public int Position { get; set; }
        public View View { get; set; }
        public ArticlesCommentAdapterViewHolder Holder { get; set; }
        public CommentsArticlesObject CommentObject { get; set; }
    }

    public class CommentReplyMoviesClickEventArgs : EventArgs
    {
        public int Position { get; set; }
        public View View { get; set; }
        public MoviesCommentAdapterViewHolder Holder { get; set; }
        public CommentsMoviesObject CommentObject { get; set; }
    }

}