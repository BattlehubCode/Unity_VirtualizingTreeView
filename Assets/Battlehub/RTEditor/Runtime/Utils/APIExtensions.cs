using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

#if UNITY_6000_0_OR_NEWER
using Battlehub.RTCommon;
#endif

namespace Battlehub
{
    public static class ParticleSystemExt
    {
        public static int GetColliderCount(this ParticleSystem.TriggerModule o)
        {
#if UNITY_2020_2_OR_NEWER
            return o.colliderCount;
#else
            return o.maxColliderCount;
#endif
        }

        public static int GetPlaneCount(this ParticleSystem.CollisionModule o)
        {
#if UNITY_2020_2_OR_NEWER
            return o.planeCount;
#else
            return o.maxPlaneCount;
#endif
        }
    }

    public static class CameraExt
    {

#if UNITY_6000_0_OR_NEWER
        private static readonly Dictionary<Camera, Dictionary<CameraEvent, List<CommandBuffer>>> s_commandBuffers = new Dictionary<Camera, Dictionary<CameraEvent, List<CommandBuffer>>>();
        private static readonly IList<CommandBuffer> s_empty = new CommandBuffer[0];
#endif

        public static bool HasCmdBuffers(this Camera camera)
        {
#if UNITY_6000_0_OR_NEWER
            if (RenderPipelineInfo.Type == RPType.Standard)
            {
                return camera.commandBufferCount > 0;
            }
            return s_commandBuffers.ContainsKey(camera);
#else
            return camera.commandBufferCount > 0;
#endif
        }

        public static void AddCmdBuffer(this Camera camera, CameraEvent cameraEvent, CommandBuffer commandBuffer)
        {
#if UNITY_6000_0_OR_NEWER
            if (RenderPipelineInfo.Type == RPType.Standard)
            {
                camera.AddCommandBuffer(cameraEvent, commandBuffer);
            }
            else
            {
                if (!s_commandBuffers.TryGetValue(camera, out var cameraEventToCommandBuffers))
                {
                    cameraEventToCommandBuffers = new Dictionary<CameraEvent, List<CommandBuffer>>();
                    s_commandBuffers.Add(camera, cameraEventToCommandBuffers);
                }

                if (!cameraEventToCommandBuffers.TryGetValue(cameraEvent, out var commandBuffers))
                {
                    commandBuffers = new List<CommandBuffer>();
                    cameraEventToCommandBuffers.Add(cameraEvent, commandBuffers);
                }

                commandBuffers.Add(commandBuffer);
            }
#else
            camera.AddCommandBuffer(cameraEvent, commandBuffer);
#endif
        }

        public static void RemoveCmdBuffer(this Camera camera, CameraEvent cameraEvent, CommandBuffer commandBuffer)
        {
#if UNITY_6000_0_OR_NEWER
            if (RenderPipelineInfo.Type == RPType.Standard)
            {
                camera.RemoveCommandBuffer(cameraEvent, commandBuffer);
            }
            else
            {
                if (!s_commandBuffers.TryGetValue(camera, out var cameraEventToCommandBuffers))
                {
                    return;
                }

                if (!cameraEventToCommandBuffers.TryGetValue(cameraEvent, out var commandBuffers))
                {
                    return;
                }

                commandBuffers.Remove(commandBuffer);
                if (commandBuffers.Count == 0)
                {
                    cameraEventToCommandBuffers.Remove(cameraEvent);
                    if (cameraEventToCommandBuffers.Count == 0)
                    {
                        s_commandBuffers.Remove(camera);
                    }
                }
            }
#else
            camera.RemoveCommandBuffer(cameraEvent, commandBuffer);
#endif
        }

        public static void RemoveAllCmdBuffers(this Camera camera)
        {
#if UNITY_6000_0_OR_NEWER
            if (RenderPipelineInfo.Type == RPType.Standard)
            {
                camera.RemoveAllCommandBuffers();
            }
            else
            {
                if (!s_commandBuffers.TryGetValue(camera, out var cameraEventToCommandBuffers))
                {
                    return;
                }

                s_commandBuffers.Remove(camera);
            }
#else
            camera.RemoveAllCommandBuffers();
#endif
        }

        public static IList<CommandBuffer> GetCmdBuffers(this Camera camera, CameraEvent cameraEvent)
        {
#if UNITY_6000_0_OR_NEWER
            if (RenderPipelineInfo.Type == RPType.Standard)
            {
                return camera.GetCommandBuffers(cameraEvent);
            }

            if (!s_commandBuffers.TryGetValue(camera, out var cameraEventToCommandBuffers))
            {
                return s_empty;
            }

            if (!cameraEventToCommandBuffers.TryGetValue(cameraEvent, out var commandBuffers))
            {
                return s_empty;
            }

            return commandBuffers;
#else
            return camera.GetCommandBuffers(cameraEvent);
#endif
        }
    }

    public static class UnityObjectExt
    {
        public static T FindAnyObjectByType<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        public static Object FindAnyObjectByType(System.Type type)
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindAnyObjectByType(type);
#else
            return Object.FindObjectOfType(type);
#endif
        }

        public static T[] FindObjectsByType<T>() where T : Object
        {
#if UNITY_6000_0_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<T>();
#endif
        }

        public static T[] FindObjectsByType<T>(bool includeInactive) where T : Object
        {

#if UNITY_6000_0_OR_NEWER
            return Object.FindObjectsByType<T>(
                includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<T>(includeInactive);
#endif
        }
    }

    public static class GraphicsSettingsExt
    {
        public static RenderPipelineAsset renderPipelineAsset
        {
            get
            {
#if UNITY_6000_0_OR_NEWER
                return GraphicsSettings.defaultRenderPipeline;
#else
                return GraphicsSettings.renderPipelineAsset;
#endif
            }
            set
            {
#if UNITY_6000_0_OR_NEWER
                GraphicsSettings.defaultRenderPipeline = value;
#else
                GraphicsSettings.renderPipelineAsset = value;
#endif

            }
        }
    }
}
