namespace PiDev
{
    public static class PortingUtils
    {
        // Porting Utils
        public static bool IsDeviceWithTouchscreen()
        {
#if UNITY_EDITOR
            return true; // Force true on Editor
#elif UNITY_IOS || __IOS__ || UNITY_ANDROID || __ANDROID__
        return true;
#elif UNITY_PS4 || __PS4__ || UNITY_PS5 || __PS5__
        return false;
#elif UNITY_GAMECORE || __GAMECORE__
        // GameCore (Xbox platforms) does not have a touchscreen
        return false;
#elif UNITY_SWITCH || __SWITCH__
        TODO: Use nn.hid  [Resolve this compile error!]
        return true if not docked
#else
        // Default to false for unsupported or unknown platforms
        return false;
#endif
        }
    }
}