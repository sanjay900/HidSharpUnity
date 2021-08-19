using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using HidSharp;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
namespace HidSharpUnity
{
    public class RockbandCalibration : MonoBehaviour
    {
        private Byte[] activate_mic_wii = { 0x00, 0xE9, 0x00, 0x83, 0x1B, 0x00, 0x00, 0x00, 0x02 };
        private Byte[] activate_light_wii = { 0x00, 0xE9, 0x00, 0x83, 0x1B, 0x00, 0x00, 0x00, 0x01 };
        private Byte[] disable_mic_light_wii = { 0x00, 0xE9, 0x00, 0x83, 0x1B, 0x00, 0x00, 0x00, 0x00 };
        private Byte[][] other_commands_wii = new Byte[4][] {
            new Byte[9] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
            new Byte[9]  {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00},
            new Byte[9] {0x00, 0x00, 0x00, 0x83, 0x00, 0x00, 0x00, 0x00, 0x00},
            new Byte[9]  {0x00, 0xE9, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}
        };
        private Byte[] activate_mic_ps3 = { 0x00, 0x01, 0x08, 0x01, 0x40, 0x00, 0x00, 0x00, 0x00 };
        private Byte[] activate_light_ps3 = { 0x00, 0x01, 0x08, 0x01, 0xFF, 0x00, 0x00, 0x00, 0x00 };
        private Byte[] disable_mic_light_ps3 = { 0x00, 0x01, 0x08, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00 };
        private readonly int PS3_ROCK_BAND_GUITAR_VID = 0x12ba;
        private readonly int PS3_ROCK_BAND_GUITAR_PID = 0x0200;
        private readonly int WII_ROCK_BAND_GUITAR_VID = 0x1bad;
        private readonly int WII_ROCK_BAND_GUITAR_PID = 0x0004;
        private readonly int ARDWIINO_VID = 0x1209;
        private readonly int ARDWIINO_PID = 0x2882;
        private IEnumerator<WaitForSeconds> sendPacket(RewiredSharpJoystick device, Byte[] packet)
        {
            var stream = device.hidSharpDevice.Open();
            stream.SetFeature(packet);
            yield return new WaitForSeconds(1.473f);
            foreach (Byte[] cmd in other_commands_wii)
            {
                stream.SetFeature(cmd);
                yield return new WaitForSeconds(1.473f);
            }
            Byte[] data = new Byte[0x001C];
            stream.GetFeature(data);
            yield return new WaitForSeconds(1.473f);
            stream.Close();
        }

        public void enableMicSensor(RewiredSharpJoystick device) {
            if (device.hidSharpDevice.VendorID == PS3_ROCK_BAND_GUITAR_VID && device.hidSharpDevice.ProductID == PS3_ROCK_BAND_GUITAR_PID) {
                enableMicPs3(device);
            } else if (device.hidSharpDevice.VendorID == WII_ROCK_BAND_GUITAR_VID && device.hidSharpDevice.ProductID == WII_ROCK_BAND_GUITAR_PID) {
                enableMicWii(device);
            } else if (device.hidSharpDevice.VendorID == ARDWIINO_VID && device.hidSharpDevice.ProductID == ARDWIINO_PID) {
                enableLightX360(device);
            }
        }

        public void enableLightSensor(RewiredSharpJoystick device) {
            if (device.hidSharpDevice.VendorID == PS3_ROCK_BAND_GUITAR_VID && device.hidSharpDevice.ProductID == PS3_ROCK_BAND_GUITAR_PID) {
                enableLightPs3(device);
            } else if (device.hidSharpDevice.VendorID == WII_ROCK_BAND_GUITAR_VID && device.hidSharpDevice.ProductID == WII_ROCK_BAND_GUITAR_PID) {
                enableLightWii(device);
            } else if (device.hidSharpDevice.VendorID == ARDWIINO_VID && device.hidSharpDevice.ProductID == ARDWIINO_PID) {
                enableLightX360(device);
            }
        }

        public void disableSensors(RewiredSharpJoystick device) {
            if (device.hidSharpDevice.VendorID == PS3_ROCK_BAND_GUITAR_VID && device.hidSharpDevice.ProductID == PS3_ROCK_BAND_GUITAR_PID) {
                disableSensorsPs3(device);
            } else if (device.hidSharpDevice.VendorID == WII_ROCK_BAND_GUITAR_VID && device.hidSharpDevice.ProductID == WII_ROCK_BAND_GUITAR_PID) {
                disableSensorsWii(device);
            } else if (device.hidSharpDevice.VendorID == ARDWIINO_VID && device.hidSharpDevice.ProductID == ARDWIINO_PID) {
                disableSensorsX360(device);
            }
        }

        private void enableMicWii(RewiredSharpJoystick device)
        {
            StartCoroutine(sendPacket(device, activate_mic_wii));
        }
        private void enableLightWii(RewiredSharpJoystick device)
        {
            StartCoroutine(sendPacket(device, activate_light_wii));
        }
        private void disableSensorsWii(RewiredSharpJoystick device)
        {
            StartCoroutine(sendPacket(device, disable_mic_light_wii));
        }

        private void enableMicPs3(RewiredSharpJoystick device)
        {
            var stream = device.hidSharpDevice.Open();
            stream.Write(activate_mic_ps3);
            stream.Close();
        }

        private void enableLightPs3(RewiredSharpJoystick device)
        {
            var stream = device.hidSharpDevice.Open();
            stream.Write(activate_light_ps3);
            stream.Close();
        }

        private void disableSensorsPs3(RewiredSharpJoystick device)
        {
            var stream = device.hidSharpDevice.Open();
            stream.Write(disable_mic_light_ps3);
            stream.Close();
        }

        private void enableMicX360(RewiredSharpJoystick device)
        {
            // I suspect this will be done via something like rumble, and won't use hid feature reports.
        }
        private void enableLightX360(RewiredSharpJoystick device)
        {
        }
        private void disableSensorsX360(RewiredSharpJoystick device)
        {
        }
    }
}