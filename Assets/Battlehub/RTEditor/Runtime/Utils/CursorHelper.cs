using UnityEngine;
using System.Collections.Generic;
using System;

namespace Battlehub.Utils
{
    public enum KnownCursor
    {
        VResize,
        HResize,
        DropNotAllowed,
        DropAllowed,
        None,
    }

    public class CursorHelper
    {
        private object m_lock;
        private Texture2D m_texture;

        private readonly Dictionary<KnownCursor, Texture2D> m_knownCursorToTexture = new Dictionary<KnownCursor, Texture2D>();

        [Obsolete("Renamed to SetCursorTexture")] //13.11.2020
        public void Map(KnownCursor cursorType, Texture2D texture)
        {
            SetCursorTexture(cursorType, texture);
        }

        [Obsolete("Renamed to ClearCursorTextures")] //13.11.2020
        public void Reset()
        {
            ClearCursorTextures();
        }

        public void SetCursorTexture(KnownCursor cursorType, Texture2D texture)
        {
            m_knownCursorToTexture[cursorType] = texture;
        }

        public void ClearCursorTextures()
        {
            m_knownCursorToTexture.Clear();
        }

        private Texture2D m_defaultCursorTexture;
        private Vector2 m_defaultCursorHotspot;
        public Texture2D DefaultCursorTexture
        {
            get 
            {
                return m_defaultCursorTexture;
            }
        }
        public Vector2 DefaultCursorHotspot
        {
            get 
            {
                return m_defaultCursorHotspot;
            }
        }
        public void SetDefaultCursor(Texture2D texture, Vector2 hotspot)
        {
            m_defaultCursorTexture = texture;
            m_defaultCursorHotspot = hotspot;
            ResetCursor(null);
        }

        public bool SetCursor(object locker, KnownCursor cursorType)
        {
            return SetCursor(locker, cursorType, new Vector2(0.5f, 0.5f), CursorMode.Auto);
        }

        public bool SetCursor(object locker, KnownCursor cursorType, Vector2 hotspot, CursorMode mode)
        {
            Texture2D texture;
            if(!m_knownCursorToTexture.TryGetValue(cursorType, out texture))
            {
                texture = null;
            }
            return SetCursor(locker, texture, hotspot, mode);
        }

        public bool SetCursor(object locker, Texture2D texture)
        {
            return SetCursor(locker, texture, new Vector2(0.5f, 0.5f), CursorMode.Auto);
        }

        public bool SetCursor(object locker, Texture2D texture, Vector2 hotspot, CursorMode mode)
        {
            if (m_lock != null && m_lock != locker)
            {
                return false;
            }

            if (texture != null)
            {
                hotspot = new Vector2(texture.width * hotspot.x, texture.height * hotspot.y);
            }
            else
            {
                texture = DefaultCursorTexture;
                if (texture != null)
                {
                    hotspot = new Vector2(texture.width * DefaultCursorHotspot.x, texture.height * DefaultCursorHotspot.y);
                }
            }

            m_lock = locker;
            if (m_texture != texture)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                Cursor.SetCursor(texture, hotspot, mode);
                m_texture = texture;
                return true;
            }

            return false;
        }

        public void ResetCursor(object locker)
        {            
            if (m_lock != locker)
            {
                return;
            }
            m_lock = null;
            SetCursor(null, DefaultCursorTexture, DefaultCursorHotspot, CursorMode.Auto);
        }
    }

}
