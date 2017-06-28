using System;

namespace Models.Domain
{
    public interface IBaseModel
    {
        long Id { get; set; }
        DateTime CreatedOn { get; set; }
        string CreatedBy { get; set; } // ApplicationUserId
        DateTime? ModifiedOn { get; set; }
        string ModifiedBy { get; set; } // ApplicationUser.Id
        DateTime? ArchivedOn { get; set; }
        string ArchivedBy { get; set; } // ApplicationUser.Id
    }
}
