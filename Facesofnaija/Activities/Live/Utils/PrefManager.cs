using Android.Content;
using Facesofnaija.Activities.Live.Page;

namespace Facesofnaija.Activities.Live.Utils
{
    public class PrefManager
    {
        public static ISharedPreferences GetPreferences(Context context)
        {
            return context.GetSharedPreferences(Constants.PrefName, FileCreationMode.Private);
        }
    }
}