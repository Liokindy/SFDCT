using System;
using SFD;
using SFD.Objects;
using SFDCT.Helper;
using HarmonyLib;

namespace SFDCT.Objects
{
    [HarmonyPatch]
    internal static class Default
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ObjectPulleyJoint), nameof(ObjectPulleyJoint.RecreateJoint))]
        private static void ObjectPulleyJointRecreateJoint(ObjectPulleyJoint __instance)
        {
            if (string.IsNullOrEmpty(__instance.m_lineTexture.Name))
            {
                __instance.m_lineTexture = null;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ObjectPullJoint), nameof(ObjectPullJoint.RecreateJoint))]
        private static void ObjectPullJointRecreateJoint(ObjectPullJoint __instance)
        {
            if (string.IsNullOrEmpty(__instance.m_lineTexture.Name))
            {
                __instance.m_lineTexture = null;
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(ObjectDistanceJoint), nameof(ObjectDistanceJoint.UpdateLineTexture))]
        //private static void ObjectDistanceJointUpdateLineTexture(ObjectDistanceJoint __instance)
        //{
        //    if (string.IsNullOrEmpty(__instance.m_lineTexture.Name))
        //    {
        //        __instance.m_lineTexture = null;
        //    }
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(ObjectElevatorPathJoint), nameof(ObjectElevatorPathJoint.UpdateLineTexture))]
        //private static void ObjectElevatorPathJointUpdateLineTexture(ObjectElevatorPathJoint __instance)
        //{
        //    if (string.IsNullOrEmpty(__instance.m_lineTexture.Name))
        //    {
        //        __instance.m_lineTexture = null;
        //    }
        //}
    }
}
