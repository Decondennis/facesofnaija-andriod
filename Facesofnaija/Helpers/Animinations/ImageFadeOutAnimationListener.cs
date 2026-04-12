using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace Facesofnaija.Helpers.Animinations
{
    public class ImageFadeOutAnimationListener : Java.Lang.Object, Animation.IAnimationListener
    {
        private readonly ImageView VideoImage;
        public ImageFadeOutAnimationListener(ImageView videoImage)
        {
            VideoImage = videoImage;
        }
        public void OnAnimationEnd(Animation animation)
        {
            VideoImage.Visibility = ViewStates.Invisible;
        }

        public void OnAnimationRepeat(Animation animation)
        {

        }

        public void OnAnimationStart(Animation animation)
        {

        }
    }
}
