using System.Collections.ObjectModel;

namespace MultiConcernsRoxyTest.RoxyViewModels
{
    public interface IBusinessGroup
    {
        string Name { get; set; }

        ObservableCollection<ISelectableRemovablePerson> People { get; }
    }
}
