#if DEBUGMONITOR
#region File Description
//-----------------------------------------------------------------------------
// SpriteFontControl.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FE7x.Debug_Monitor
{
    /// <summary>
    /// Example control inherits from GraphicsDeviceControl, which allows it to
    /// render using a GraphicsDevice. This control shows how to use ContentManager
    /// inside a WinForms application. It loads a SpriteFont object through the
    /// ContentManager, then uses a SpriteBatch to draw text. The control is not
    /// animated, so it only redraws itself in response to WinForms paint messages.
    /// </summary>
    class DebugMonitorControl : GraphicsDeviceControl
    {
        ContentManager content;
        SpriteBatch spriteBatch;

        /// <summary>
        /// Initializes the control, creating the ContentManager
        /// and using it to load a SpriteFont.
        /// </summary>
        protected override void Initialize()
        {
            content = new ContentManager(Services, "Content");

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }


        /// <summary>
        /// Disposes the control, unloading the ContentManager.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                content.Unload();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Draws the control, using SpriteBatch and SpriteFont.
        /// </summary>
        protected override void Draw()
        {
            GraphicsDevice.Clear(Color.Beige);

            FEXNA.Global.debug_monitor.draw(spriteBatch, content);
        }

        internal void change_tab(int index)
        {
            FEXNA.Global.debug_monitor.change_page(index);
        }

        internal void change_variable_group(int group)
        {
            FEXNA.Global.debug_monitor.change_variable_group(group);
        }

        internal void reseed_rng()
        {
            FEXNA.Global.reseed_rng();
        }

        internal void open_debug_menu()
        {
            FEXNA.Global.open_debug_menu();
        }
    }
}
#endif