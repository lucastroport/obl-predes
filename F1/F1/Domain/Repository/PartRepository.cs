using F1.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1.Domain.Repository
{
    public sealed class PartRepository : IPartRepository
    {
        private static PartRepository _instance;
        private List<Part> _parts;

        private PartRepository()
        {
            _parts = new List<Part>();
        }

        public static IPartRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PartRepository();
                }
                return _instance;
            }
        }

        public void AddPart(Part p)
        {
            if (_parts.Contains(p))
            {
                throw new ArgumentException($"Cannot add Part with duplicated id: {p.Id}");
            }

            int lastAssignedId = _parts.Select(part => int.Parse(part.Id)).DefaultIfEmpty(0).Max();
            
            int newId = lastAssignedId + 1;
            p.Id = newId.ToString();

            _parts.Add(p);
        }

        public void RemovePart(Part p)
        {
            _parts.Remove(p);
        }

        public Part? QueryById(string id)
        {
            return _parts.FirstOrDefault(p => p.Id == id);
        }

        public List<Part> QueryItemsByName(string name)
        {
            return _parts.FindAll(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        public List<Part> GetAllParts() => _parts;
    }
}
