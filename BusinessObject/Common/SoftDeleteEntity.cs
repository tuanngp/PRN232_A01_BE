using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Common
{
    public abstract class SoftDeleteEntity : AuditableEntity, ISoftDelete
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedById { get; set; }

        [ForeignKey("DeletedById")]
        public virtual SystemAccount? DeletedBy { get; set; }
    }
}