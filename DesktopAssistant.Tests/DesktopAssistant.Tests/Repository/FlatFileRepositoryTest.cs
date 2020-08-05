using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Common.Reports;
using DesktopAssistant.BL.Persistence;
using DesktopAssistant.Repository;
using DesktopAssistant.Tests.DesktopAssistant.BL.Tests.Persistence;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.Tests.Repository
{
    public class FlatFileRepositoryTest : IRepositoryTest
    {
        protected override IRepository GivenRepository()
        {
            return new FlatFileRepository();
        }

        protected override void AssertEntityUpdated<TObject>(TObject entity)
        {
            object accessor = FlatFileRepository.SelectAccessDelegate(entity.GetType());

            if (accessor is Reporter && entity is Exception ex)
                AssertEntityUpdatedException(ex);
        }

        private void AssertEntityUpdatedException(Exception ex)
        {
            string directory = ConfigurationManager.AppSettings["reporterDirectory"];
            string file = $@".\{directory}\{directory}.txt";
            
            Assert.True(File.Exists(file));
            
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
            }

            string lastRelevant = lines.LastOrDefault(l => l.Contains("Message:"));

            Assert.Contains(ex.Message, lastRelevant);
        }
        
    }
}