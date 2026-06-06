using System.Collections.ObjectModel;
using Facesofnaija.CustomApi.Classes.Global;

namespace Facesofnaija.Activities.Communities.Adapters
{
    public enum CommunityDashboardSectionType
    {
        Suggested,
        Joined,
        Requested
    }

    public class CommunityDashboardSectionModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public CommunityDashboardSectionType Type { get; set; }
        public ObservableCollection<CommunityDataObject> Communities { get; set; } = new ObservableCollection<CommunityDataObject>();
    }
}