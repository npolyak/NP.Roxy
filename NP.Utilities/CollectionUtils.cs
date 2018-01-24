// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author(s) of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Utilities
{
    public static class CollectionUtils
    {
        public static void DoForEach<T>
        (
            this IEnumerable<T> collection,
            Action<T> action
        )
        {
            if (collection == null)
                return;

            foreach (T item in collection)
            {
                action(item);
            }
        }

        public static void DoForEach<T>
        (
            this IEnumerable<T> collection,
            Action<T, int> action
        )
        {
            if (collection == null)
                return;

            int i = 0;
            foreach (T item in collection)
            {
                action(item, i);

                i++;
            }
        }



        public static IEnumerable<ResultType>
            GetItemsOfType<BaseType, ResultType>(this IEnumerable<BaseType> inputCollection)
            where ResultType : BaseType
        {
            return inputCollection
                    .Where((item) => item is ResultType)
                    .Select(item => item.TypeConvert<ResultType>());
        }

        public static IEnumerable<T> ToTypedCollection<T>(this IEnumerable coll)
        {
            if (coll == null)
                yield break;

            foreach (object obj in coll)
            {
                yield return (T)obj;
            }
        }

        public static IEnumerable<T> ToCollection<T>(this T item)
        {
            if (item == null)
                return Enumerable.Empty<T>();

            return new T[] { item };
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> coll, T item)
        { 
            return coll.Concat(item.ToCollection());
        }

        public static void AddAll
        (
            this IList collection,
            IEnumerable collectionToAdd,
            bool noDuplicates = false
        )
        {
            if ((collection == null) || (collectionToAdd == null))
                return;

            foreach (object el in collectionToAdd)
            {
                if (noDuplicates)
                {
                    if (collection.Contains(el))
                        continue;
                }

                collection.Add(el);
            }
        }

        public static void AddAll<T>
        (
            this IList<T> collection, 
            IEnumerable<T> collectionToAdd, 
            bool noDuplicates = false
        )
        {
            if ((collection == null) || (collectionToAdd == null))
                return;

            foreach (T el in collectionToAdd)
            {
                if (noDuplicates)
                {
                    if (collection.Contains(el))
                        continue;
                }

                collection.Add(el);
            }
        }

        public static void RemoveMatching<T, LookupType>
        (
            this IList<T> collection,
            LookupType lookupItem,
            Func<T, LookupType, bool> predicate
        )
        {
            IEnumerable<T> matchingItems =
                collection.FindItems(lookupItem, predicate).ToList();

            foreach (T item in matchingItems)
            {
                collection.Remove(item);
            }
        }

        public static void RemoveAll
        (
            this IList collection, 
            IEnumerable itemsToRemove)
        {
            if (itemsToRemove == null)
                return;

            foreach(object itemToRemove in itemsToRemove)
            {
                collection.Remove(itemToRemove);
            }
        }

        public static void RemoveAll(this IList collection)
        {
            if (collection == null)
                return;

            var elements =
                collection.ToTypedCollection<object>().ToList();

            foreach (object el in elements)
            {
                collection.Remove(el);
            }
        }

        public static bool IsNullOrEmpty(this IEnumerable collection)
        {
            if (collection == null)
                return true;

            int i = 0;
            foreach (object obj in collection)
            {
                i++;

                if (i > 0)
                    return false;
            }

            return true;
        }

        public static bool HasData(this IEnumerable collection)
        {
            return !collection.IsNullOrEmpty();
        }

        public static IEnumerable NullToEmpty(this IEnumerable collection)
        {
            if (collection == null)
                yield break;

            foreach (object obj in collection)
                yield return obj;
        }

        public static IEnumerable<T> NullToEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection == null)
                return Enumerable.Empty<T>();

            return collection;
        }


        public static bool HasElementByPredicate<T, LookupType>
        (
            this IEnumerable<T> coll,
            LookupType element,
            Func<T, LookupType, bool> predicate
        )
        {
            return !coll.Where((item) => predicate(item, element)).IsNullOrEmpty();
        }

        public static IEnumerable<(T Item, int Idx)> FindItemsAndIndecies<T, TLookup>
        (
            this IEnumerable<T> coll,
            TLookup lookupItem,
            Func<T, TLookup, bool> predicate
        )
        {
            return coll.Select((item, idx) =>  (item, idx)).Where(tuple => predicate(tuple.item, lookupItem));
        }

        public static IEnumerable<T> FindItems<T, TLookup>
        (
            this IEnumerable<T> coll,
            TLookup lookupItem,
            Func<T, TLookup, bool> predicate
        )
        {
            return coll.FindItemsAndIndecies(lookupItem, predicate).Select(tuple => tuple.Item); //coll.Where(item => predicate(item, lookupItem));
        }

        public static (T Item, int Idx) FindItemAndIdx<T, LookupType>
        (
            this IEnumerable<T> coll,
            LookupType lookupItem,
            Func<T, LookupType, bool> predicate = null
        )
            where T : class
        {
            if (predicate == null)
            {
                predicate = (item, searchItem) => item.ObjEquals(searchItem);
            }

            return coll.FindItemsAndIndecies(lookupItem, predicate).FirstOrDefault();
        }

        public static T FindItem<T, LookupType>
        (
            this IEnumerable<T> coll,
            LookupType lookupItem,
            Func<T, LookupType, bool> predicate = null
        )
            where T : class
        {
            return coll.FindItemAndIdx(lookupItem, predicate).Item;
        }

        public static int FindIdx<T, LookupType>
        (
            this IEnumerable<T> coll,
            LookupType lookupItem,
            Func<T, LookupType, bool> predicate = null
        )
            where T : class
        {
            return coll.FindItemAndIdx(lookupItem, predicate).Idx;
        }

        public static void AddIfNotThere<T>
        (
            this IList<T> collection,
            T elementToAdd,
            Func<T, T, bool> predicate = null
        )
        {
            if (predicate == null)
                predicate = (el, elToAdd) => el.ObjEquals(elToAdd);

            if (collection.HasElementByPredicate(elementToAdd, predicate))
                return;

            collection.Add(elementToAdd);
        }


        public static bool IsInValCollection<T>(this T obj, IEnumerable<object> valueCollection)
        {
            if (valueCollection.IsNullOrEmpty())
                return false;

            foreach (object val in valueCollection)
            {
                if (obj.ObjEquals(val))
                    return true;
            }

            return false;
        }

        public static bool IsIn<T>(this T obj, params object[] vals)
        {
            return obj.IsInValCollection(vals);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable collection)
        {
            if (collection == null)
                return null;

            ObservableCollection<T> result = new ObservableCollection<T>();

            foreach (object obj in collection)
            {
                result.Add((T)obj);
            }

            return result;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection)
        {
            return ((IEnumerable)collection).ToObservableCollection<T>();
        }

        public static IEnumerable<object> ToSingleObjectCollection(this object obj)
        {
            return new []{ obj };
        }

        public static IEnumerable<object> ToCollection(this object obj)
        {
            if (obj == null)
                return Enumerable.Empty<object>();

            IEnumerable<object> result = obj as IEnumerable<object>;

            if (result != null)
                return result;

            return obj.ToSingleObjectCollection();
        }

        public static void InsertInOrder<T>(this IList<T> list, T item, Func<T, T, int> comparisonFn)
        {
            int idx = 0;
            foreach(T listItem in list)
            {
                if (comparisonFn(listItem, item) >= 0)
                {
                    list.Insert(idx, item);
                    return;
                }

                idx++;
            }

            list.Add(item);
        }

        public static void InsertAllInOrder<T>(this IList<T> list, IEnumerable<T> items, Func<T, T, int> comparisonFn)
        {
            if (items == null)
                return;

            items.DoForEach(item => list.InsertInOrder(item, comparisonFn));
        }
    }
}
