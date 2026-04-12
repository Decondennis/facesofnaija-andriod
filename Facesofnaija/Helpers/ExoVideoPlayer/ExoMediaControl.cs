using Android.Content;
using Android.Views;
using System;

// TODO: Implement MediaPlayer media controls
// Original ExoPlayer implementation saved as ExoMediaControl.cs.bak

namespace Facesofnaija.Helpers.ExoVideoPlayer
{
    public class ExoMediaControl
    {
        private readonly Context Context;

        public ExoMediaControl(Context context)
        {
            Context = context;
        }

        public View GetView()
        {
            // TODO: Return MediaPlayer control view
            return null;
        }

        public void Play()
        {
            // TODO: Implement play
        }

        public void Pause()
        {
            // TODO: Implement pause
        }

        public void Release()
        {
            // TODO: Implement release
        }
    }
}
