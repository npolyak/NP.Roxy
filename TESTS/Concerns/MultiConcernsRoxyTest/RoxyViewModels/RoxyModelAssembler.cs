using NP.Roxy;
using NP.Roxy.TypeConfigImpl;
using NP.Utilities.Behaviors;
using System;
using System.Collections.Generic;
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
        ISelectableRemovableItem<ISelectableRemovablePerson>
    {

    }

    public interface ISelectableRemovablePersonWrapper :
        ISelectableRemovableWrapper<ISelectableRemovablePerson>
    {

    }

    public static class RoxyModelAssembler
    {

        public static void AssembleSelectableRemovablePerson()
        {
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<ISelectableRemovablePerson, PersonDataVM, ISelectableRemovablePersonWrapper>();

            typeConfig.SetEventArgThisIdx(nameof(ISelectableRemovablePerson.IsSelectedChanged), 0);
            typeConfig.SetEventArgThisIdx(nameof(ISelectableRemovablePerson.RemoveEvent), 0);

            typeConfig.ConfigurationCompleted();
        }
    }
}
