using UnityEngine.UIElements;

namespace EditorAttributes.Editor
{
    internal static class UIElementsCompatibilityExtensions
    {
        public static void RegisterCallbackOnce<TEventType>(this CallbackEventHandler handler, EventCallback<TEventType> callback, TrickleDown trickleDown = TrickleDown.NoTrickleDown)
            where TEventType : EventBase<TEventType>, new()
        {
            EventCallback<TEventType> wrapper = null;
            wrapper = evt =>
            {
                handler.UnregisterCallback(wrapper, trickleDown);
                callback?.Invoke(evt);
            };

            handler.RegisterCallback(wrapper, trickleDown);
        }
    }
}
