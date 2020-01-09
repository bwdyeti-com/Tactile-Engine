using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ContentManagers
{
    /// <summary>
    /// Provide a content manager that can be used in multiple threads,
    /// but only tries to load one asset at a time. It will block the
    /// other threads trying to load assets until the current asset
    /// load is completed.
    /// </summary>
    class ThreadSafeContentManager : ContentManager
    {
        static object loadLock = new object();

#if DEBUG
#if MONOGAME
        new
#endif
        private System.Collections.Generic.HashSet<string> LoadedAssets =
            new System.Collections.Generic.HashSet<string>();
#endif

        public ThreadSafeContentManager(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        public ThreadSafeContentManager(IServiceProvider serviceProvider, string rootDirectory)
            : base(serviceProvider, rootDirectory) { }

        public override T Load<T>(string assetName)
        {
            lock (loadLock)
            {
                try
                {
#if DEBUG
                    if (!LoadedAssets.Contains(assetName))
                        Console.WriteLine(string.Format("Loading \"{0}\"", assetName));
                    LoadedAssets.Add(assetName);
#endif
                    return base.Load<T>(assetName);
                }
                catch (ContentLoadException ex) { throw; }
            }
        }

        /*internal T DefaultLoad<T>(string assetName) //Debug
        {
            return base.Load<T>(assetName);
        }*/

        /// <summary>
        /// Creates a Texture2D from the given stream, synchronized with the content loading to avoid thread conflicts.
        /// </summary>
        /// <param name="stream">Stream containing the .png or .jpg</param>
        /// <returns>Texture created from the stream</returns>
        public Texture2D FromStream(Stream stream)
        {
            lock (loadLock)
            {
                IGraphicsDeviceService graphicsDeviceService = (IGraphicsDeviceService)ServiceProvider.GetService(typeof(IGraphicsDeviceService));
                return Texture2D.FromStream(graphicsDeviceService.GraphicsDevice, stream);
            }
        }

        public Texture2D texture_from_size(int width, int height)
        {
            lock (loadLock)
            {
                IGraphicsDeviceService graphicsDeviceService = (IGraphicsDeviceService)ServiceProvider.GetService(typeof(IGraphicsDeviceService));
                return new Texture2D(graphicsDeviceService.GraphicsDevice, width, height);
            }
        }
    }
}