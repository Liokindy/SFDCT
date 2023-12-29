using Box2D.XNA;
using HarmonyLib;
using SFD;
using SFD.Projectiles;
using SFD.Sounds;
using SFDGameScriptInterface;
using SFR.Sync;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SFR.Game;

/// <summary>
///     This class contain patches that affect all the rounds, such as how the game is supposed to dispose objects.
/// </summary>
[HarmonyPatch]
internal static class World
{
    /*
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectDestructible), nameof(ObjectDestructible.OnDestroyObject))]
    private static void DestroyObject(ObjectDestructible __instance)
    {
        if (__instance.GameOwner != GameOwnerEnum.Client)
        {
            if (__instance.MapObjectID is "CRATE00" or "CRATE01" && Constants.Random.NextDouble() <= 0.02)
            {
                __instance.GameWorld.SpawnDebris(__instance, __instance.GetWorldPosition(), 0f, new[] { Constants.Random.NextBool() ? "BeachBall00" : "Monkey00" }, 1, false);
            }
        }
    }
    */

    /// <summary>
    ///     For unknown reasons players tempt to crash when joining a game.
    ///     This is caused because a collection is being modified during its iteration.
    ///     Therefore we iterate the collection backwards so it can be modified without throwing an exception.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.FinalizeProperties))]
    private static bool FinalizeProperties(GameWorld __instance)
    {
        __instance.b2_settings.timeStep = 0f;
        __instance.Step(__instance.b2_settings);

        for (int i = __instance.DynamicObjects.Count - 1; i >= 0; i--)
        {
            __instance.DynamicObjects.ElementAt(i).Value.FinalizeProperties();
        }

        for (int i = __instance.StaticObjects.Count - 1; i >= 0; i--)
        {
            __instance.StaticObjects.ElementAt(i).Value.FinalizeProperties();
        }

        return false;
    }

    /// <summary>
    ///     This class will be called at the end of every round.
    ///     Use it to dispose your collections or reset some data.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.DisposeAllObjects))]
    private static void DisposeData()
    {
        SyncHandler.Attempts.Clear();
    }

    /// <summary>
    ///     This alters the low-hp effects threshold
    ///     <0.25 to <0.5
    /// </summary>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
    private static IEnumerable<CodeInstruction> Update(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(760).operand = 0.5f; // Initial Check
        instructions.ElementAt(764).operand = 0.5f; // Divider
        instructions.ElementAt(770).operand = 1f; // Bias for saturation
        // instructions.ElementAt(779).operand = 0.6f; // Heartbeat delay Max()
        return instructions;
    }

    /*
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SFD.GameWorld), MethodType.Constructor, typeof(SFD.GameSFD), typeof(SFD.GameOwnerEnum))]
    private static void GameWorldConstructor(SFD.GameWorld __instance)
    {
        __instance.b2_world_active.Gravity = new Microsoft.Xna.Framework.Vector2(0f, -26f);
    }
    */

    /*
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFD.GameWorld), nameof(SFD.GameWorld.CheckProjectileHit))]
    private static bool CheckProjectileHit(GameWorld __instance, ref List<KeyValuePair<RayCastOutput, Fixture>> __result, Projectile projectile, ref Box2D.XNA.RayCastInput rayCastInput, bool environmentCollision, bool playerCollision, bool includeOverlappingFixture, GameWorld.TunnelingCheckType tunnelingCheck)
    {
        List<KeyValuePair<RayCastOutput, Fixture>> hits = new List<KeyValuePair<RayCastOutput, Fixture>>(10);
        Box2D.XNA.RayCastInput _rci = rayCastInput;
        RayCastOutput _rco = default(RayCastOutput);
        AABB aabb;
        AABB.Create(out aabb, _rci.p1, _rci.p2, 0.005f);
        if (environmentCollision)
        {
            __instance.b2_world_active.QueryAABB(delegate (Fixture fixture)
            {
                if (fixture != null && fixture.GetUserData() != null)
                {
                    ObjectData objectData = ObjectData.Read(fixture);
                    if (projectile.CheckProjectileHit(objectData, fixture) && (tunnelingCheck == GameWorld.TunnelingCheckType.None || (tunnelingCheck == GameWorld.TunnelingCheckType.FeetToProjectileBase && objectData.ProjectileTunnelingCheck == ProjectileTunnelingCheck.Full) || (tunnelingCheck == GameWorld.TunnelingCheckType.ProjectileBaseToSpawnPosition && (objectData.ProjectileTunnelingCheck == ProjectileTunnelingCheck.Full || objectData.ProjectileTunnelingCheck == ProjectileTunnelingCheck.IgnoreFeetPerformArm))) && !objectData.IsPlayer && projectile.ObjectIDToIgnore != objectData.ObjectID)
                    {
                        if (fixture.RayCast(out _rco, ref _rci))
                        {
                            bool flag2 = true;
                            foreach (KeyValuePair<RayCastOutput, Fixture> keyValuePair in hits)
                            {
                                if (keyValuePair.Value == fixture)
                                {
                                    flag2 = false;
                                    break;
                                }
                            }
                            if (flag2)
                            {
                                hits.Add(new KeyValuePair<RayCastOutput, Fixture>(_rco, fixture));
                            }
                        }
                        else if (includeOverlappingFixture && fixture.TestPoint(_rci.p1))
                        {
                            bool flag3 = true;
                            foreach (KeyValuePair<RayCastOutput, Fixture> keyValuePair2 in hits)
                            {
                                if (keyValuePair2.Value == fixture)
                                {
                                    flag3 = false;
                                    break;
                                }
                            }
                            if (flag3)
                            {
                                Box2D.XNA.RayCastInput rci = _rci;
                                rci.p1 -= projectile.Direction * 0.04f * 8f;
                                RayCastOutput key;
                                if (!fixture.RayCast(out key, ref rci))
                                {
                                    key.normal = -projectile.Direction;
                                }
                                key.fraction = 0f;
                                hits.Add(new KeyValuePair<RayCastOutput, Fixture>(key, fixture));
                            }
                        }
                    }
                }
                return true;
            }, ref aabb);
        }
        if (playerCollision)
        {
            foreach (Player player in __instance.Players)
            {
                if (!player.IsRemoved && (projectile.PlayerDistanceTraveled > 24f || projectile.PlayerOwnerID != player.ObjectID))
                {
                    AABB aabb2;
                    player.GetAABBWhole(out aabb2);
                    Microsoft.Xna.Framework.Vector2 value = player.CalcServerPositionDifference();
                    aabb2.lowerBound -= value;
                    aabb2.upperBound -= value;
                    if (AABB.TestOverlap(ref aabb2, ref aabb))
                    {
                        bool flag = aabb2.RayCast(out _rco, ref _rci, includeOverlappingFixture);
                        if (flag && player.TestProjectileHit(projectile))
                        {
                            hits.Add(new KeyValuePair<RayCastOutput, Fixture>(_rco, player.WorldBody.GetFixtureList()));
                        }
                    }
                }
            }
        }
        hits.Sort(delegate (KeyValuePair<RayCastOutput, Fixture> p1, KeyValuePair<RayCastOutput, Fixture> p2)
        {
            float fraction = p1.Key.fraction;
            return fraction.CompareTo(p2.Key.fraction);
        });

        __result = hits;
        return false;
    }
    */

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFD.GameWorld), nameof(SFD.GameWorld.CheckProjectileHit))]
    private static void Prefix_CheckProjectileHit(GameWorld __instance, out Projectile __state, Projectile projectile)
    {
        __state = projectile;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SFD.GameWorld), nameof(SFD.GameWorld.CheckProjectileHit))]
    private static void Postfix_CheckProjectileHit(GameWorld __instance, ref List<KeyValuePair<RayCastOutput, Fixture>> __result, Projectile __state)
    {
        // Return if we're a client.
        if (__instance.GameOwner == GameOwnerEnum.Client) { return; }
        if (__state.PlayerOwner == null || __state.PlayerOwner.IsRemoved) { return; }

        // Check if any hit object is a player, and if so
        // check if it's a teammate of the projectile's owner.
        List<KeyValuePair<RayCastOutput, Fixture>> hits = new List<KeyValuePair<RayCastOutput, Fixture>>();
        foreach (KeyValuePair<RayCastOutput, Fixture> value in __result)
        {
            ObjectData objectData = ObjectData.Read(value.Value);
            bool flag = true;

            if (objectData.IsPlayer)
            {
                Player player = (Player)objectData.InternalData;
                if (!player.IsDead && __state.PlayerOwner.InSameTeam(player))
                {
                    // 0.9f * 0.85f = 0.72f
                    if (SFD.Constants.RANDOM.NextDouble() < (double)__state.Properties.DodgeChance * 0.85f)
                    {
                        flag = false;

                        // Declare projectile as missed
                        player.m_projectileMissed.Add(__state);
                        // Feedback
                        player.Shake.Start(500f);
                    }
                }
            }

            if (flag)
            {
                hits.Add(value);
            }
        }

        __result = hits;
    }
}