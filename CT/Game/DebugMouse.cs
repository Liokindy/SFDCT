using Box2D.XNA;
using Microsoft.Xna.Framework;
using SFD;

namespace SFDCT.Game;

internal class DebugMouse
{
    internal GameConnectionTag Tag;
    internal Vector2 Box2DPosition;
    internal bool Pressed;
    internal ObjectData Object;
    internal MouseJoint Joint;
    internal World World;
    internal float LastNetUpdateTime;
}
