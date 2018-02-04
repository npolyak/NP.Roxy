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

    public interface ISelectableRemovabledCollectionWrapper<TParent, TChild>
        where TParent : class, ISelectableItem<TParent>
        where TChild : class, ISelectableItem<TChild>
    {
        ParentChildSelectionBehavior<TParent, TChild> TheParentChildSelectionBehavior { get; }
        RemovableCollectionBehavior TheRemovableCollectionBehavior { get; }
    }


    public interface ISelectableRemovableBusinessGroup :
        IBusinessGroup,
        ISelectableRemovableItem<ISelectableRemovableBusinessGroup>
    {

    }

    public interface ISelectableRemovableBusinessGroupWrapper :
        ISelectableRemovableWrapper<ISelectableRemovableBusinessGroup>
    {
        
    }

    public static class RoxyModelAssembler
    {
        public static void AssembleSelectableRemovablePerson()
        {
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<ISelectableRemovablePerson, ISelectableRemovablePersonWrapper>();

            typeConfig.ConfigurationCompleted();
        }

        public static void AssembleSelectableRemovablePersonCollection()
        {
            //ITypeConfig typeConfig =
            //    Core.FindOrCreateTypeConfig<ObservableCollection<ISelectableRemovablePerson>, ISelectableRemovabledCollectionWrapper<k>();

            //typeConfig.SetEventArgThisIdx(nameof(ISelectableRemovablePerson.IsSelectedChanged), 0);
            //typeConfig.SetEventArgThisIdx(nameof(ISelectableRemovablePerson.RemoveEvent), 0);

            //typeConfig.ConfigurationCompleted();
        }

        public static void AssembleSelectableRemovableBusinessGroup()
        {
            ITypeConfig<ISelectableRemovableBusinessGroup, BusinessGroupDataVM, ISelectableRemovableBusinessGroupWrapper> typeConfig =
                Core.FindOrCreateTypeConfig<ISelectableRemovableBusinessGroup, BusinessGroupDataVM, ISelectableRemovableBusinessGroupWrapper>();

            //typeConfig.InitAction = ()

            //typeConfig.SetActions
            //(
            //    (IBusinessGroup businessGroup) => businessGroup.People,
            //    (ISelectableRemovableBusinessGroupWrapper wrapper) => wrapper.,
            //    (people, removable) => 
            //);

            typeConfig.ConfigurationCompleted();
        }
    }
}
