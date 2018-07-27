using NP.Concepts.Behaviors;
using System.ComponentModel;
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
        [PullMember(WrapperMemberName = null, WrappedMemberName = "TheCollection")]
        [Plugin]
        RemovableCollectionBehavior TheRemovableCollectionBehavior { get; }
    }

    public interface ISelectableRemovableBusinessGroupImplementor :
        ISelectableRemovableItem<ISelectableRemovableBusinessGroup>,
        ISelectableRemovableImplementor<ISelectableRemovableBusinessGroup>
    {
        [Plugin]
        ObservableCollection<ISelectableRemovablePerson> People { get; }

        [PullMember(WrapperMemberName = null, WrappedMemberName = "Parent")]
        [PullMember(WrapperMemberName = "People", WrappedMemberName = "Children")]
        [Plugin]
        ParentChildSelectionBehavior<ISelectableRemovableBusinessGroup, ISelectableRemovablePerson> TheParentChildSelectionBehavior { get; }

        [PullMember(WrapperMemberName = "People", WrappedMemberName = "TheCollection")]
        [Plugin]
        SingleSelectionBehavior<ISelectableRemovablePerson> TheSingleSelectionBehavior { get; }

        [PullMember(WrapperMemberName = "People", WrappedMemberName = "TheCollection")]
        [Plugin]
        RemovableCollectionBehavior TheRemovableCollectionBehavior { get; }
    }
}
