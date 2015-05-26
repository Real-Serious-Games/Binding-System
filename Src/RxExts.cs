using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RSG;

namespace RSG.Internal
{
    public static class RxExts
    {
        /// <summary>
        ///  Create an observable sequence of all Property Changed events.
        /// </summary>
        public static IObservable<PropertyChangedEventArgs> OnAnyPropertyChanges<T>(this T source)
            where T : INotifyPropertyChanged
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                handler => handler.Invoke,
                h => source.PropertyChanged += h,
                h => source.PropertyChanged -= h
            )
            .Select(eventPattern => eventPattern.EventArgs);
        }

        /// <summary>
        /// Create an observable sequence of all Property Changing events.
        /// </summary>
        public static IObservable<PropertyChangingEventArgs> OnAnyPropertyChanging<T>(this T source)
            where T : INotifyPropertyChanging
        {
            return Observable.FromEventPattern<PropertyChangingEventHandler, PropertyChangingEventArgs>(
                handler => handler.Invoke,
                h => source.PropertyChanging += h,
                h => source.PropertyChanging -= h
            )
            .Select(eventPattern => eventPattern.EventArgs);
        }

        /// <summary>
        /// Create an observable sequence of all Notify Collection Changed events.
        /// </summary>
        public static IObservable<NotifyCollectionChangedEventArgs> OnAnyCollectionChanges<T>(this T source)
            where T : INotifyCollectionChanged
        {
            return Observable.FromEventPattern<EventHandler<NotifyCollectionChangedEventArgs>, NotifyCollectionChangedEventArgs>(
                handler => handler.Invoke,
                h => source.CollectionChanged += h,
                h => source.CollectionChanged -= h
            )
            .Select(eventPattern => eventPattern.EventArgs);
        }
    }
}
