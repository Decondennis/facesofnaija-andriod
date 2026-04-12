using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.Dialog;
using System;

namespace WoWonder.Helpers.Utils
{
    /// <summary>
    /// MaskType enum for API compatibility with AndroidHUD
    /// </summary>
    public enum MaskType
    {
        Clear,
        Black,
        Gradient
    }

    /// <summary>
    /// Replacement for AndroidHUD - provides Material Design progress dialogs
    /// </summary>
    public static class ProgressDialogHelper
    {
        private static ProgressDialog CurrentDialog;

        /// <summary>
        /// Show a progress dialog with optional message
        /// </summary>
        public static void Show(Context context, string message = null, string title = null)
        {
            try
            {
                if (context == null || (context is Activity activity && activity.IsFinishing))
                    return;

                Dismiss();

                CurrentDialog = new ProgressDialog(context);
                CurrentDialog.SetCancelable(false);
                
                if (!string.IsNullOrEmpty(title))
                    CurrentDialog.SetTitle(title);
                
                CurrentDialog.SetMessage(message ?? "Loading...");
                CurrentDialog.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ProgressDialogHelper.Show error: " + ex.Message);
            }
        }

        /// <summary>
        /// Show progress dialog with custom status text
        /// </summary>
        public static void ShowStatus(Context context, string status)
        {
            Show(context, status);
        }

        /// <summary>
        /// Dismiss the current progress dialog
        /// </summary>
        public static void Dismiss(Context context = null)
        {
            try
            {
                if (CurrentDialog != null && CurrentDialog.IsShowing)
                {
                    CurrentDialog.Dismiss();
                    CurrentDialog = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ProgressDialogHelper.Dismiss error: " + ex.Message);
            }
        }

        /// <summary>
        /// Show a success message toast
        /// </summary>
        public static void ShowSuccessToast(Context context, string message, MaskType maskType = MaskType.Clear, TimeSpan? duration = null)
        {
            try
            {
                Dismiss();
                Toast.MakeText(context, message, ToastLength.Short)?.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ProgressDialogHelper.ShowSuccessToast error: " + ex.Message);
            }
        }

        /// <summary>
        /// Show an error message toast
        /// </summary>
        public static void ShowErrorToast(Context context, string message, MaskType maskType = MaskType.Clear, TimeSpan? duration = null)
        {
            try
            {
                Dismiss();
                Toast.MakeText(context, message, ToastLength.Short)?.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ProgressDialogHelper.ShowErrorToast error: " + ex.Message);
            }
        }

        /// <summary>
        /// Show a toast message
        /// </summary>
        public static void ShowToast(Context context, string message, MaskType maskType = MaskType.Clear, TimeSpan? duration = null)
        {
            try
            {
                Dismiss();
                Toast.MakeText(context, message, ToastLength.Short)?.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ProgressDialogHelper.ShowToast error: " + ex.Message);
            }
        }

        // Remove the MaskType enum that was nested - it's now at namespace level
    }
}
