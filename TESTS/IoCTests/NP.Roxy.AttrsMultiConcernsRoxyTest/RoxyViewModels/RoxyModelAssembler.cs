using NP.Roxy;
using NP.Roxy.TypeConfigImpl;
using NP.Utilities;
using NP.Utilities.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.AttrsMultiConcernsRoxyTest.RoxyViewModels
{
    public interface ISelectableRemovableItem<T> : ISelectableItem<T>, IRemovable, INotifyPropertyChanged
       where T : ISelectableItem<T>
    {
    }

    public interface ISelectableRemovablePerson :
        IPersonDataVM, ISelectableRemovableItem<ISelectableRemovablePerson>
    {

    }

    public interface ISelectableRemovableBusinessGroup :
        IBusinessGroup,
        ISelectableRemovableItem<ISelectableRemovableBusinessGroup>
    {

    }

    public interface IRemovableCollectionBehaviorWrapper
    {
        RemovableCollectionBehavior TheRemovableCollectionBehavior { get; }
    }

    public class RemovableCollectionBehaviorAdaptor
    {
        public RemovableCollectionBehavior TheRemovableBehavior { get; } =
            new RemovableCollectionBehavior();


        public IEnumerable<IRemovable> TheCollection
        {
            get => TheRemovableBehavior.TheCollection;
            set => TheRemovableBehavior.TheCollection = value;
        }
    }


    public interface ISelectableRemovableBusinessGroupWrapper :
        ISelectableItemWrapper<ISelectableRemovableBusinessGroup>,
        IRemovableWrapper,
        IRemovableCollectionBehaviorWrapper
    {
        ParentChildSelectionBehavior<ISelectableRemovableBusinessGroup, ISelectableRemovablePerson> TheParentChildSelectionBehavior { get; }
    }

    public static class RoxyModelAssembler
    {
        public static void AssembleSelectableRemovableBusinessGroup()
        {
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<ISelectableRemovableBusinessGroup, ISelectableRemovableBusinessGroupWrapper>();

            typeConfig.SetInit<SingleSelectionObservableCollection<ISelectableRemovablePerson>>(nameof(IBusinessGroup.People));

            typeConfig.SetEventArgThisIdx(nameof(INotifyPropertyChanged.PropertyChanged), 0);

            typeConfig.SetThisMemberMap
            (
                nameof(ISelectableRemovableBusinessGroupWrapper.TheParentChildSelectionBehavior),
                nameof(ParentChildSelectionBehavior<ISelectableRemovableBusinessGroup, ISelectableRemovablePerson>.Parent)
            );

            typeConfig.SetMemberMap
            (
                nameof(ISelectableRemovableBusinessGroupWrapper.TheParentChildSelectionBehavior),
                nameof(ParentChildSelectionBehavior<ISelectableRemovableBusinessGroup, ISelectableRemovablePerson>.Children),
                nameof(IBusinessGroup.People)
            );

            typeConfig.SetMemberMap
            (
                nameof(ISelectableRemovableBusinessGroupWrapper.TheRemovableCollectionBehavior),
                nameof(RemovableCollectionBehavior.TheCollection),
                nameof(IBusinessGroup.People)
            );

            typeConfig.ConfigurationCompleted();
        }

        public static void AssembleBusinessGroupsCollection()
        {
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<SingleSelectionObservableCollection<ISelectableRemovableBusinessGroup>, IRemovableCollectionBehaviorWrapper>();

            typeConfig.SetThisMemberMap
            (
                nameof(IRemovableCollectionBehaviorWrapper.TheRemovableCollectionBehavior),
                nameof(RemovableCollectionBehavior.TheCollection)
            );

            typeConfig.ConfigurationCompleted();
        }
    }
}
