using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFD;
using SFD.Objects;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFDCT.Helper;

namespace SFDCT.Objects;

[HarmonyPatch]
internal static class ObjectDataHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectData), nameof(ObjectData.DrawActivateHightlight))]
    private static bool DrawActivateHighlight(ObjectData __instance, SpriteBatch spriteBatch)
    {
        bool doOverStep = __instance is not ObjectWeaponItem;
        Color outlineCol = Color.White;
        Color outlineEdgeCol = outlineCol;
        outlineEdgeCol.A = 96;

        Vector2 drawPos;
        float drawAng;
        if (__instance.CurrentAnimation == null)
        {
            for (int i = 0; i < __instance.m_objectDecals.Count; i++)
            {
                ObjectDecal objectDecal = __instance.m_objectDecals[i];
                drawAng = -__instance.Body.GetAngle();
                drawPos = (objectDecal.HaveOffset ? __instance.Body.GetWorldPoint(objectDecal.LocalOffset) : __instance.Body.Position);
                if (doOverStep)
                {
                    drawAng -= __instance.GameWorld.DrawingBox2DSimulationTimestepOver * __instance.Body.GetAngularVelocity();
                    drawPos += __instance.GameWorld.DrawingBox2DSimulationTimestepOver * __instance.Body.GetLinearVelocity();
                }
                Camera.ConvertBox2DToScreen(ref drawPos, out drawPos);

                spriteBatch.Draw(objectDecal.Texture, drawPos + new Vector2(1f, 0), null, outlineCol, drawAng, objectDecal.TextureOrigin, Camera.ZoomUpscaled, __instance.m_faceDirectionSpriteEffect, 0f);
                spriteBatch.Draw(objectDecal.Texture, drawPos + new Vector2(0, 1f), null, outlineCol, drawAng, objectDecal.TextureOrigin, Camera.ZoomUpscaled, __instance.m_faceDirectionSpriteEffect, 0f);
                spriteBatch.Draw(objectDecal.Texture, drawPos + new Vector2(-1f, 0), null, outlineCol, drawAng, objectDecal.TextureOrigin, Camera.ZoomUpscaled, __instance.m_faceDirectionSpriteEffect, 0f);
                spriteBatch.Draw(objectDecal.Texture, drawPos + new Vector2(0, -1f), null, outlineCol, drawAng, objectDecal.TextureOrigin, Camera.ZoomUpscaled, __instance.m_faceDirectionSpriteEffect, 0f);

                spriteBatch.Draw(objectDecal.Texture, drawPos + new Vector2(1f), null, outlineEdgeCol, drawAng, objectDecal.TextureOrigin, Camera.ZoomUpscaled, __instance.m_faceDirectionSpriteEffect, 0f);
                spriteBatch.Draw(objectDecal.Texture, drawPos + new Vector2(1f, -1f), null, outlineEdgeCol, drawAng, objectDecal.TextureOrigin, Camera.ZoomUpscaled, __instance.m_faceDirectionSpriteEffect, 0f);
                spriteBatch.Draw(objectDecal.Texture, drawPos + new Vector2(-1f, 1f), null, outlineEdgeCol, drawAng, objectDecal.TextureOrigin, Camera.ZoomUpscaled, __instance.m_faceDirectionSpriteEffect, 0f);
                spriteBatch.Draw(objectDecal.Texture, drawPos + new Vector2(-1f), null, outlineEdgeCol, drawAng, objectDecal.TextureOrigin, Camera.ZoomUpscaled, __instance.m_faceDirectionSpriteEffect, 0f);
            }
            return false;
        }

        int sizeMulX;
        int sizeMulY;
        __instance.GetObjectSizeMultiplier(out sizeMulX, out sizeMulY);
        if (sizeMulX != 1 | sizeMulY != 1)
        {
            Vector2 value2 = __instance.Body.GetPosition();
            if (doOverStep)
            {
                value2 += __instance.GameWorld.DrawingBox2DSimulationTimestepOver * __instance.Body.GetLinearVelocity();
            }

            value2 = Converter.ConvertBox2DToWorld(value2);

            Vector2 zero = Vector2.Zero;
            for (int j = 0; j < sizeMulX; j++)
            {
                for (int k = 0; k < sizeMulY; k++)
                {
                    zero.X = (float)(j * __instance.CurrentAnimation.FrameWidth);
                    zero.Y = -(float)(k * __instance.CurrentAnimation.FrameHeight);
                    
                    drawAng = __instance.Body.GetAngle();
                    if (doOverStep)
                    {
                        drawAng += __instance.GameWorld.DrawingBox2DSimulationTimestepOver * __instance.Body.GetAngularVelocity();
                    }
                    
                    SFDMath.RotatePosition(ref zero, drawAng, out zero);
                    drawPos = Camera.ConvertWorldToScreen(value2 + zero);

                    __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(1f, 0), drawAng, __instance.m_faceDirectionSpriteEffect, outlineCol);
                    __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(0, 1f), drawAng, __instance.m_faceDirectionSpriteEffect, outlineCol);
                    __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(-1f, 0), drawAng, __instance.m_faceDirectionSpriteEffect, outlineCol);
                    __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(0, -1f), drawAng, __instance.m_faceDirectionSpriteEffect, outlineCol);

                    __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(1f), drawAng, __instance.m_faceDirectionSpriteEffect, outlineEdgeCol);
                    __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(1f, -1f), drawAng, __instance.m_faceDirectionSpriteEffect, outlineEdgeCol);
                    __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(-1f, 1f), drawAng, __instance.m_faceDirectionSpriteEffect, outlineEdgeCol);
                    __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(-1f), drawAng, __instance.m_faceDirectionSpriteEffect, outlineEdgeCol);
                }
            }
            return false;
        }

        drawAng = __instance.Body.GetAngle();
        drawPos = __instance.Body.GetPosition();
        if (doOverStep)
        {
            drawAng += __instance.GameWorld.DrawingBox2DSimulationTimestepOver * __instance.Body.GetAngularVelocity();
            drawPos += __instance.GameWorld.DrawingBox2DSimulationTimestepOver * __instance.Body.GetLinearVelocity();
        }
        
        Camera.ConvertBox2DToScreen(ref drawPos, out drawPos);

        __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(1f, 0), drawAng, __instance.m_faceDirectionSpriteEffect, outlineCol);
        __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(0, 1f), drawAng, __instance.m_faceDirectionSpriteEffect, outlineCol);
        __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(-1f, 0), drawAng, __instance.m_faceDirectionSpriteEffect, outlineCol);
        __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(0, -1f), drawAng, __instance.m_faceDirectionSpriteEffect, outlineCol);

        __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(1f), drawAng, __instance.m_faceDirectionSpriteEffect, outlineEdgeCol);
        __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(1f, -1f), drawAng, __instance.m_faceDirectionSpriteEffect, outlineEdgeCol);
        __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(-1f, 1f), drawAng, __instance.m_faceDirectionSpriteEffect, outlineEdgeCol);
        __instance.CurrentAnimation.Draw(spriteBatch, __instance.Texture, drawPos + new Vector2(-1f), drawAng, __instance.m_faceDirectionSpriteEffect, outlineEdgeCol);
        return false;
    }
}