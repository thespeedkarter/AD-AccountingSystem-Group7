namespace AccountingSystem.Models
{
    public enum EventAction
    {
        // CRUD-ish
        Created = 1,
        Updated = 2,
        Activated = 3,
        Deactivated = 4,

        // aliases 
        Create = Created,
        Update = Updated,
        Activate = Activated,
        Deactivate = Deactivated,

        Deleted = 5,
        Delete = Deleted,

        // Workflow (journal/access/etc.)
        Approved = 10,
        Rejected = 11,
        Posted = 20,

        // aliases to match 
        Approve = Approved,
        Reject = Rejected,
        Post = Posted,

        // Files
        Uploaded = 30,
        Upload = Uploaded
    }
}