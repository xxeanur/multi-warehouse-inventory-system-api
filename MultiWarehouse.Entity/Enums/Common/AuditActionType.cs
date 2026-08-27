namespace MultiWarehouse.Entity.Enums.Common
{
    public enum AuditActionType
    {
        Create,
        Update,
        Delete,
        Login,
        Logout,
        PasswordChanged,
        EmailChangeRequested,
        EmailChanged,
        SessionRevoked,
        AllOtherSessionsRevoked,

        DocumentCreated,
        DocumentApproved,
        DocumentCancelled,
        DocumentCompleted
    }
}