// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using NP.Concepts.Behaviors;
using NP.Roxy.TypeConfigImpl;
using System.Collections.ObjectModel;


namespace NP.Roxy.Tests.SelectableDataTest
{
    public interface IRemovableData : IMyData, IRemovable
    {
    }

    public interface IRemovableWrapper
    {
        Removable TheRemovable { get; }
    }


    public interface IRemovableBehaviorCollectionWrapper
    {
        RemovableCollectionBehavior TheRemovableCollectionBehavior { get; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ITypeConfig removableTypeConfig =
                Core.FindOrCreateTypeConfig<IRemovableData, NoType, IRemovableWrapper>();

            string className = removableTypeConfig.ClassName;

            removableTypeConfig.SetEventArgThisIdx(nameof(IRemovableData.RemoveEvent), 0);

            removableTypeConfig.ConfigurationCompleted();

            IRemovableData removableObj1 =
                Core.GetInstanceOfGeneratedType<IRemovableData>();

            removableObj1.FirstName = "Joe";
            removableObj1.LastName = "Doe";

            IRemovableData removableObj2 =
                 Core.GetInstanceOfGeneratedType<IRemovableData>();

            removableObj2.FirstName = "Jane";
            removableObj2.LastName = "Dane";

            #region Using removableCollectionBehavior on top of the collection
            ObservableCollection<IRemovableData> collectionWithRemovabledItems =
                new ObservableCollection<IRemovableData>();

            collectionWithRemovabledItems.Add(removableObj1);
            collectionWithRemovabledItems.Add(removableObj2);

            RemovableCollectionBehavior removableCollectionBehavior = new RemovableCollectionBehavior();

            removableCollectionBehavior.TheCollection = collectionWithRemovabledItems;

            removableObj1.Remove();

            removableObj2.Remove();
            #endregion Using removableCollectionBehavior on top of the collection


            #region dynamically creating an observable collection class with removableCollectionBehavior inside

            ITypeConfig<NoType, ObservableCollection<IRemovableData>, IRemovableBehaviorCollectionWrapper> collectionTypeConfig =
                Core.FindOrCreateTypeConfig<NoType, ObservableCollection<IRemovableData>, IRemovableBehaviorCollectionWrapper>("CollectionWithRemovableBehavior");

            collectionTypeConfig.UnInitAction =
                (intrfc, superClass, collWrapper) =>
                {
                    collWrapper.TheRemovableCollectionBehavior.TheCollection = null;
                };

            collectionTypeConfig.InitAction =
                (intrfc, superClass, collWrapper) =>
                {
                    collWrapper.TheRemovableCollectionBehavior.TheCollection = superClass;
                };

            collectionTypeConfig.ConfigurationCompleted();

            ObservableCollection<IRemovableData> collection =
                Core.GetInstanceOfGeneratedType<ObservableCollection<IRemovableData>>();

            collection.Add(removableObj1);
            collection.Add(removableObj2);

            removableObj1.Remove();

            #endregion dynamically creating an observable collection class with removableCollectionBehavior inside
        }
    }
}
