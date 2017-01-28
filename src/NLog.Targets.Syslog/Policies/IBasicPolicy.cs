// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Policies
{
    internal interface IBasicPolicy<in T, out TResult>
    {
        bool IsApplicable();

        TResult Apply(T item);
    }
}