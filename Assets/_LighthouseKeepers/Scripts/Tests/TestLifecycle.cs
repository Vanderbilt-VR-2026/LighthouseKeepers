using System.Reflection;
using UnityEngine;

namespace LighthouseKeepers.Tests
{
    // EditMode skips MonoBehaviour lifecycle. Invoke managed methods directly;
    // SendMessage to lifecycle names triggers Unity's ShouldRunBehaviour assertion.
    internal static class TestLifecycle
    {
        public static void Invoke(Component component, string method) =>
            component.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(component, null);
    }
}
