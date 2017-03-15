// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Syslog.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Syslog STRUCTURED-DATA part configuration</summary>
    public class StructuredDataConfig : NotifyPropertyChanged, IDisposable
    {
        private Layout fromEventProperties;
        private ObservableCollection<SdElementConfig> sdElements;
        private readonly PropertyChangedEventHandler sdElementPropsChanged;
        private readonly NotifyCollectionChangedEventHandler sdElementsCollectionChanged;

        /// <summary>Allows to use log event properties data enabling different STRUCTURED-DATA for each log message</summary>
        public Layout FromEventProperties
        {
            get { return fromEventProperties; }
            set { SetProperty(ref fromEventProperties, value); }
        }

        /// <summary>The SD-ELEMENTs contained in the STRUCTURED-DATA part</summary>
        [ArrayParameter(typeof(SdElementConfig), "SdElement")]
        public ObservableCollection<SdElementConfig> SdElements
        {
            get { return sdElements; }
            set { SetProperty(ref sdElements, value); }
        }

        /// <summary>Builds a new instance of the StructuredData class</summary>
        public StructuredDataConfig()
        {
            fromEventProperties = string.Empty;
            sdElements = new ObservableCollection<SdElementConfig>();
            sdElementPropsChanged = (s, a) => OnPropertyChanged(nameof(SdElements));
            sdElementsCollectionChanged = CollectionChangedFactory(sdElementPropsChanged);
            sdElements.CollectionChanged += sdElementsCollectionChanged;
        }

        public void Dispose()
        {
            sdElements.ForEach(x =>
            {
                x.PropertyChanged -= sdElementPropsChanged;
                x.Dispose();
            });
            sdElements.CollectionChanged -= sdElementsCollectionChanged;
        }
    }
}