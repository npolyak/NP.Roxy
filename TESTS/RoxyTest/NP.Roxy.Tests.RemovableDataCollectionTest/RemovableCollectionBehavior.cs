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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.Tests.SelectableDataTest
{
    public class RemovableCollectionBehavior
    {
        IDisposable _behaviorDisposable = null;

        IEnumerable<IRemovable> _collection;
        public IEnumerable<IRemovable> TheCollection
        {
            get => _collection;

            set
            {
                if (ReferenceEquals(_collection, value))
                    return;

                _collection = value;

                _behaviorDisposable =
                    _collection?.AddBehavior
                    (
                        (item) => item.RemoveEvent += Item_RemoveEvent,
                        (item) => item.RemoveEvent -= Item_RemoveEvent
                    );
            }
        }

        private void Item_RemoveEvent(IRemovable itemToRemove)
        {
            (TheCollection as IList).Remove(itemToRemove);
        }
    }
}
