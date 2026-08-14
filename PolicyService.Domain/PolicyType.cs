namespace PolicyService.Domain
{
    public enum PolicyType
    {
        Health = 1,
        Auto = 2,
        Home = 3,
        Life = 4
    }

    public enum PolicyStatus
    {
        Active = 1,
        Expired = 2,
        Cancelled = 3
    }
}
