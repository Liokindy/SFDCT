using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Projectiles;
using HarmonyLib;
using SFD.Objects;
using Box2D.XNA;
using SFD.Effects;
using SFD.Sounds;

namespace SFDCT.Fighter;

/// <summary>
///     This class contains all patches regarding players movements, delays etc...
/// </summary>
[HarmonyPatch]
internal static class PlayerHandler
{
    // Nothing
}