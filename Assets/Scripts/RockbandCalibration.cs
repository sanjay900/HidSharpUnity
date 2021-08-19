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
        Byte[] activate_mic = { 0x00, 0xE9, 0x00, 0x83, 0x1B, 0x00, 0x00, 0x00, 0x02 };
        Byte[] activate_light = { 0x00, 0xE9, 0x00, 0x83, 0x1B, 0x00, 0x00, 0x00, 0x01 };
        Byte[] disable_mic_light = { 0x00, 0xE9, 0x00, 0x83, 0x1B, 0x00, 0x00, 0x00, 0x00 };
        Byte[][] other_commands = new Byte[4][] {
            new Byte[9] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
            new Byte[9]  {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00},
            new Byte[9] {0x00, 0x00, 0x00, 0x83, 0x00, 0x00, 0x00, 0x00, 0x00},
            new Byte[9]  {0x00, 0xE9, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}
        };
        Byte[] activate_mic_ps3 = { 0x00, 0x01, 0x08, 0x01, 0x40, 0x00, 0x00, 0x00, 0x00 };
        Byte[] activate_light_ps3 = { 0x00, 0x01, 0x08, 0x01, 0xFF, 0x00, 0x00, 0x00, 0x00 };
        Byte[] disable_mic_light_ps3 = { 0x00, 0x01, 0x08, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00 };
        private IEnumerator<WaitForSeconds> sendPacket(HidDevice device, Byte[] packet)
        {
            var stream = device.Open();
            stream.SetFeature(packet);
            yield return new WaitForSeconds(1.473f);
            foreach (Byte[] cmd in other_commands)
            {
                stream.SetFeature(cmd);
                yield return new WaitForSeconds(1.473f);
            }
            Byte[] data = new Byte[0x001C];
            stream.GetFeature(data);
            yield return new WaitForSeconds(1.473f);
            stream.Close();
        }

        public void enableMic(HidDevice device)
        {
            StartCoroutine(sendPacket(device, activate_mic));
        }
        public void enableLight(HidDevice device)
        {
            StartCoroutine(sendPacket(device, activate_light));
        }
        public void disableSensors(HidDevice device)
        {
            StartCoroutine(sendPacket(device, disable_mic_light));
        }

        public void enableMicPs3(HidDevice device)
        {
            var stream = device.Open();
            stream.Write(activate_mic_ps3);
            stream.Close();
        }

        public void enableLightPs3(HidDevice device)
        {
            var stream = device.Open();
            stream.Write(activate_light_ps3);
            stream.Close();
        }

        public void disableSensorsPs3(HidDevice device)
        {
            var stream = device.Open();
            stream.Write(disable_mic_light_ps3);
            stream.Close();
        }
    }
}