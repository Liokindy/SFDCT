using SFDCT.Helper;
using System;

namespace SFDCT.UI.Panels;

internal static class EventHelper
{
    internal static void Add<T>(T instance, string eventName, Delegate function)
    {
        var instanceType = typeof(T);
        var fromEvent = instanceType.GetEvent(eventName);

        try
        {
            fromEvent.AddEventHandler(instance, function);
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception trying to hook an event!");
            Logger.LogError(ex.ToString());
        }
    }
}
