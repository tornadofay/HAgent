using System;

namespace HAgent.WinForms.UI
{
    /// <summary>
    /// Coarse-grained policy for automatic WinForms UI behavior. This is a convenience
    /// policy, not a security boundary; hosts may supply their own authorization logic.
    /// </summary>
    public sealed class UiAutomationPermissions
    {
        public bool AutomaticDiscovery { get; set; }
        public bool ReadControls { get; set; }
        public bool ReadData { get; set; }
        public bool WriteControls { get; set; }
        public bool InvokeControls { get; set; }

        public UiAutomationPermissions()
        {
            AutomaticDiscovery = false;
            ReadControls = true;
            ReadData = true;
            WriteControls = false;
            InvokeControls = false;
        }

        public UiAutomationPermissions Clone()
        {
            return new UiAutomationPermissions
            {
                AutomaticDiscovery = AutomaticDiscovery,
                ReadControls = ReadControls,
                ReadData = ReadData,
                WriteControls = WriteControls,
                InvokeControls = InvokeControls
            };
        }

        public void Validate()
        {
            if (!ReadControls && ReadData)
                throw new InvalidOperationException("ReadData permission requires ReadControls permission for automatic UI authorization.");
        }
    }
}
