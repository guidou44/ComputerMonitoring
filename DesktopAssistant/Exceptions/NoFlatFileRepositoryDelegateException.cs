using System;

namespace DesktopAssistant.Exceptions
{
    public class NoFlatFileRepositoryDelegateException : Exception
    {
        public NoFlatFileRepositoryDelegateException(string message) : 
            base(nameof(NoFlatFileRepositoryDelegateException) + "\n" + message)
        {
                
        }
        
    }
}