using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.Content;
using Google.Common.Util.Concurrent;
using Java.Lang;
using System;
using System.Linq;
using Facesofnaija.Activities.Communities.Groups;
using Facesofnaija.Activities.Communities.Pages;
using Facesofnaija.Activities.QrCode.Tools;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace Facesofnaija.Activities.QrCode.Fragment
{
    public class QrScannerFragment : AndroidX.Fragment.App.Fragment, IQrCodeFoundListener
    {
        #region Variables Basic

        private PreviewView PreviewView;
        private IListenableFuture CameraProviderFuture;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.QrCodeScanLayout, container, false);
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
                PreviewView = view.FindViewById<PreviewView>(Resource.Id.activity_main_previewView);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Camera

        public void RequestCamera()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    StartCamera();
                }
                else
                {
                    if (ContextCompat.CheckSelfPermission(Context, Manifest.Permission.Camera) == Permission.Granted)
                    {
                        StartCamera();
                    }
                    else
                    {
                        //request Code 103
                        new PermissionsController(Activity).RequestPermission(103);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartCamera()
        {
            try
            {
                if (CameraProviderFuture == null)
                {
                    CameraProviderFuture = ProcessCameraProvider.GetInstance(Activity);

                    CameraProviderFuture.AddListener(new Runnable(() =>
                    {
                        try
                        {
                            var cameraProvider = CameraProviderFuture.Get();
                            if (cameraProvider is ProcessCameraProvider provider)
                            {
                                BindCameraPreview(provider);
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    }), ContextCompat.GetMainExecutor(Activity));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BindCameraPreview(ProcessCameraProvider cameraProvider)
        {
            try
            {
                // Preview
                Preview preview = new Preview.Builder()
                    .Build();

                // Select back camera as a default
                CameraSelector cameraSelector = new CameraSelector.Builder()
                    .RequireLensFacing(CameraSelector.LensFacingBack)
                    .Build();

                preview.SetSurfaceProvider(PreviewView.SurfaceProvider);

                ImageAnalysis imageAnalysis = new ImageAnalysis.Builder()
                    .SetTargetResolution(new Size(480, 640))
                    .SetTargetRotation((int)SurfaceOrientation.Rotation0)
                    .SetBackpressureStrategy(ImageAnalysis.StrategyKeepOnlyLatest)
                    .Build();

                imageAnalysis.SetAnalyzer(ContextCompat.GetMainExecutor(Activity), new QrCodeImageAnalyzer(this));

                // Unbind use cases before rebinding
                cameraProvider.UnbindAll();

                // Bind use cases to camera
                var camera = cameraProvider.BindToLifecycle(this, cameraSelector, imageAnalysis, preview);

                if (camera.CameraInfo.HasFlashUnit)
                {
                    camera.CameraControl.EnableTorch(false);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Listener Camera

        public void OnQrCodeFound(string qrCode)
        {
            try
            {
                if (!string.IsNullOrEmpty(qrCode) && qrCode.Contains(InitializeWoWonder.WebsiteUrl))
                    OpenQrCode(qrCode);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void QrCodeNotFound()
        {

        }

        #endregion

        #region Get Type QR Code

        private async void OpenQrCode(string qrCode)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //Show a progress
                    AndHUD.Shared.Show(Activity, GetText(Resource.String.Lbl_Loading));

                    var username = qrCode.Split("/").LastOrDefault();
                    var (apiStatus, respond) = await RequestsAsync.Global.CheckTypeUrlAsync(username);
                    if (apiStatus == 200)
                    {
                        if (respond is TypeUrlObject result)
                        {
                            AndHUD.Shared.Dismiss(Activity);

                            switch (result.Type)
                            {
                                case "user":
                                    WoWonderTools.OpenProfile(Activity, result.Id, null);
                                    break;
                                case "page":
                                    {
                                        var intent = new Intent(Activity, typeof(PageProfileActivity));
                                        //intent.PutExtra("PageObject", JsonConvert.SerializeObject(item));
                                        intent.PutExtra("PageId", result.Id);
                                        Activity.StartActivity(intent);
                                        break;
                                    }
                                case "group":
                                    {
                                        var intent = new Intent(Activity, typeof(GroupProfileActivity));
                                        //intent.PutExtra("GroupObject", JsonConvert.SerializeObject(item));
                                        intent.PutExtra("GroupId", result.Id);
                                        Activity.StartActivity(intent);
                                        break;
                                    }
                            }
                            Activity.Finish();
                        }
                    }
                    else
                        Methods.DisplayAndHudErrorResult(Activity, respond);
                }
                else
                {
                    ToastUtils.ShowToast(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(Activity);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}