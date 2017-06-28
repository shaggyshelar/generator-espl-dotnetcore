using System;

namespace Models.Domain
{
    public abstract class BaseModel : IBaseModel
    {
        public BaseModel()
        {
            // Set property defaults here
            CreatedOn = DateTime.Now;
        }

        public long Id { get; set; }

        // Audit properties
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } // ApplicationUser.Id
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; } // ApplicationUser.Id
        public DateTime? ArchivedOn { get; set; }
        public string ArchivedBy { get; set; } // ApplicationUser.Id

        public bool IsNew
        {
            get { return Id == default(long); }
        }
    }
}
