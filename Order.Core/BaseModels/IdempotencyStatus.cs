namespace Order.Core.BaseModels;

public enum IdempotencyStatus : short
{
    InProgress = 0,
    Completed = 1,
    Failed = 2
}