using Android.App;
using Com.Google.Android.Play.Core.Review;
using Com.Google.Android.Play.Core.Tasks;
using ComTask = Com.Google.Android.Play.Core.Tasks.Task;
using Exception = System.Exception;

namespace Facesofnaija.Helpers.Utils
{
    public interface IInAppReview
    {
        /// <summary>
        /// Function to launch the In-App Review
        /// </summary>
        void LaunchReview();
    }
    public class InAppReview : IInAppReview
    {
        private readonly Activity MainActivity;

        public InAppReview(Activity mainActivity)
        {
            MainActivity = mainActivity;
        }

        public void LaunchReview()
        {
            try
            {
                var manager = ReviewManagerFactory.Create(MainActivity);

                var request = manager.RequestReviewFlow();
                request.AddOnCompleteListener(new OnCompleteListener(MainActivity, manager));
                request.AddOnFailureListener(new AppReviewOnFailureListener());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class OnCompleteListener : Java.Lang.Object, IOnCompleteListener
    {
        private readonly IReviewManager ReviewManager;
        private readonly Activity MainActivity;

        public OnCompleteListener(Activity mainActivity, IReviewManager reviewManager)
        {
            try
            {
                MainActivity = mainActivity;
                ReviewManager = reviewManager;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        void IOnCompleteListener.OnComplete(ComTask p0)
        {
            try
            {
                switch (p0.IsSuccessful)
                {
                    case false:
                        return;
                    default:
                        // Canceling the review raises an exception

                        LaunchReview(p0);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LaunchReview(ComTask p0)
        {
            try
            {
                var review = p0.GetResult(Java.Lang.Class.FromType(typeof(ReviewInfo)));
                var x = ReviewManager.LaunchReviewFlow(MainActivity, (ReviewInfo)review);
                //x.AddOnCompleteListener(new OnCompleteListener(MainActivity, ReviewManager));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class AppReviewOnFailureListener : Java.Lang.Object, IOnFailureListener
    {
        public void OnFailure(Java.Lang.Exception p0)
        {
            try
            {
                Methods.DisplayReportResultTrack(p0);
            }
            catch
            {
                // ignored
            }
        }
    }
}