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
using System.Runtime.InteropServices;
namespace HidSharpUnity
{
    public class RewiredSharpJoystick {
        private Joystick _joystick;
        public Joystick rewiredJoystick { get => _joystick; }
        private HidDevice _hiddevice;
        public HidDevice hidSharpDevice { get => _hiddevice; }
        public RewiredSharpJoystick(Joystick joystick, HidDevice device) {
            this._joystick = joystick;
            this._hiddevice = device;
        }
    }
    public class HidSharpUnity
    {

        private static Guid CreateGuidHashSHA1(string text)
        {
            using (SHA1 val = SHA1.Create())
            {
                byte[] sourceArray = ((HashAlgorithm)val).ComputeHash(Encoding.UTF8.GetBytes(text));
                byte[] array = new byte[16];
                Array.Copy(sourceArray, array, 16);
                return new Guid(array);
            }
        }
        private static Guid CreateGuidHashSHA256(string text)
        {
            SHA256Managed val = new SHA256Managed();
            byte[] sourceArray = ((HashAlgorithm)val).ComputeHash(Encoding.UTF8.GetBytes(text));
            byte[] array = new byte[16];
            Array.Copy(sourceArray, array, 16);
            return new Guid(array);
        }
        public RewiredSharpJoystick findHidSharpDevice(Joystick joystick)
        {
            var list = DeviceList.Local;
            string id = joystick.hardwareIdentifier;
            int? vid = null;
            int? pid = null;
            // With macOS, we can't easily determine the vid and pid as we want. 
            // Setting these to null will instead loop over all devices.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                id = id.Substring(id.Length - 36);
                vid = Convert.ToInt32(id.Substring(4, 4), 16);
                pid = Convert.ToInt32(id.Substring(0, 4), 16);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && id.Contains("XInput")) {
                // For rewired xinput devices, the userIndex is just appended to the joystick name
                int userIndex = Int32.Parse(joystick.name.Substring(joystick.name.Length - 1));
                List<HidDevice> xinputDevices = new List<HidDevice>();
                foreach (HidDevice device in list.GetHidDevices(null, null, null, null)) {
                    String text = device.DevicePath;
                    // On windows, ig_xx is added to XInput devices, where xx is a number that increases for each device
                    if (text.Contains("ig_")) {
                        xinputDevices.Add(device);
                    }
                }
                // If we sort the ig numbers, than the device list will be in the same order as the userIndex
                xinputDevices.Sort((x,y) => x.DevicePath.Split(new [] {"ig_"}, StringSplitOptions.None)[1].Split('#')[0].CompareTo(y.DevicePath.Split(new [] {"ig_"}, StringSplitOptions.None)[1].Split('#')[0]));
                return new RewiredSharpJoystick(joystick, xinputDevices[userIndex-1]);

            }
            foreach (HidDevice device in list.GetHidDevices(vid, pid, null, null))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // deviceInstanceGuid is just a SHA1 hash of the DevicePath
                    if (CreateGuidHashSHA1(device.DevicePath) == joystick.deviceInstanceGuid)
                    {
                        return new RewiredSharpJoystick(joystick, device);
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Rewired encodes the device path into the deviceInstanceGuid
                    // We can construct our own deviceInstanceGuid based on the HidSharp device
                    // And then just find the device that matches
                    String text = device.DevicePath;
                    Guid guid;
                    //RawInput uses the device instance ID with a null byte on the end, DirectInput just uses the device path
                    //RawInput uses SHA1, DirectInput uses SHA256
                    if (joystick.hardwareIdentifier.Contains("RawInput"))
                    {
                        // Raw input uses the SHA1 hash of the Device Instance ID as its identifier.
                        // We can get a Device Instance ID from a path with the below transform.
                        // Note that there is an extra null byte on the end.
                        text = text.Substring(0, text.IndexOf("{") - 1).Substring(4).ToUpper().Replace("#", "\\") + "\0";
                        guid = CreateGuidHashSHA1(text);
                    }
                    else
                    {
                        // DirectInput just uses the SHA256 of the device path
                        guid = CreateGuidHashSHA256(text);
                    }
                    if (guid == joystick.deviceInstanceGuid)
                    {
                        return new RewiredSharpJoystick(joystick, device);
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    //VID and PID are at the end but we can't easily splice them individually, hence the first loop
                    //Once we have a device with the right VID and PID though, we can then loop through and work out the actual device.
                    if (joystick.hardwareIdentifier.EndsWith(device.VendorID.ToString() + device.ProductID.ToString())) {
                        List<Joystick> sortedJoys = new List<Joystick>(ReInput.controllers.Joysticks);
                        // Internally, rewired grabs a list of all hid devices, and then sorts by the location.
                        // After this process the system id is incremented for each device in order.
                        // Sorting by systemId will give us a list of devices sorted by location.
                        // Rewired does sort in reverse though, so we need to flip it
                        sortedJoys.Sort((x,y) => -Nullable.Compare(x.systemId, y.systemId));
                        sortedJoys.Where(x => x.hardwareIdentifier.EndsWith(device.VendorID.ToString() + device.ProductID.ToString()));
                        List<HidDevice> hidDevices = list.GetHidDevices(device.VendorID, device.ProductID, null, null).ToList();
                        // Now sort the HidSharp devices, this means that both lists are in the same order
                        hidDevices.Sort((x, y) => x.Location.CompareTo(y.Location));
                        // And return the hidsharp at the same index
                        return new RewiredSharpJoystick(joystick, hidDevices.ToList()[sortedJoys.IndexOf(joystick)]);
                    }
                }
            }
            Debug.Log("Unable to find hidSharpDevice for " + joystick.hardwareIdentifier);
            return null;
        }
    }
}