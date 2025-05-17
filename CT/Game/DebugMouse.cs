using Box2D.XNA;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Objects;
using System;
using System.Collections.Generic;

namespace SFDCT.Game;

internal class DebugMouse
{
    public bool IsDisposed;

    public long RemoteUniqueIdentifier;
    public double LastUpdateNetTime;

    public MouseJoint MouseJoint;
    public ObjectData MouseObject;
    public ObjectData MouseConfirmDeletionObject;
    public double MouseConfirmDeletionObjectTime;
    public World MouseWorld;

    public bool MouseDeleteRequest;
    public bool MouseIsPressed;
    public bool MouseLastIsPressed;

    public Vector2 MouseBox2DPosition;
    public Vector2 MouseWorldPosition
    {
        get
        {
            return Converter.Box2DToWorld(MouseBox2DPosition);
        }
    }

    public Vector2 MouseLastBox2DPosition;

    public void SendMessageToOwner(string message, Color color)
    {
        GameSFD gameSFD = GameSFD.Handle;
        Server server = gameSFD?.Server;
        if (server != null)
        {
            NetConnection recipient;
            recipient = server.GetConnectionByRemoteUniqueIdentifier(this.RemoteUniqueIdentifier);

            if (recipient != null)
            {
                server.SendMessage(MessageType.ChatMessage, new NetMessage.ChatMessage.Data(message, color), null, recipient);
            }
        }
    }

    public void Update(GameWorld world)
    {
        if (world == null) return;

        if (this.MouseDeleteRequest)
        {
            List<ObjectData> allObjectsAtPosition = world.GetAllObjectsAtPosition(this.MouseBox2DPosition);

            if (allObjectsAtPosition.Count > 0)
            {
                allObjectsAtPosition.Sort(new Comparison<ObjectData>(world.DeleteObjectAtCursorSorting));
                ObjectData objectData = allObjectsAtPosition[0];

                if (world.DeleteObjectAtCursorConfirmDeletion(objectData) && (this.MouseConfirmDeletionObject != objectData || NetTime.Now - this.MouseConfirmDeletionObjectTime > 0.35))
                {
                    this.MouseConfirmDeletionObject = objectData;
                    this.MouseConfirmDeletionObjectTime = NetTime.Now;

                    this.SendMessageToOwner(string.Format("Confirm deletion: ({0}) {1}", objectData.ObjectID, objectData.MapObjectID), Color.Yellow);
                    this.MouseDeleteRequest = false;
                    return;
                }

                this.SendMessageToOwner(string.Format("Deleting ({0}) {1}", objectData.ObjectID, objectData.MapObjectID), Color.IndianRed);
                objectData.Destroy();

                this.MouseDeleteRequest = false;
                EffectHandler.PlayEffect("GR_D", this.MouseWorldPosition, world);
            }
        }
        else
        {
            if (this.MouseIsPressed)
            {
                if (!this.MouseLastIsPressed)
                {
                    List<ObjectData> triggers = [];

                    AABB aabb;
                    AABB.Create(out aabb, this.MouseBox2DPosition, 0.16f);

                    world.GetActiveWorld.QueryAABB(delegate (Fixture fixture)
                    {
                        if (fixture != null)
                        {
                            ObjectData objectData2 = ObjectData.Read(fixture);
                            if (objectData2 is ObjectActivateTrigger && objectData2.Activateable && world.EditCheckTouch(objectData2, this.MouseWorldPosition))
                            {
                                triggers.Add(objectData2);
                            }
                        }
                        return true;
                    }, ref aabb);

                    bool activatedTrigger = false;
                    foreach (ObjectData objectData in triggers)
                    {
                        if (!objectData.IsDisposed)
                        {
                            activatedTrigger = true;
                            this.SendMessageToOwner(string.Format("Activating ({0}) {1}", objectData.ObjectID, objectData.MapObjectID), Color.LightGreen);

                            objectData.Activate(null);
                        }
                    }

                    if (activatedTrigger)
                    {
                        this.MouseLastIsPressed = this.MouseIsPressed;
                        this.MouseLastBox2DPosition = this.MouseBox2DPosition;
                        return;
                    }
                }

                float dist = Vector2.Distance(this.MouseBox2DPosition, this.MouseLastBox2DPosition);
                float maxDist = 0.4f;

                dist = Math.Min(dist, maxDist * 20f);

                EffectHandler.PlayEffect("GLM", this.MouseWorldPosition, world);
                if (dist > maxDist)
                {
                    Vector2 dir = (this.MouseBox2DPosition - this.MouseLastBox2DPosition);
                    dir.Normalize();

                    Vector2 pos = this.MouseBox2DPosition;
                    for (float i = 0; i < dist - maxDist; i += maxDist)
                    {
                        pos -= dir * maxDist;
                        EffectHandler.PlayEffect("GLM", Converter.Box2DToWorld(pos), world);
                    }
                }

                if (this.MouseObject == null)
                {
                    ObjectData objectAtMousePosition = world.GetObjectAtPosition(this.MouseBox2DPosition, true, true, true, world.EditGroupID, new Func<ObjectData, bool>(world.DebugMouseFilter));

                    if (objectAtMousePosition != null && objectAtMousePosition.IsDynamic)
                    {
                        this.MouseObject = objectAtMousePosition;
                    }

                    if (this.MouseObject != null)
                    {
                        this.MouseObject.Body.SetAwake(true);

                        MouseJointDef mouseJointDef = new MouseJointDef();
                        mouseJointDef.target = this.MouseBox2DPosition;
                        mouseJointDef.localAnchor = this.MouseObject.Body.GetLocalPoint(mouseJointDef.target);
                        float num = this.MouseObject.Body.GetMass();
                        List<Body> connectedWeldedBodies = this.MouseObject.Body.GetConnectedWeldedBodies();
                        if (connectedWeldedBodies != null)
                        {
                            for (int i = 0; i < connectedWeldedBodies.Count; i++)
                            {
                                num += connectedWeldedBodies[i].GetMass();
                            }
                        }
                        mouseJointDef.maxForce = num * 150f;
                        mouseJointDef.dampingRatio = 1f;
                        mouseJointDef.frequencyHz = 40f;
                        mouseJointDef.collideConnected = false;
                        this.MouseWorld = this.MouseObject.Body.GetWorld();
                        mouseJointDef.bodyA = this.MouseWorld.GroundBody;
                        mouseJointDef.bodyB = this.MouseObject.Body;
                        this.MouseJoint = (MouseJoint)this.MouseObject.Body.GetWorld().CreateJoint(mouseJointDef);
                    }
                }

                if (this.MouseObject != null && !this.MouseObject.IsDisposed)
                {
                    this.MouseJoint.SetTarget(this.MouseBox2DPosition);

                    if (this.MouseObject.IsPlayer)
                    {
                        Player player = (Player)this.MouseObject.InternalData;
                        if (!player.IsRemoved && !player.Falling)
                        {
                            player.Fall();
                            return;
                        }
                    }
                }
            }
            else
            {
                if (this.MouseObject != null)
                {
                    if (this.MouseJoint != null && !this.MouseObject.IsDisposed)
                    {
                        this.MouseWorld.DestroyJoint(this.MouseJoint);
                    }

                    this.MouseJoint = null;
                    this.MouseObject = null;
                }
            }
        }

        this.MouseLastIsPressed = this.MouseIsPressed;
        this.MouseLastBox2DPosition = this.MouseBox2DPosition;
    }

    public void Dispose()
    {
        if (this.IsDisposed)
        {
            return;
        }

        if (this.MouseObject != null && this.MouseWorld != null)
        {
            if (this.MouseJoint != null && !this.MouseObject.IsDisposed)
            {
                this.MouseWorld.DestroyJoint(this.MouseJoint);
            }
        }

        this.MouseWorld = null;
        this.MouseJoint = null;
        this.MouseObject = null;
        this.IsDisposed = true;
    }
}
