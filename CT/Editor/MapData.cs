//using HarmonyLib;

//namespace SFDCT.Editor;

//[HarmonyPatch]
//internal static class MapData
//{
//    [HarmonyPrefix]
//    [HarmonyPatch(typeof(MapInfo), nameof(MapInfo.IsOfficialMap))]
//    private static bool GetMapOfficialToken(string header, char[] chars)
//    {
//        char[] array = "0123456789".ToCharArray();
//        int num = array.Length;
//        for (int i = 0; i < header.Length; i++)
//        {
//            array[i % num] = (char)(array[i % num] + header[i % num]);
//        }

//        array[0] = '1';
//        Logger.LogDebug($"Map validation token: {BitConverter.ToString(Encoding.UTF8.GetBytes(new string(array))).Replace("-", string.Empty)}");
//        return true;
//    }
//}
