namespace NLog.Targets
{
    internal interface IBasicPolicy<in T, out TResult>
    {
        bool IsApplicable();
        TResult Apply(T item);
    }
}