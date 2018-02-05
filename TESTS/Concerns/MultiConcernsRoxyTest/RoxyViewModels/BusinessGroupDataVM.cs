using NP.Utilities.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiConcernsRoxyTest.RoxyViewModels
{
    public interface IBusinessGroup
    {
        string Name { get; set; }

        ObservableCollection<ISelectableRemovablePerson> People { get; }
    }
}
