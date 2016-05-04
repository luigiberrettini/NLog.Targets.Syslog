namespace NLog.Targets.Syslog.Policies
{
    internal interface IBasicPolicy<in T, out TResult>
    {
        bool IsApplicable();
        TResult Apply(T item);
    }
}