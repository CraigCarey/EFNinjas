﻿using NinjaDomain.Classes.Interfaces;
using System;
using System.Collections.Generic;

namespace NinjaDomain.Classes
{
    public class Ninja : IModificationHistory
    {
        public Ninja()
        {
            EquipmentOwned = new List<NinjaEquipment>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool ServedInOniwaban { get; set; }
        public Clan Clan { get; set; }
        public int ClanId { get; set; }
        // make virtual to enable lazy loading
        //public virtual List<NinjaEquipment> EquipmentOwned { get; set; }
        public List<NinjaEquipment> EquipmentOwned { get; set; }
        public DateTime DateOfBirth { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsDirty { get; set; }
    }
}
