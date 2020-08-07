using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using Common.Helpers;
using Common.Reports;
using DesktopAssistant.BL.Persistence;
using DesktopAssistant.Configuration;
using DesktopAssistant.Exceptions;
using DesktopAssistant.UI;
using Hardware.Models;
using ProcessMonitoring.Models;

namespace DesktopAssistant.Repository
{
    public class FlatFileRepository : IRepository
    {
        private static readonly XmlHelper XmlHelper = new XmlHelper();
        private static readonly Reporter Reporter = new Reporter();

        
        private static readonly IEnumerable<ITuple> AccessDelegates = new HashSet<ITuple>()
        {
            ValueTuple.Create(typeof(ResourceCollection), XmlHelper, ConfigurationManager.AppSettings["hardwareConfiguration"]),
            ValueTuple.Create(typeof(WatchdogInitialization), XmlHelper, ConfigurationManager.AppSettings["processWatchConfiguration"]),
            ValueTuple.Create(typeof(Exception), Reporter, ConfigurationManager.AppSettings["reporterDirectory"]),
            ValueTuple.Create(typeof(UserInterfaceConfiguration), XmlHelper, ConfigurationManager.AppSettings["userInterfaceConfiguration"])
        };

        public TObject Read<TObject>()
        {
            object accessor = SelectAccessDelegate(typeof(TObject));
            string configPath = SelectConfigPath(typeof(TObject));
            
            if (configPath is null)
                throw new ArgumentException($"Could not load config path for type : {typeof(TObject)}");
            
            if (accessor is XmlHelper helper)
                return helper.Deserialize<TObject>(configPath);
            throw new NoFlatFileRepositoryDelegateException($"Found invalid type for delegate access : {accessor.GetType().Name}");
        }

        public void Update<TObject>(TObject updatedEntity)
        {
            object accessor = SelectAccessDelegate(typeof(TObject));
            string configPath = SelectConfigPath(typeof(TObject));
            
            if (configPath is null)
                throw new ArgumentException($"Could not load config path for type : {typeof(TObject)}");
            
            if (accessor is Reporter reporter && updatedEntity is Exception exception)
                reporter.LogException(exception, configPath);
            else if (accessor is XmlHelper xmlHelper)
                xmlHelper.SerializeOverwrite(updatedEntity, configPath);
        }
        
        public static object SelectAccessDelegate(Type targetType)
        {
            bool isValidType = AccessDelegates.Any(ad => (Type) ad[0] == targetType);
            if (!isValidType)
                throw new NoFlatFileRepositoryDelegateException(targetType.Name);
            object accessor = AccessDelegates.SingleOrDefault(ad => (Type) ad[0] == targetType)?[1];
            return accessor;
        }

        private static string SelectConfigPath(Type targetType)
        {
            bool isValidType = AccessDelegates.Any(ad => (Type) ad[0] == targetType);
            if (!isValidType)
                throw new NoFlatFileRepositoryDelegateException(targetType.Name);
            return  AccessDelegates.SingleOrDefault(ad => (Type) ad[0] == targetType)?[2] as string;
        }
        
    }
}