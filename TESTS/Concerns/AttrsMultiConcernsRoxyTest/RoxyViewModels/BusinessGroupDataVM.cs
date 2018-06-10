using System.Collections.ObjectModel;

namespace AttrsMultiConcernsRoxyTest.RoxyViewModels
{
    public interface IBusinessGroup
    {
        string Name { get; set; }

        ObservableCollection<ISelectableRemovablePerson> People { get; }
    }
}
