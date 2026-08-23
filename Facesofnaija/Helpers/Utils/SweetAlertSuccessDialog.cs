using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using System;

namespace Facesofnaija.Helpers.Utils
{
    public static class SweetAlertSuccessDialog
    {
        public static void Show(Activity activity, string title, string message, string buttonText = "OK, Got It", Action onOkClicked = null)
        {
            if (activity == null || activity.IsFinishing || activity.IsDestroyed)
                return;

            activity.RunOnUiThread(() =>
            {
                try
                {
                    var dialogView = activity.LayoutInflater.Inflate(Resource.Layout.dialog_sweet_alert_success, null);
                    var txtTitle = dialogView.FindViewById<TextView>(Resource.Id.dialog_title);
                    var txtMessage = dialogView.FindViewById<TextView>(Resource.Id.dialog_message);
                    var btnOk = dialogView.FindViewById<AppCompatButton>(Resource.Id.dialog_btn_ok);

                    if (txtTitle != null && !string.IsNullOrWhiteSpace(title))
                        txtTitle.Text = title;

                    if (txtMessage != null && !string.IsNullOrWhiteSpace(message))
                        txtMessage.Text = message;

                    if (btnOk != null && !string.IsNullOrWhiteSpace(buttonText))
                        btnOk.Text = buttonText;

                    var dialogBuilder = new AndroidX.AppCompat.App.AlertDialog.Builder(activity);
                    dialogBuilder.SetView(dialogView);
                    dialogBuilder.SetCancelable(false);

                    var alertDialog = dialogBuilder.Create();
                    alertDialog.Window?.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));

                    btnOk?.SetOnClickListener(new ClickListener(() =>
                    {
                        try { alertDialog.Dismiss(); } catch { }
                        onOkClicked?.Invoke();
                    }));

                    alertDialog.Show();

                    var displayMetrics = activity.Resources?.DisplayMetrics;
                    if (displayMetrics != null && alertDialog.Window != null)
                    {
                        int width = (int)(displayMetrics.WidthPixels * 0.88);
                        alertDialog.Window.SetLayout(width, ViewGroup.LayoutParams.WrapContent);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SWEET_ALERT_ERROR: {ex.Message}");
                    try
                    {
                        var fallback = new AndroidX.AppCompat.App.AlertDialog.Builder(activity);
                        fallback.SetTitle(title);
                        fallback.SetMessage(message);
                        fallback.SetPositiveButton(buttonText, (s, a) => onOkClicked?.Invoke());
                        fallback.SetCancelable(false);
                        fallback.Show();
                    }
                    catch
                    {
                    }
                }
            });
        }

        private class ClickListener : Java.Lang.Object, View.IOnClickListener
        {
            private readonly Action Action;
            public ClickListener(Action action) => Action = action;
            public void OnClick(View v) => Action?.Invoke();
        }
    }
}
