using Amplitude;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TASTools_Mod
{
    class InputManagerHooks
    {
        public static void TriggerClickDownEvent(InputManager self, string eventName, ref List<ClickDownInfo> clickInfosContainer)
        {
            DynData<InputManager> d = new DynData<InputManager>(self);
            d.Set<bool>("stopClickEventPropagation", false);
            LayerMask mask = (!d.Get<IGameCameraService>("gameCameraManager").IsTacticalMapActive()) ? d.Get<LayerMask>("gameMouseClickLayerMask") : d.Get<LayerMask>("tacticalMapClickLayerMask");
            RaycastHit[] array = Physics.RaycastAll(d.Get<IGameCameraService>("gameCameraManager").ScreenPointToRay(TASInputPlayer.GetMousePos()), float.PositiveInfinity, mask);
            Array.Sort<RaycastHit>(array, (RaycastHit hitInfo1, RaycastHit hitInfo2) => hitInfo1.distance.CompareTo(hitInfo2.distance));
            if (d.Get<bool>("debug"))
            {
                Diagnostics.LogWarning(string.Concat(new object[]
                {
                eventName,
                " > ",
                array.Length,
                " hits"
                }), new object[0]);
            }
            clickInfosContainer = new List<ClickDownInfo>();
            int num = 0;
            foreach (RaycastHit raycastHit in array)
            {
                ClickDownInfo item = new ClickDownInfo(raycastHit.collider, raycastHit.point, num, array.Length);
                clickInfosContainer.Add(item);
            }
            foreach (ClickDownInfo clickDownInfo in clickInfosContainer)
            {
                if (d.Get<bool>("debug"))
                {
                    Diagnostics.Log(string.Concat(new object[]
                    {
                    eventName,
                    "@",
                    num,
                    "=",
                    clickDownInfo.HitCollider.name,
                    " @",
                    clickDownInfo.WorldPosition
                    }), new object[0]);
                }
                clickDownInfo.HitCollider.SendMessage(eventName, clickDownInfo, SendMessageOptions.DontRequireReceiver);
                num++;
                if (d.Get<bool>("stopClickEventPropagation"))
                {
                    break;
                }
            }
        }
        public static void TriggerClickUpEvent(InputManager self, string eventName, ref List<ClickDownInfo> clickInfosContainer)
        {
            DynData<InputManager> d = new DynData<InputManager>(self);
            if (d.Get<bool>("debug"))
            {
                Diagnostics.LogWarning(string.Concat(new object[]
                {
                eventName,
                " > ",
                clickInfosContainer.Count,
                " hits"
                }), new object[0]);
            }
            d.Set("stopClickEventPropagation", false);
            bool isDragging = SingletonManager.Get<CameraDragSupport>(true).IsDragging;
            foreach (ClickDownInfo clickDownInfo in clickInfosContainer)
            {
                if (clickDownInfo.HitCollider != null)
                {
                    if (d.Get<bool>("debug"))
                    {
                        Diagnostics.Log(string.Concat(new object[]
                        {
                        eventName,
                        "@",
                        clickDownInfo.Index,
                        "=",
                        clickDownInfo.HitCollider.name
                        }), new object[0]);
                    }
                    clickDownInfo.HitCollider.SendMessage(eventName, new ClickUpInfo(clickDownInfo, isDragging), SendMessageOptions.DontRequireReceiver);
                    if (d.Get<bool>("stopClickEventPropagation"))
                    {
                        break;
                    }
                }
            }
            clickInfosContainer = null;
        }
        public static void Update_MouseEvents(InputManager self)
        {
            DynData<InputManager> d = new DynData<InputManager>(self);
            List<ClickDownInfo> leftClickInfos = d.Get<List<ClickDownInfo>>("leftClickInfos");
            List<ClickDownInfo> rightClickInfos = d.Get<List<ClickDownInfo>>("rightClickInfos");
            List<ClickDownInfo> middleClickInfos = d.Get<List<ClickDownInfo>>("middleClickInfos");
            if (!AgeManager.IsMouseCovered)
            {
                if (TASInputPlayer.GetMouseButtonDown(0))
                {
                    TriggerClickDownEvent(self, "OnLeftClickDown", ref leftClickInfos);
                }
                if (TASInputPlayer.GetMouseButtonDown(1))
                {
                    TriggerClickDownEvent(self, "OnRightClickDown", ref rightClickInfos);
                }
                if (TASInputPlayer.GetMouseButtonDown(2))
                {
                    TriggerClickDownEvent(self, "OnMiddleClickDown", ref middleClickInfos);
                }
                // TODO MAYBE ADD MOUSEWHEEL?
            }
            if (TASInputPlayer.GetMouseButtonUp(0) && leftClickInfos != null)
            {
                TriggerClickUpEvent(self, "OnLeftClickUp", ref leftClickInfos);
            }
            if (TASInputPlayer.GetMouseButtonUp(1) && rightClickInfos != null)
            {
                TriggerClickUpEvent(self, "OnRightClickUp", ref rightClickInfos);
            }
            if (TASInputPlayer.GetMouseButtonUp(2) && middleClickInfos != null)
            {
                TriggerClickUpEvent(self, "OnMiddleClickUp", ref middleClickInfos);
            }
            d.Set("leftClickInfos", leftClickInfos);
            d.Set("rightClickInfos", rightClickInfos);
            d.Set("middleClickInfos", middleClickInfos);
        }
    }
}
