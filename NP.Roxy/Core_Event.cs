using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace NP.Roxy
{
    public partial class Core
    {
        private Dictionary<string, int> EventMap = new Dictionary<string, int>();

        internal void AddEvent(string fullEventName, int idx)
        {
            if (EventMap.TryGetValue(fullEventName, out int currentIdx))
            {
                if (currentIdx == idx)
                    return;

                throw new Exception($"Roxy Usage Error: different index {idx} vs old index {currentIdx} for the event {fullEventName}.");
            }

            EventMap.Add(fullEventName, idx);
        }

        internal int GetEventIdx(string fullEventName)
        {
            if (EventMap.TryGetValue(fullEventName, out int idx))
            {
                return idx;
            }

            return -1;
        }


        Dictionary<IEventSymbol, int> _eventDictionary =
            new Dictionary<IEventSymbol, int>(SymbolComparer.TheSymbolComparer);

        private static IEventSymbol GetEventSymbol(INamedTypeSymbol typeSymbol, string eventName)
        {
            IEventSymbol eventSymbol = typeSymbol.GetMemberByName<IEventSymbol>(eventName);

            if (eventSymbol == null)
            {
                throw new Exception($"Roxy Usage Error: event {eventName} is not found within type {typeSymbol.GetFullTypeStrWithNamespace()}");
            }

            return eventSymbol;
        }

        public void AddEventInfo(INamedTypeSymbol typeSymbol, string eventName, int idx = 0)
        {
            IEventSymbol eventSymbol = GetEventSymbol(typeSymbol, eventName);

            _eventDictionary[eventSymbol] = idx;
        }

        public void AddEventInfo(Type eventContainerType, string eventName, int idx = 0)
        {
            INamedTypeSymbol typeSymbol = this.GetTypeSymbol(eventContainerType);

            AddEventInfo(typeSymbol, eventName, idx);
        }

        public void AddEventInfo<TEventContainer>(string eventName, int idx = 0)
        {
            AddEventInfo(typeof(TEventContainer), eventName, idx);
        }

        public static void AddEventIdxInfo<TEventContainer>(string eventName, int idx = 0)
        {
            TheCore.AddEventInfo<TEventContainer>(eventName, idx);
        }

        public int GetEventThisIdx(IEventSymbol eventSymbol)
        {
            if (_eventDictionary.TryGetValue(eventSymbol, out int idx))
            {
                return idx;
            }
            return -1;
        }

        public int GetEventThisIdx(Type eventContainerType, string eventName)
        {
            INamedTypeSymbol typeSymbol = this.GetTypeSymbol(eventContainerType);

            IEventSymbol eventSymbol = GetEventSymbol(typeSymbol, eventName);

            return GetEventThisIdx(eventSymbol);
        }
    }
}
