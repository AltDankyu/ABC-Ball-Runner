using System;
using UnityEngine;

#if UNITY_IOS
using System.Runtime.InteropServices;
using UnityEngine.iOS;
#endif

namespace SupersonicWisdomSDK
{
    internal static class SwSKAdNetworkAdapter
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        extern private static void _swRegisterAppForAdNetwork();
        
        [DllImport("__Internal")]
        extern private static void _swUpdateConversionValue(int conversionValue);
#endif

        public static void RegisterAppForAdNetwork ()
        {
#if !UNITY_IOS || UNITY_EDITOR
            throw new Exception($"RegisterAppForAdNetwork() Unsupported system {Application.platform}");
#elif UNITY_IOS
            _swRegisterAppForAdNetwork();
            SwInfra.Logger.Log("SKAdNetwork | RegisterAppForAdNetwork");
#endif
        }

        public static void UpdateConversionValue(int conversionValue)
        {
#if !UNITY_IOS || UNITY_EDITOR
            throw new Exception($"UpdateConversionValue({conversionValue}) Unsupported system {Application.platform}");
#elif UNITY_IOS
            _swUpdateConversionValue(conversionValue);
            SwInfra.Logger.Log($"SKAdNetwork | UpdateConversionValue | {conversionValue}");
#endif
        }
    }
}