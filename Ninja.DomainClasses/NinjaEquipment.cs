using NinjaDomain.Classes.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace NinjaDomain.Classes
{
    public class NinjaEquipment : IModificationHistory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public EquipmentType Type { get; set; }

        [Required]  // without this, EF gives NinjaEquipment a many to 0 or 1 relationship with Ninja
        public Ninja Ninja { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsDirty { get; set; }
    }
}
