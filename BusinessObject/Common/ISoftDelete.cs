namespace BusinessObject.Common
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
        int? DeletedById { get; set; }
        SystemAccount? DeletedBy { get; set; }
    }
} 