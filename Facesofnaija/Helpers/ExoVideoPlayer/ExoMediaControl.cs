using Android.Content;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Upstream.Cache;
using System;
using System.Threading.Tasks;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.MediaPlayers;
using static Facesofnaija.Activities.NativePost.Extra.WRecyclerView;
using Object = Java.Lang.Object;

namespace Facesofnaija.Helpers.ExoVideoPlayer
{


    public class ExoMediaControl
    {
        private readonly object _lock = new object();

        public IExoPlayer Exoplayer;
        public Context MainContext;
        public VolumeState VolumeStateProvider;

        public IDataSource.IFactory HttpDataSourceFactory;
        public CacheDataSource.Factory CacheFactoryDataSource;
        public DefaultMediaSourceFactory DefaultMediaDataSourceFactory;

        public IMediaSource.IFactory MainDataSource;
        public CacheDataSink.Factory CacheDataSinkfactory;

        public DefaultDataSource.Factory DefaultDataSourcesFactory;
        private SimpleCache Cache;

        public ExoMediaControl Build(Context context)
        {
            MainContext = context;
            TrackSelector TrackSelector = new DefaultTrackSelector(MainContext);
            //var loadControl = new DefaultLoadControl.Builder().SetBufferDurationsMs(32 * 1024, 64 * 1024, 1024, 1024).;

            Exoplayer = new IExoPlayer.Builder(MainContext).Build();
            return this;
        }



        public void ConfigureDataSources()
        {
            try
            {
                //Create Video Source for http urls
                HttpDataSourceFactory = new DefaultHttpDataSource.Factory().SetAllowCrossProtocolRedirects(true);
                DefaultDataSourcesFactory = new DefaultDataSource.Factory(MainContext, HttpDataSourceFactory);



                if (PlayerSettings.EnableOfflineMode)
                {

                    Cache = MainApplication.GetInstance().ExoCache;
                    CacheFactoryDataSource = new CacheDataSource.Factory();
                    CacheDataSink dataSink = new CacheDataSink(Cache, CacheDataSink.DefaultFragmentSize);

                    //CacheFactoryDataSource.SetCache(Cache).SetUpstreamDataSourceFactory(HttpDataSourceFactory)
                    //    .SetCacheWriteDataSinkFactory(null).SetFlags(CacheDataSource.FlagIgnoreCacheOnError)
                    //    .SetEventListener(new CachEventListner());


                    CacheDataSinkfactory = new CacheDataSink.Factory();
                    CacheDataSinkfactory.SetCache(Cache).SetFragmentSize(CacheDataSink.DefaultFragmentSize);

                    CacheFactoryDataSource.SetCache(Cache).SetUpstreamDataSourceFactory(HttpDataSourceFactory).SetCacheReadDataSourceFactory(new FileDataSource.Factory()).SetCacheWriteDataSinkFactory(CacheDataSinkfactory)
                        .SetFlags(CacheDataSource.FlagBlockOnCache | CacheDataSource.FlagIgnoreCacheOnError)
                        .SetEventListener(new CachEventListner());

                    // var cacheFactoryDataSource = new CacheDataSource(Cache, DefaultDataSourcesFactory.CreateDataSource(), new FileDataSource(), dataSink, CacheDataSource.FlagBlockOnCache | CacheDataSource.FlagIgnoreCacheOnError, new CachEventListner());
                }



                MainDataSource = new DefaultMediaSourceFactory(MainContext).SetDataSourceFactory(CacheFactoryDataSource);
                //.SetLocalAdInsertionComponents(adsLoaderProvider, /* adViewProvider= */ playerView);

                //MainDataSource = new ProgressiveMediaSource.Factory(CacheFactoryDataSource);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }





        public void BuildPlayer()
        {
            if (MainDataSource != null)
                Exoplayer = new IExoPlayer.Builder(MainContext).SetMediaSourceFactory(MainDataSource).Build();

        }

        public void CreateMediaSourcePlayer()
        {
            if (MainDataSource != null)
                Exoplayer = new IExoPlayer.Builder(MainContext).SetMediaSourceFactory(MainDataSource).Build();

        }



        public IExoPlayer PrepareMediaSource(string trackurl)
        {
            try
            {
                //https://wowonder.fra1.cdn.digitaloceanspaces.com/upload/videos/2022/11/PRjaipBn2Gmj4CEV1O4e_11_a509a30225863eb0c2e5cf08e343a826_video_720p_converted.mp4
                //MediaItem mediaItem = MediaItem.FromUri(Android.Net.Uri.Parse(trackurl));
                //Exoplayer.AddMediaItem(mediaItem);
                return Exoplayer;

                //Exoplayer.Prepare();
                //Exoplayer.Play();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Exoplayer;
            }

        }

        public void PrelaodVideoCach(string trackurl)
        {

            try
            {

                //CacheDataSinkfactory = new CacheDataSink.Factory();
                //CacheDataSinkfactory.SetCache(Cache).SetFragmentSize(CacheDataSink.DefaultFragmentSize);
                //CacheFactoryDataSource.SetCacheReadDataSourceFactory(new FileDataSource.Factory()).SetCacheWriteDataSinkFactory(CacheDataSinkfactory);
                //var Cachew = new CacheDataSource.Factory().SetCache(Cache).SetUpstreamDataSourceFactory(DefaultDataSourcesFactory);

                //CacheDataSink dataSink = new CacheDataSink(Cache, CacheDataSink.DefaultFragmentSize);
                //var cacheFactoryDataSource = new CacheDataSource(Cache, DefaultDataSourcesFactory.CreateDataSource(), new FileDataSource(), dataSink, CacheDataSource.FlagBlockOnCache | CacheDataSource.FlagIgnoreCacheOnError, new CachEventListner());


                Task.Run(() =>
                {

                    var d = CacheFactoryDataSource.CreateDataSource();

                    CacheWriter cw = new CacheWriter(d, new DataSpec(Android.Net.Uri.Parse(trackurl), 0, 80 * 1024), null, new CaceWriterListner());
                    cw.Cache();

                });


            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);


            }

        }

        public class CaceWriterListner : Object, CacheWriter.IProgressListener
        {
            public void OnProgress(long requestLength, long bytesCached, long newBytesCached)
            {
                var downloadPercentage = (bytesCached * 100 / requestLength);

                var BytehasKB = System.Math.Round((double)bytesCached / 1024, 0);
                var BytenewBytesCachedKB = System.Math.Round((double)newBytesCached / 1024, 0);

                Console.WriteLine("OnProgress downloadPercentage:" + downloadPercentage + " requestLength = " + requestLength + " BytesCached = " + BytehasKB + " newBytesCached = " + BytenewBytesCachedKB);
            }
        }

        public class CachEventListner : Object, CacheDataSource.IEventListener
        {
            public void OnCachedBytesRead(long bytesCached, long requestLength)
            {
                Console.WriteLine("OnCachedBytesRead:" + bytesCached);
                var downloadPercentage = (bytesCached * 100 / requestLength);

                var BytehasKB = System.Math.Round((double)bytesCached / 1024, 0);


                Console.WriteLine("OnCachedBytesRead downloadPercentage:" + downloadPercentage + " requestLength= " + requestLength + " BytesCached = " + BytehasKB);
            }

            public void OnCacheIgnored(int p0)
            {
                Console.WriteLine("OnCacheIgnored:" + p0);


            }


        }


    }



}

