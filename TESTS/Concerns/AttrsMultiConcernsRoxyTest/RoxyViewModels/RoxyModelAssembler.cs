using NP.Roxy;
using NP.Roxy.TypeConfigImpl;
using NP.Concepts.Behaviors;
using System.ComponentModel;
using NP.Concepts.Attributes;
using NP.Roxy.Attributes;
using System.Collections.ObjectModel;

namespace AttrsMultiConcernsRoxyTest.RoxyViewModels
{
    public interface ISelectableRemovableItem<T> : ISelectableItem<T>, IRemovable, INotifyPropertyChanged
       where T : ISelectableItem<T>
    {
    }

    public interface ISelectableRemovableImplementor<T>
        where T : ISelectableItem<T>
    {
        [Plugin]
        SelectableItem<T> Selectable { get; }

        [Plugin]
        Removable Removable { get; }
    }

    public interface ISelectableRemovablePerson :
        IPersonDataVM, ISelectableRemovableItem<ISelectableRemovablePerson>
    {

    }

    public interface ISelectableRemovablePersonImplementor :
        ISelectableRemovableItem<ISelectableRemovablePerson>,
        ISelectableRemovableImplementor<ISelectableRemovablePerson>
    {
        [Plugin]
        PersonDataVM ThePerson { get; }
    }

    public interface ISelectableRemovableBusinessGroup :
        IBusinessGroup,
        ISelectableRemovableItem<ISelectableRemovableBusinessGroup>
    {

    }

    public interface IRemovableCollectionBehaviorImplementor
    {
        [PullMember(WrapperMemberName = null, WrappedMemberName = nameof(RemovableCollectionBehavior.TheCollection))]
        [Plugin]
        RemovableCollectionBehavior TheRemovableCollectionBehavior { get; }
    }

    public interface ISelectableRemovableBusinessGroupImplementor :
        ISelectableRemovableItem<ISelectableRemovableBusinessGroup>,
        ISelectableRemovableImplementor<ISelectableRemovableBusinessGroup>
    {
        [Plugin]
        ObservableCollection<ISelectableRemovablePerson> People { get; }

        [PullMember(WrapperMemberName = null, WrappedMemberName = nameof(ParentChildSelectionBehavior<ISelectableRemovableBusinessGroup, ISelectableRemovablePerson>.Parent))]
        [PullMember(WrapperMemberName = nameof(IBusinessGroup.People), WrappedMemberName = nameof(ParentChildSelectionBehavior <ISelectableRemovableBusinessGroup, ISelectableRemovablePerson>.Children))]
        [Plugin]
        ParentChildSelectionBehavior<ISelectableRemovableBusinessGroup, ISelectableRemovablePerson> TheParentChildSelectionBehavior { get; }

        [PullMember(WrapperMemberName = nameof(IBusinessGroup.People), WrappedMemberName = nameof(RemovableCollectionBehavior.TheCollection))]
        [Plugin]
        SingleSelectionBehavior<ISelectableRemovablePerson> TheSingleSelectionBehavior { get; }

        [PullMember(WrapperMemberName = nameof(IBusinessGroup.People), WrappedMemberName = nameof(RemovableCollectionBehavior.TheCollection))]
        [Plugin]
        RemovableCollectionBehavior TheRemovableCollectionBehavior { get; }
    }
}
