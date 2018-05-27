using NP.Roxy;
using NP.Roxy.TypeConfigImpl;
using NP.Concepts.Behaviors;
using System.ComponentModel;

namespace MultiConcernsRoxyTest.RoxyViewModels
{
    public interface ISelectableRemovableItem<T> : ISelectableItem<T>, IRemovable, INotifyPropertyChanged
       where T : ISelectableItem<T>
    {
    }

    public interface ISelectableRemovableWrapper<T>
        where T : ISelectableItem<T>
    {
        SelectableItem<T> Selectable { get; }

        Removable Removable { get; }
    }

    public interface ISelectableRemovablePerson :
        IPersonDataVM, ISelectableRemovableItem<ISelectableRemovablePerson>
    {

    }

    public interface ISelectableRemovablePersonWrapper :
        ISelectableRemovableWrapper<ISelectableRemovablePerson>
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

    public interface ISelectableRemovableBusinessGroupWrapper :
        ISelectableRemovableWrapper<ISelectableRemovableBusinessGroup>,
        IRemovableCollectionBehaviorWrapper
    {
        ParentChildSelectionBehavior<ISelectableRemovableBusinessGroup, ISelectableRemovablePerson> TheParentChildSelectionBehavior { get; }
    }

    public static class RoxyModelAssembler
    {
        public static void AssembleSelectableRemovablePerson()
        {
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<ISelectableRemovablePerson, PersonDataVM, ISelectableRemovablePersonWrapper>();

            typeConfig.SetEventArgThisIdx(nameof(INotifyPropertyChanged.PropertyChanged), 0);

            typeConfig.ConfigurationCompleted();
        }

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
                Core.FindOrCreateTypeConfig<SingleSelectionObservableCollection<ISelectableRemovableBusinessGroup>, NoType, IRemovableCollectionBehaviorWrapper>();

            typeConfig.SetThisMemberMap
            (
                nameof(IRemovableCollectionBehaviorWrapper.TheRemovableCollectionBehavior),
                nameof(RemovableCollectionBehavior.TheCollection)
            );

            typeConfig.ConfigurationCompleted();
        }
    }
}
