using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AudioAmplifierMockup.Models;

namespace AudioAmplifierMockup.Services
{
    public class AudioDeviceService : IAudioDeviceService
    {
        public IList<AudioDeviceInfo> GetOutputDevices()
        {
            IMMDeviceEnumerator enumerator = CreateEnumerator();

            IMMDeviceCollection collection;
            enumerator.EnumAudioEndpoints(
                EDataFlow.eRender,
                DEVICE_STATE.ACTIVE,
                out collection);

            uint count;
            collection.GetCount(out count);

            List<AudioDeviceInfo> devices = new List<AudioDeviceInfo>();

            for (uint i = 0; i < count; i++)
            {
                IMMDevice device;
                collection.Item(i, out device);

                string id;
                device.GetId(out id);

                devices.Add(new AudioDeviceInfo
                {
                    Id = id,
                    Name = GetDeviceName(device)
                });

                Marshal.ReleaseComObject(device);
            }

            Marshal.ReleaseComObject(collection);
            Marshal.ReleaseComObject(enumerator);

            return devices;
        }

        public AudioDeviceInfo GetDefaultOutputDevice()
        {
            IMMDeviceEnumerator enumerator = CreateEnumerator();

            IMMDevice device;
            enumerator.GetDefaultAudioEndpoint(
                EDataFlow.eRender,
                ERole.eMultimedia,
                out device);

            string id;
            device.GetId(out id);

            AudioDeviceInfo result = new AudioDeviceInfo
            {
                Id = id,
                Name = GetDeviceName(device)
            };

            Marshal.ReleaseComObject(device);
            Marshal.ReleaseComObject(enumerator);

            return result;
        }

        public void SetDefaultOutputDevice(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return;

            IPolicyConfig policyConfig = new PolicyConfigClient() as IPolicyConfig;

            if (policyConfig == null)
                throw new InvalidOperationException("Could not create Windows audio policy config.");

            policyConfig.SetDefaultEndpoint(deviceId, ERole.eConsole);
            policyConfig.SetDefaultEndpoint(deviceId, ERole.eMultimedia);
            policyConfig.SetDefaultEndpoint(deviceId, ERole.eCommunications);
        }

        public int GetMasterVolume()
        {
            IAudioEndpointVolume endpointVolume = GetDefaultEndpointVolume();

            float level;
            endpointVolume.GetMasterVolumeLevelScalar(out level);

            Marshal.ReleaseComObject(endpointVolume);

            return ClampToVolumeRange((int)Math.Round(level * 100));
        }

        public void SetMasterVolume(int volume)
        {
            int safeVolume = ClampToVolumeRange(volume);

            IAudioEndpointVolume endpointVolume = GetDefaultEndpointVolume();

            endpointVolume.SetMasterVolumeLevelScalar(safeVolume / 100f, Guid.Empty);

            Marshal.ReleaseComObject(endpointVolume);
        }

        private static int ClampToVolumeRange(int volume)
        {
            if (volume < 1)
                return 1;

            if (volume > 100)
                return 100;

            return volume;
        }

        private static IMMDeviceEnumerator CreateEnumerator()
        {
            return (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
        }

        private static IAudioEndpointVolume GetDefaultEndpointVolume()
        {
            IMMDeviceEnumerator enumerator = CreateEnumerator();

            IMMDevice device;
            enumerator.GetDefaultAudioEndpoint(
                EDataFlow.eRender,
                ERole.eMultimedia,
                out device);

            Guid iid = typeof(IAudioEndpointVolume).GUID;

            object endpointVolumeObject;
            device.Activate(ref iid, CLSCTX.ALL, IntPtr.Zero, out endpointVolumeObject);

            Marshal.ReleaseComObject(device);
            Marshal.ReleaseComObject(enumerator);

            return (IAudioEndpointVolume)endpointVolumeObject;
        }

        private static string GetDeviceName(IMMDevice device)
        {
            IPropertyStore propertyStore;
            device.OpenPropertyStore(STGM.READ, out propertyStore);

            PROPVARIANT propertyValue;
            PROPERTYKEY key = PropertyKeys.PKEY_Device_FriendlyName;

            propertyStore.GetValue(ref key, out propertyValue);

            string name = propertyValue.GetValue();

            PropVariantClear(ref propertyValue);
            Marshal.ReleaseComObject(propertyStore);

            return string.IsNullOrWhiteSpace(name) ? "Unknown Audio Device" : name;
        }

        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PROPVARIANT pvar);
    }

    internal enum EDataFlow
    {
        eRender = 0,
        eCapture = 1,
        eAll = 2
    }

    internal enum ERole
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2
    }

    [Flags]
    internal enum DEVICE_STATE : uint
    {
        ACTIVE = 0x00000001
    }

    internal enum STGM
    {
        READ = 0x00000000
    }

    [Flags]
    internal enum CLSCTX
    {
        INPROC_SERVER = 0x1,
        INPROC_HANDLER = 0x2,
        LOCAL_SERVER = 0x4,
        REMOTE_SERVER = 0x10,
        ALL = INPROC_SERVER | INPROC_HANDLER | LOCAL_SERVER | REMOTE_SERVER
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
    }

    internal static class PropertyKeys
    {
        public static readonly PROPERTYKEY PKEY_Device_FriendlyName =
            new PROPERTYKEY
            {
                fmtid = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"),
                pid = 14
            };
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROPVARIANT
    {
        public ushort vt;
        public ushort wReserved1;
        public ushort wReserved2;
        public ushort wReserved3;
        public IntPtr p;
        public int p2;

        public string GetValue()
        {
            if (vt == 31)
                return Marshal.PtrToStringUni(p);

            return null;
        }
    }

    [ComImport]
    [Guid("bcde0395-e52f-467c-8e3d-c4579291692e")]
    internal class MMDeviceEnumeratorComObject
    {
    }

    [ComImport]
    [Guid("a95664d2-9614-4f35-a746-de8db63617e6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        void EnumAudioEndpoints(
            EDataFlow dataFlow,
            DEVICE_STATE stateMask,
            out IMMDeviceCollection devices);

        void GetDefaultAudioEndpoint(
            EDataFlow dataFlow,
            ERole role,
            out IMMDevice endpoint);

        void GetDevice(
            [MarshalAs(UnmanagedType.LPWStr)] string id,
            out IMMDevice device);

        void RegisterEndpointNotificationCallback(IntPtr client);

        void UnregisterEndpointNotificationCallback(IntPtr client);
    }

    [ComImport]
    [Guid("0bd7a1be-7a1a-44db-8397-cc5392387b5e")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceCollection
    {
        void GetCount(out uint count);

        void Item(uint index, out IMMDevice device);
    }

    [ComImport]
    [Guid("d666063f-1587-4e43-81f1-b948e807363f")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        void Activate(
            ref Guid iid,
            CLSCTX clsCtx,
            IntPtr activationParams,
            [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);

        void OpenPropertyStore(
            STGM access,
            out IPropertyStore properties);

        void GetId(
            [MarshalAs(UnmanagedType.LPWStr)] out string id);

        void GetState(out DEVICE_STATE state);
    }

    [ComImport]
    [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        void GetCount(out uint propertyCount);

        void GetAt(uint propertyIndex, out PROPERTYKEY key);

        void GetValue(ref PROPERTYKEY key, out PROPVARIANT value);

        void SetValue(ref PROPERTYKEY key, ref PROPVARIANT value);

        void Commit();
    }

    [ComImport]
    [Guid("5cdf2c82-841e-4546-9722-0cf74078229a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        void RegisterControlChangeNotify(IntPtr notify);
        void UnregisterControlChangeNotify(IntPtr notify);
        void GetChannelCount(out uint channelCount);
        void SetMasterVolumeLevel(float level, Guid eventContext);
        void SetMasterVolumeLevelScalar(float level, Guid eventContext);
        void GetMasterVolumeLevel(out float level);
        void GetMasterVolumeLevelScalar(out float level);
        void SetChannelVolumeLevel(uint channelNumber, float level, Guid eventContext);
        void SetChannelVolumeLevelScalar(uint channelNumber, float level, Guid eventContext);
        void GetChannelVolumeLevel(uint channelNumber, out float level);
        void GetChannelVolumeLevelScalar(uint channelNumber, out float level);
        void SetMute(bool isMuted, Guid eventContext);
        void GetMute(out bool isMuted);
        void GetVolumeStepInfo(out uint step, out uint stepCount);
        void VolumeStepUp(Guid eventContext);
        void VolumeStepDown(Guid eventContext);
        void QueryHardwareSupport(out uint hardwareSupportMask);
        void GetVolumeRange(out float minVolumeDb, out float maxVolumeDb, out float volumeIncrementDb);
    }

    [ComImport]
    [Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
    internal class PolicyConfigClient
    {
    }

    [ComImport]
    [Guid("f8679f50-850a-41cf-9c72-430f290290c8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPolicyConfig
    {
        void GetMixFormat();
        void GetDeviceFormat();
        void ResetDeviceFormat();
        void SetDeviceFormat();
        void GetProcessingPeriod();
        void SetProcessingPeriod();
        void GetShareMode();
        void SetShareMode();
        void GetPropertyValue();
        void SetPropertyValue();
        void SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string deviceId, ERole role);
        void SetEndpointVisibility();
    }
}