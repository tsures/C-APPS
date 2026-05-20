using System.Collections.Generic;
using AudioAmplifierMockup.Models;

namespace AudioAmplifierMockup.Services
{
    public interface IAudioDeviceService
    {
        IList<AudioDeviceInfo> GetOutputDevices();

        AudioDeviceInfo GetDefaultOutputDevice();

        void SetDefaultOutputDevice(string deviceId);

        int GetMasterVolume();

        void SetMasterVolume(int volume);
    }
}
