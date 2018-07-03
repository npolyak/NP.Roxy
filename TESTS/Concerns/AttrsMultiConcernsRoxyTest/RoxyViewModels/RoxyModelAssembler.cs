﻿using NP.Roxy;
using NP.Roxy.TypeConfigImpl;
using NP.Concepts.Behaviors;
using System.ComponentModel;
using NP.Concepts.Attributes;
using NP.Roxy.Attributes;
using System.Collections.ObjectModel;

namespace AttrsMultiConcernsRoxyTest.RoxyViewModels
{
    [ClassEventThisIdx(nameof(PropertyChanged))]
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
        [PullMember(WrapperMemberName = null, WrappedMemberName = nameof(RemovableCollectionBehavior.TheCollection))]
        RemovableCollectionBehavior TheRemovableCollectionBehavior { get; }
    }

    public interface ISelectableRemovableBusinessGroupWrapper :
        ISelectableRemovableWrapper<ISelectableRemovableBusinessGroup>
    {
        [SharedProperty(InitType = typeof(SingleSelectionObservableCollection<ISelectableRemovablePerson>))]
        ObservableCollection<ISelectableRemovablePerson> People { get; }

        [PullMember(WrapperMemberName = null, WrappedMemberName = nameof(ParentChildSelectionBehavior<ISelectableRemovableBusinessGroup, ISelectableRemovablePerson>.Parent))]
        [PullMember(WrapperMemberName = nameof(IBusinessGroup.People), WrappedMemberName = nameof(ParentChildSelectionBehavior <ISelectableRemovableBusinessGroup, ISelectableRemovablePerson>.Children))]
        ParentChildSelectionBehavior<ISelectableRemovableBusinessGroup, ISelectableRemovablePerson> TheParentChildSelectionBehavior { get; }

        [PullMember(WrapperMemberName = nameof(IBusinessGroup.People), WrappedMemberName = nameof(RemovableCollectionBehavior.TheCollection))]
        RemovableCollectionBehavior TheRemovableCollectionBehavior { get; }
    }

    public static class RoxyModelAssembler
    {
        public static void AssembleSelectableRemovablePerson()
        {
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<ISelectableRemovablePerson, PersonDataVM, ISelectableRemovablePersonWrapper>();

            typeConfig.ConfigurationCompleted();
        }

        public static void AssembleSelectableRemovableBusinessGroup()
        {
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<ISelectableRemovableBusinessGroup, ISelectableRemovableBusinessGroupWrapper>();

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