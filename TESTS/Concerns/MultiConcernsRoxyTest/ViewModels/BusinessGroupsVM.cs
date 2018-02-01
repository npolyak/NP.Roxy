using NP.Utilities.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiConcernsRoxyTest.ViewModels
{
    public class BusinessGroupsVM : SingleSelectionObservableCollection<BusinessGroupVM>
    {
        IDisposable _behaviorsDisposable;
        public BusinessGroupsVM()
        {
            _behaviorsDisposable =
                this.AddBehavior
                (
                    (businessGroup) => businessGroup.RemoveEvent += BusinessGroup_RemoveEvent,
                    (businessGroup) => businessGroup.RemoveEvent -= BusinessGroup_RemoveEvent
                );
        }

        private void BusinessGroup_RemoveEvent(IRemovable businessGroupToRemove)
        {
            this.Remove((BusinessGroupVM)businessGroupToRemove);
        }
    }
}
