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

        public static PartRepository Instance
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

            p.Id = $"{_parts.Count + 1}";
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
            return _parts.TakeWhile(p => p.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        public List<Part> GetAllParts() => _parts;
    }
}
