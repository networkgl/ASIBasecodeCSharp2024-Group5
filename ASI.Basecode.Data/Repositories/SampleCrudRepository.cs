using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class SampleCrudRepository : ISampleCrudRepository
    {
        private readonly List<SampleCrud> _data = new List<SampleCrud>();
        private int _nextId = 1;

        public IEnumerable<SampleCrud> RetrieveAll()
        {
            return _data;
        }

        public void Add(SampleCrud model)
        {
            model.Id = _nextId++;
            _data.Add(model);
        }

        public void Update(SampleCrud model)
        { 
            var existingData = _data.Where(x => x.Id == model.Id).FirstOrDefault();
            if (existingData != null)
            {
                existingData = model;
            }
        }

        public void Delete(int id)
        {
            var existingData = _data.Where(x => x.Id == id).FirstOrDefault();
            if(existingData != null)
            {
                _data.Remove(existingData);
            }
        }
    }
}
