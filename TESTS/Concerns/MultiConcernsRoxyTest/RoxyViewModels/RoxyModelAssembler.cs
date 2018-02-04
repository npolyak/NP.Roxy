using NP.Roxy;
using NP.Roxy.TypeConfigImpl;
using NP.Utilities.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiConcernsRoxyTest.RoxyViewModels
{
    public interface ISelectableRemovableItem<T> : ISelectableItem<T>, IRemovable
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

    public interface ISelectableRemovableBusinessGroupWrapper :
        ISelectableRemovableWrapper<ISelectableRemovableBusinessGroup>
    {
        ParentChildSelectionBehavior<ISelectableRemovableBusinessGroup, ISelectableRemovablePerson> TheParentChildSelectionBehavior { get; }
        RemovableCollectionBehavior TheRemovableCollectionBehavior { get; }
    }

    public static class RoxyModelAssembler
    {
        public static void AssembleSelectableRemovablePerson()
        {
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<ISelectableRemovablePerson, ISelectableRemovablePersonWrapper>();

            typeConfig.ConfigurationCompleted();
        }

        public static void AssembleSelectableRemovableBusinessGroup()
        {
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<ISelectableRemovableBusinessGroup, ISelectableRemovableBusinessGroupWrapper>();

            typeConfig.SetInit<SingleSelectionObservableCollection<ISelectableRemovablePerson>>(nameof(IBusinessGroup.People));

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
    }
}
