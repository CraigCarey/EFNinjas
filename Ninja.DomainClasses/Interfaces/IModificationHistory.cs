using System;

namespace NinjaDomain.Classes.Interfaces
{
    public interface IModificationHistory
    {
        // used for logging changes to data
        DateTime DateModified { get; set; }
        DateTime DateCreated { get; set; }

        // used on client side only, not persisted into db
        bool IsDirty { get; set; }
    }
}
