using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using HarmonyLib;
using Rewired.Utils;
using Rewired;
using System.Security.Cryptography;
using System.Text;

public class MyPatcher
{
    // make sure DoPatching() is called at start either by
    // the mod loader or by your injector

    public static void DoPatching()
    {
        var harmony = new Harmony("com.example.patch");
        harmony.PatchAll();
    }
}

[HarmonyPatch()]
class Patch01
{

    static MethodInfo TargetMethod() {
        return typeof(AxisType).Assembly.GetType("Rewired.Utils.MiscTools")
            .GetMethod("CreateGuidHashSHA1",  BindingFlags.Static | BindingFlags.Public);
    }
    public static Guid CreateGuidHashSHA1(string text)
        {
            using (SHA1 val = SHA1.Create()) {
                byte[] sourceArray = ((HashAlgorithm)val).ComputeHash(Encoding.UTF8.GetBytes(text));
                byte[] array = new byte[16];
                Array.Copy(sourceArray, array, 16);
                return new Guid(array);
            }
        }
    static public void PrintByteArray(byte[] bytes)
{
    var sb = new StringBuilder("new byte[] { ");
    foreach (var b in bytes)
    {
        sb.Append(b + ", ");
    }
    sb.Append("}");
    Debug.Log(sb.ToString());
}
    static bool Prefix(ref string text)
    {
        Debug.Log("PrefixSHA1");
        Debug.Log(CreateGuidHashSHA1(text));
        Debug.Log(text);
        PrintByteArray(Encoding.UTF8.GetBytes(text));
        return false;
    }

    static void Postfix(ref string text)
    {
    }
}
[HarmonyPatch()]
class Patch02
{

    static MethodInfo TargetMethod() {
        return typeof(AxisType).Assembly.GetType("Rewired.Utils.MiscTools")
            .GetMethod("CreateGuidHashSHA256",  BindingFlags.Static | BindingFlags.Public);
    }
    public static Guid CreateGuidHashSHA256(string text)
        {
           SHA256Managed val = new SHA256Managed();
           byte[] sourceArray = ((HashAlgorithm)val).ComputeHash(Encoding.UTF8.GetBytes(text));
           byte[] array = new byte[16];
           Array.Copy(sourceArray, array, 16);
           return new Guid(array);
        }
    static public void PrintByteArray(byte[] bytes)
{
    var sb = new StringBuilder("new byte[] { ");
    foreach (var b in bytes)
    {
        sb.Append(b + ", ");
    }
    sb.Append("}");
    Debug.Log(sb.ToString());
}
    static bool Prefix(ref string text)
    {
        Debug.Log("PrefixSHA256");
        Debug.Log(CreateGuidHashSHA256(text));
        Debug.Log(text);
        PrintByteArray(Encoding.UTF8.GetBytes(text));
        return false;
    }

    static void Postfix(ref string text)
    {
    }
}
public class TestPatcher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MyPatcher.DoPatching();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
