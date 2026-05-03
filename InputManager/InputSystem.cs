using Assets.Infrastructure.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Infrastructure.InputManager
{
    public enum InputSystemType
    {
        WindowsUser32,      // Все версии Windows с user32.dll
        WindowsWinRT,       // Windows 8/8.1/10/11 с WinRT (опционально) сейчас не участвует
        WindowsGameInput,   // Windows 10/11 с GameInput (опционально)  сейчас не участвует
        LinuxX11,           // Linux с X11 (libX11.so)
        LinuxWayland,       // Linux с Wayland (libwayland-client.so)
        LinuxConsole,       // Linux без графики (termios/evdev)
        MacCocoa,           // macOS 10.7+ (Cocoa)
        MacCarbon,          // macOS 10.4-10.14 (Carbon)
        Unknown
    }

    public class InputSystem : MonoBehaviour
    {
        private Action updateAction;
        private static Action<int, Action, Action, Action> bindAction;
        private static Action<int, Action, Action, Action> unbindAction;
        private static Func<int, bool> getKey;
        private static Func<int[], bool> getKeyDown;
        private static Func<int, bool> getKeyUp;

        private InputSystemType typeInputSystem;

        private void Awake()
        {
            switch (GetInputSystemType())
            {
                case InputSystemType.WindowsUser32:
                    // Все версии Windows (user32.dll)
                    // TODO: Добавить вызовы user32.dll
                    ServiceContainer.RegisterSingleton<InputSystemWindows>();
                    var action0 = ServiceContainer.Get<InputSystemWindows>();

                    updateAction = action0.Update;
                    bindAction = InputSystemWindows.Bind;
                    unbindAction = InputSystemWindows.Unbind;
                    getKey = InputSystemWindows.GetKey;
                    getKeyDown = InputSystemWindows.GetKeyDown;
                    getKeyUp = InputSystemWindows.GetKeyUp;

                    typeInputSystem = InputSystemType.WindowsUser32;

                    var win = new KeyCodeWindows();
                    win.KeyCodeInitialization();
                    break;

                case InputSystemType.LinuxX11:
                    // Linux с X11
                    // TODO: Добавить вызовы libX11.so
                    ServiceContainer.RegisterSingleton<InputSystemX11>();
                    var action1 = ServiceContainer.Get<InputSystemX11>();

                    updateAction = action1.Update;
                    bindAction = InputSystemX11.Bind;
                    unbindAction = InputSystemX11.Unbind;
                    getKey = InputSystemX11.GetKey;
                    getKeyDown = InputSystemX11.GetKeyDown;
                    getKeyUp = InputSystemX11.GetKeyUp;

                    typeInputSystem = InputSystemType.LinuxX11;

                    var libx11 = new KeyCodeLibX11();
                    libx11.KeyCodeInitialization();
                    break;

                case InputSystemType.LinuxWayland:
                    // Linux с Wayland
                    // TODO: Добавить вызовы libwayland-client.so
                    ServiceContainer.RegisterSingleton<InputSystemWayland>();
                    var action2 = ServiceContainer.Get<InputSystemWayland>();

                    updateAction = action2.Update;
                    bindAction = InputSystemWayland.Bind;
                    unbindAction = InputSystemWayland.Unbind;
                    getKey = InputSystemWayland.GetKey;
                    getKeyDown = InputSystemWayland.GetKeyDown;
                    getKeyUp = InputSystemWayland.GetKeyUp;

                    typeInputSystem = InputSystemType.LinuxWayland;

                    var way = new KeyCodeWayland();
                    way.KeyCodeInitialization();
                    break;

                case InputSystemType.LinuxConsole:
                    // Linux без графики
                    // TODO: Добавить вызовы termios
                    break;

                case InputSystemType.MacCocoa:
                    // macOS 10.7+
                    // TODO: Добавить вызовы Cocoa/AppKit
                    break;

                case InputSystemType.MacCarbon:
                    ServiceContainer.RegisterSingleton<InputSystemCarbon>();
                    var action3 = ServiceContainer.Get<InputSystemCarbon>();

                    updateAction = action3.Update;
                    bindAction = InputSystemCarbon.Bind;
                    unbindAction = InputSystemCarbon.Unbind;
                    getKey = InputSystemCarbon.GetKey;
                    getKeyDown = InputSystemCarbon.GetKeyDown;
                    getKeyUp = InputSystemCarbon.GetKeyUp;

                    typeInputSystem = InputSystemType.MacCarbon;

                    var carbon = new KeyCodeCarbon();
                    carbon.KeyCodeInitialization();
                    break;

                case InputSystemType.Unknown:
                    // Неизвестная ОС

                    break;
            }
        }
        private void Update() => updateAction?.Invoke();
        public static void Bind(int keyCode, Action onDown = null, Action onHold = null, Action onUp = null) =>
            bindAction(keyCode, onDown, onHold, onUp);
        public static void Unbind(int keyCode, Action onDown = null, Action onHold = null, Action onUp = null) =>
            unbindAction(keyCode, onDown, onHold, onUp);

        public static bool GetKey(int keyCode) => getKey(keyCode);
        public static bool GetKeyDown(int[] keyCode) => getKeyDown(keyCode);
        public static bool GetKeyUp(int keyCode) => getKeyUp(keyCode);

        public void SubMouse(Action<Vector2> action)
        {
            switch(typeInputSystem)
            {
                case InputSystemType.WindowsUser32:
                    InputSystemWindows.SubscribeMouseMove(action);
                    break;
                case InputSystemType.LinuxX11:
                    InputSystemX11.SubscribeMouseMove(action);
                    break;
                case InputSystemType.LinuxWayland:
                    InputSystemWayland.SubscribeMouseMove(action);
                    break;
                case InputSystemType.MacCarbon:
                    InputSystemCarbon.SubscribeMouseMove(action);
                    break;
            }
        }
        public void UnsubMouse(Action<Vector2> action)
        {
            switch (typeInputSystem)
            {
                case InputSystemType.WindowsUser32:
                    InputSystemWindows.UnsubscribeMouseMove(action);
                    break;
                case InputSystemType.LinuxX11:
                    InputSystemX11.UnsubscribeMouseMove(action);
                    break;
                case InputSystemType.LinuxWayland:
                    InputSystemWayland.UnsubscribeMouseMove(action);
                    break;
                case InputSystemType.MacCarbon:
                    InputSystemCarbon.UnsubscribeMouseMove(action);
                    break;
            }
        }

        public static InputSystemType GetInputSystemType()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Все версии Windows имеют user32.dll
                return InputSystemType.WindowsUser32;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string sessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE")?.ToLower();
                string display = Environment.GetEnvironmentVariable("DISPLAY");
                string waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");

                if (!string.IsNullOrEmpty(waylandDisplay) || sessionType == "wayland")
                    return InputSystemType.LinuxWayland;

                if (!string.IsNullOrEmpty(display) || sessionType == "x11")
                    return InputSystemType.LinuxX11;

                return InputSystemType.LinuxConsole;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var version = Environment.OSVersion.Version;

                // macOS 10.15+ (Catalina и новее) - только Cocoa
                if (version.Major >= 19)
                    return InputSystemType.MacCocoa;

                // macOS 10.4 - 10.14 - есть и Carbon и Cocoa
                if (version.Major >= 10 && version.Major <= 18)
                    return InputSystemType.MacCarbon;

                return InputSystemType.MacCocoa;
            }

            return InputSystemType.Unknown;
        }
    }
    public class InputSystemWindows
    {
        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }

        private static byte[] _current = new byte[256];
        private static byte[] _previous = new byte[256];
        private static POINT _mousePos;
        private static POINT _prevMousePos;

        private static Dictionary<int, Action> _onJustPressed = new Dictionary<int, Action>();
        private static Dictionary<int, Action> _onPressed = new Dictionary<int, Action>();
        private static Dictionary<int, Action> _onJustReleased = new Dictionary<int, Action>();

        public static event Action<int> OnAnyKeyDown;
        public static event Action<int> OnAnyKeyUp;
        public static event Action<Vector2> OnMouseMove;

        public static Vector2 mousePosition { get; private set; }
        public static Vector2 mouseDelta { get; private set; }

        public static void Bind(int keyCode, Action onDown = null, Action onHold = null, Action onUp = null)
        {
            if (onDown != null)
            {
                if (!_onJustPressed.ContainsKey(keyCode)) _onJustPressed[keyCode] = null;
                _onJustPressed[keyCode] += onDown;
            }

            if (onHold != null)
            {
                if (!_onPressed.ContainsKey(keyCode)) _onPressed[keyCode] = null;
                _onPressed[keyCode] += onHold;
            }

            if (onUp != null)
            {
                if (!_onJustReleased.ContainsKey(keyCode)) _onJustReleased[keyCode] = null;
                _onJustReleased[keyCode] += onUp;
            }
        }

        public static void Unbind(int keyCode, Action onDown = null, Action onHold = null, Action onUp = null)
        {
            if (onDown != null && _onJustPressed.ContainsKey(keyCode))
                _onJustPressed[keyCode] -= onDown;

            if (onHold != null && _onPressed.ContainsKey(keyCode))
                _onPressed[keyCode] -= onHold;

            if (onUp != null && _onJustReleased.ContainsKey(keyCode))
                _onJustReleased[keyCode] -= onUp;
        }

        public static bool GetKey(int keyCode) => (_current[keyCode] & 0x80) != 0;
        public static bool GetKeyDown(params int[] keyCodes)
        {
            if (keyCodes == null || keyCodes.Length == 0)
                return false;

            foreach (int keyCode in keyCodes)
            {
                if (!GetKey(keyCode))
                    return false;
            }

            return true;
        }
        public static bool GetKeyUp(int keyCode) => !GetKey(keyCode) && ((_previous[keyCode] & 0x80) != 0);

        public static void SubscribeMouseMove(Action<Vector2> handler) => OnMouseMove += handler;
        public static void UnsubscribeMouseMove(Action<Vector2> handler) => OnMouseMove -= handler;
        

        public void Update()
        {
            _previous = (byte[])_current.Clone();
            _prevMousePos = _mousePos;

            GetKeyboardState(_current);
            GetCursorPos(out _mousePos);

            mousePosition = new Vector2(_mousePos.X, _mousePos.Y);
            mouseDelta = mousePosition - new Vector2(_prevMousePos.X, _prevMousePos.Y);

            if (mouseDelta != Vector2.zero)
                OnMouseMove?.Invoke(mouseDelta);

            for (int key = 0; key < 256; key++)
            {
                bool wasPressed = (_previous[key] & 0x80) != 0;
                bool isPressed = (_current[key] & 0x80) != 0;

                if (!wasPressed && isPressed)
                {
                    OnAnyKeyDown?.Invoke(key);
                    if (_onJustPressed.TryGetValue(key, out var action)) action?.Invoke();
                }
                else if (isPressed && _onPressed.TryGetValue(key, out var hold))
                {
                    hold?.Invoke();
                }
                else if (wasPressed && !isPressed)
                {
                    OnAnyKeyUp?.Invoke(key);
                    if (_onJustReleased.TryGetValue(key, out var action)) action?.Invoke();
                }
            }

        }
    }

    public class InputSystemX11
    {
        private const string LibX11 = "libX11.so";

        [DllImport(LibX11)]
        private static extern IntPtr XOpenDisplay(IntPtr display);

        [DllImport(LibX11)]
        private static extern int XCloseDisplay(IntPtr display);

        [DllImport(LibX11)]
        private static extern int XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child,
            out int rootX, out int rootY, out int winX, out int winY, out uint mask);

        [DllImport(LibX11)]
        private static extern IntPtr XDefaultRootWindow(IntPtr display);

        [DllImport(LibX11)]
        private static extern int XQueryKeymap(IntPtr display, byte[] keys);

        private static IntPtr _display;
        private static IntPtr _rootWindow;
        private static byte[] _current = new byte[32];
        private static byte[] _previous = new byte[32];

        private static int _mouseX;
        private static int _mouseY;
        private static int _prevMouseX;
        private static int _prevMouseY;
        private static uint _mouseMask;

        private static Dictionary<int, Action> _onJustPressed = new Dictionary<int, Action>();
        private static Dictionary<int, Action> _onPressed = new Dictionary<int, Action>();
        private static Dictionary<int, Action> _onJustReleased = new Dictionary<int, Action>();

        public static event Action<int> OnAnyKeyDown;
        public static event Action<int> OnAnyKeyUp;
        public static event Action<Vector2> OnMouseMove;

        public static Vector2 mousePosition { get; private set; }
        public static Vector2 mouseDelta { get; private set; }

        static InputSystemX11()
        {
            _display = XOpenDisplay(IntPtr.Zero);
            if (_display == IntPtr.Zero)
                throw new Exception("Cannot open X display");

            _rootWindow = XDefaultRootWindow(_display);
        }

        public static void Bind(int keyCode, Action onDown = null, Action onHold = null, Action onUp = null)
        {
            if (onDown != null)
            {
                if (!_onJustPressed.ContainsKey(keyCode)) _onJustPressed[keyCode] = null;
                _onJustPressed[keyCode] += onDown;
            }

            if (onHold != null)
            {
                if (!_onPressed.ContainsKey(keyCode)) _onPressed[keyCode] = null;
                _onPressed[keyCode] += onHold;
            }

            if (onUp != null)
            {
                if (!_onJustReleased.ContainsKey(keyCode)) _onJustReleased[keyCode] = null;
                _onJustReleased[keyCode] += onUp;
            }
        }

        public static void Unbind(int keyCode, Action onDown = null, Action onHold = null, Action onUp = null)
        {
            if (onDown != null && _onJustPressed.ContainsKey(keyCode))
                _onJustPressed[keyCode] -= onDown;

            if (onHold != null && _onPressed.ContainsKey(keyCode))
                _onPressed[keyCode] -= onHold;

            if (onUp != null && _onJustReleased.ContainsKey(keyCode))
                _onJustReleased[keyCode] -= onUp;
        }

        public static bool GetKey(int keyCode)
        {
            int byteIndex = keyCode / 8;
            int bitIndex = keyCode % 8;
            if (byteIndex >= _current.Length) return false;
            return (_current[byteIndex] & (1 << bitIndex)) != 0;
        }

        public static bool GetKeyDown(params int[] keyCodes)
        {
            if (keyCodes == null || keyCodes.Length == 0)
                return false;

            foreach (int keyCode in keyCodes)
            {
                if (!GetKey(keyCode))
                    return false;
            }
            return true;
        }

        public static bool GetKeyUp(int keyCode) => !GetKey(keyCode) && GetKeyPrevious(keyCode);

        private static bool GetKeyPrevious(int keyCode)
        {
            int byteIndex = keyCode / 8;
            int bitIndex = keyCode % 8;
            if (byteIndex >= _previous.Length) return false;
            return (_previous[byteIndex] & (1 << bitIndex)) != 0;
        }

        public static void SubscribeMouseMove(Action<Vector2> handler) => OnMouseMove += handler;
        public static void UnsubscribeMouseMove(Action<Vector2> handler) => OnMouseMove -= handler;
        public void Update()
        {
            Array.Copy(_current, _previous, _current.Length);
            _prevMouseX = _mouseX;
            _prevMouseY = _mouseY;

            XQueryKeymap(_display, _current);

            XQueryPointer(_display, _rootWindow, out _, out _,
                out _mouseX, out _mouseY, out _, out _, out _mouseMask);

            mousePosition = new Vector2(_mouseX, _mouseY);
            mouseDelta = mousePosition - new Vector2(_prevMouseX, _prevMouseY);

            if (mouseDelta != Vector2.zero)
                OnMouseMove?.Invoke(mouseDelta);

            int maxKeys = _current.Length * 8;
            for (int key = 0; key < maxKeys; key++)
            {
                bool wasPressed = GetKeyPrevious(key);
                bool isPressed = GetKey(key);

                if (!wasPressed && isPressed)
                {
                    OnAnyKeyDown?.Invoke(key);
                    if (_onJustPressed.TryGetValue(key, out var action)) action?.Invoke();
                }
                else if (isPressed && _onPressed.TryGetValue(key, out var hold))
                {
                    hold?.Invoke();
                }
                else if (wasPressed && !isPressed)
                {
                    OnAnyKeyUp?.Invoke(key);
                    if (_onJustReleased.TryGetValue(key, out var action)) action?.Invoke();
                }
            }
        }

        public static void Cleanup()
        {
            if (_display != IntPtr.Zero)
                XCloseDisplay(_display);
        }
    }

    public class InputSystemWayland
    {
        [DllImport("libwayland-client.so")]
        private static extern int wl_display_roundtrip(IntPtr display);

        private static IntPtr _display;
        private static byte[] _current = new byte[256];
        private static byte[] _previous = new byte[256];

        private static Dictionary<int, Action> _onJustPressed = new Dictionary<int, Action>();
        private static Dictionary<int, Action> _onPressed = new Dictionary<int, Action>();
        private static Dictionary<int, Action> _onJustReleased = new Dictionary<int, Action>();

        public static event Action<int> OnAnyKeyDown;
        public static event Action<int> OnAnyKeyUp;
        public static event Action<Vector2> OnMouseMove;

        public static void Bind(int keyCode, Action onDown = null, Action onHold = null, Action onUp = null)
        {
            if (onDown != null)
            {
                if (!_onJustPressed.ContainsKey(keyCode)) _onJustPressed[keyCode] = null;
                _onJustPressed[keyCode] += onDown;
            }

            if (onHold != null)
            {
                if (!_onPressed.ContainsKey(keyCode)) _onPressed[keyCode] = null;
                _onPressed[keyCode] += onHold;
            }

            if (onUp != null)
            {
                if (!_onJustReleased.ContainsKey(keyCode)) _onJustReleased[keyCode] = null;
                _onJustReleased[keyCode] += onUp;
            }
        }

        public static void Unbind(int keyCode, Action onDown = null, Action onHold = null, Action onUp = null)
        {
            if (onDown != null && _onJustPressed.ContainsKey(keyCode))
                _onJustPressed[keyCode] -= onDown;

            if (onHold != null && _onPressed.ContainsKey(keyCode))
                _onPressed[keyCode] -= onHold;

            if (onUp != null && _onJustReleased.ContainsKey(keyCode))
                _onJustReleased[keyCode] -= onUp;
        }

        public static bool GetKey(int keyCode) => (_current[keyCode] & 0x80) != 0;
        public static bool GetKeyDown(params int[] keyCodes)
        {
            if (keyCodes == null || keyCodes.Length == 0)
                return false;

            foreach (int keyCode in keyCodes)
            {
                if (!GetKey(keyCode))
                    return false;
            }
            return true;
        }
        public static bool GetKeyUp(int keyCode) => !GetKey(keyCode) && ((_previous[keyCode] & 0x80) != 0);

        public static void SubscribeMouseMove(Action<Vector2> handler) => OnMouseMove += handler;
        public static void UnsubscribeMouseMove(Action<Vector2> handler) => OnMouseMove -= handler;

        public void Update()
        {
            _previous = (byte[])_current.Clone();

            wl_display_roundtrip(_display);

            for (int key = 0; key < 256; key++)
            {
                bool wasPressed = (_previous[key] & 0x80) != 0;
                bool isPressed = (_current[key] & 0x80) != 0;

                if (!wasPressed && isPressed)
                {
                    OnAnyKeyDown?.Invoke(key);
                    if (_onJustPressed.TryGetValue(key, out var action)) action?.Invoke();
                }
                else if (isPressed && _onPressed.TryGetValue(key, out var hold))
                {
                    hold?.Invoke();
                }
                else if (wasPressed && !isPressed)
                {
                    OnAnyKeyUp?.Invoke(key);
                    if (_onJustReleased.TryGetValue(key, out var action)) action?.Invoke();
                }
            }
        }
    }

    public class InputSystemCarbon
    {
        [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
        private static extern void GetKeys(byte[] keyMap);

        [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
        private static extern void GetGlobalMouse(ref CGPoint lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct CGPoint { public double X; public double Y; }

        private static byte[] _current = new byte[256];
        private static byte[] _previous = new byte[256];
        private static CGPoint _mousePos;
        private static CGPoint _prevMousePos;

        private static Dictionary<int, Action> _onJustPressed = new Dictionary<int, Action>();
        private static Dictionary<int, Action> _onPressed = new Dictionary<int, Action>();
        private static Dictionary<int, Action> _onJustReleased = new Dictionary<int, Action>();

        public static event Action<int> OnAnyKeyDown;
        public static event Action<int> OnAnyKeyUp;
        public static event Action<Vector2> OnMouseMove;

        public static Vector2 mousePosition { get; private set; }
        public static Vector2 mouseDelta { get; private set; }

        public static void Bind(int keyCode, Action onDown = null, Action onHold = null, Action onUp = null)
        {
            if (onDown != null)
            {
                if (!_onJustPressed.ContainsKey(keyCode)) _onJustPressed[keyCode] = null;
                _onJustPressed[keyCode] += onDown;
            }

            if (onHold != null)
            {
                if (!_onPressed.ContainsKey(keyCode)) _onPressed[keyCode] = null;
                _onPressed[keyCode] += onHold;
            }

            if (onUp != null)
            {
                if (!_onJustReleased.ContainsKey(keyCode)) _onJustReleased[keyCode] = null;
                _onJustReleased[keyCode] += onUp;
            }
        }

        public static void Unbind(int keyCode, Action onDown = null, Action onHold = null, Action onUp = null)
        {
            if (onDown != null && _onJustPressed.ContainsKey(keyCode))
                _onJustPressed[keyCode] -= onDown;

            if (onHold != null && _onPressed.ContainsKey(keyCode))
                _onPressed[keyCode] -= onHold;

            if (onUp != null && _onJustReleased.ContainsKey(keyCode))
                _onJustReleased[keyCode] -= onUp;
        }

        public static bool GetKey(int keyCode) => (_current[keyCode] & 0x80) != 0;

        public static bool GetKeyDown(params int[] keyCodes)
        {
            if (keyCodes == null || keyCodes.Length == 0)
                return false;

            foreach (int keyCode in keyCodes)
            {
                if (!GetKey(keyCode))
                    return false;
            }

            return true;
        }

        public static bool GetKeyUp(int keyCode) => !GetKey(keyCode) && ((_previous[keyCode] & 0x80) != 0);

        public static void SubscribeMouseMove(Action<Vector2> handler) => OnMouseMove += handler;
        public static void UnsubscribeMouseMove(Action<Vector2> handler) => OnMouseMove -= handler;
        
        public void Update()
        {
            _previous = (byte[])_current.Clone();
            _prevMousePos = _mousePos;

            byte[] keyMap = new byte[16];
            GetKeys(keyMap);

            for (int i = 0; i < 128; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                bool isPressed = (keyMap[byteIndex] & (1 << bitIndex)) != 0;
                _current[i] = isPressed ? (byte)0x80 : (byte)0;
            }

            GetGlobalMouse(ref _mousePos);

            mousePosition = new Vector2((float)_mousePos.X, (float)_mousePos.Y);
            mouseDelta = mousePosition - new Vector2((float)_prevMousePos.X, (float)_prevMousePos.Y);

            if (mouseDelta != Vector2.zero)
                OnMouseMove?.Invoke(mouseDelta);

            for (int key = 0; key < 256; key++)
            {
                bool wasPressed = (_previous[key] & 0x80) != 0;
                bool isPressed = (_current[key] & 0x80) != 0;

                if (!wasPressed && isPressed)
                {
                    OnAnyKeyDown?.Invoke(key);
                    if (_onJustPressed.TryGetValue(key, out var action)) action?.Invoke();
                }
                else if (isPressed && _onPressed.TryGetValue(key, out var hold))
                {
                    hold?.Invoke();
                }
                else if (wasPressed && !isPressed)
                {
                    OnAnyKeyUp?.Invoke(key);
                    if (_onJustReleased.TryGetValue(key, out var action)) action?.Invoke();
                }
            }
        }
    }
}