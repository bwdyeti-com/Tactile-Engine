using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.IO;

namespace ContentWriterExtension
{
    [ContentProcessor]
    class TextureProcessorPalette : TextureProcessor
    {
        const int BATTLER_WIDTH = 192;

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            string[] strs = context.OutputFilename.Split('\\');
            /* // If assigning this processor to all textures, use something like this to only apply to certain folders
            if (strs.Length > 2 && strs[strs.Length - 2] == "Animations")
            { } */
            //write_debug(context.OutputDirectory);
            //write_debug(context.OutputFilename);

            bool battler = (strs.Length > 2 && strs[strs.Length - 2] == "Battlers");

            Dictionary<Color, int> palette = new Dictionary<Color,int>();

            Dictionary<string, Color[]> palette_data = context.BuildAndLoadAsset<string, Dictionary<string, Color[]>>(new ExternalReference<string>(@"BattlerPalettes.xml"), "");
            string filename = Path.GetFileNameWithoutExtension(context.OutputFilename);
            
            if (!palette_data.ContainsKey(filename))
                filename = filename.Split('-')[0];
            if (palette_data.ContainsKey(filename))
            {
                for (int i = 0; i < palette_data[filename].Length; i++)
                {
                    if (!palette.ContainsKey(palette_data[filename][i]))
                        palette.Add(palette_data[filename][i], i);
                }
            }
            // The process for getting to this point has changed, so now if there is no palette entry for a texture it shouldn't be edited at all
            else
                return base.Process(input, context);

            // Otherwise we have palette data and the image will be remapped to color indices on the red channel
            input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            foreach (MipmapChain image_face in input.Faces)
            {
                for (int i = 0; i < image_face.Count; i++)
                {
                    PixelBitmapContent<Color> mip = (PixelBitmapContent<Color>)image_face[i];
                    Color original, edited;

                    // Go through the whole image
                    for (int y = 0; y < mip.Height; y++)
                    {
                        for (int x = 0; x < mip.Width; x++)
                        {
                            original = mip.GetPixel(x, y);
                            // If this color isn't already in the palette, /leave the color alone/
                            // This is a new change to leave unpaletted colors alone
                            if (original.A > 0 && palette.ContainsKey(original))
                            {
                                edited = new Color((palette[original] * 1) % 256, 0, 0, original.A);
                                mip.SetPixel(x, y, edited);
                            }
                        }
                    }
                }
            }
            /*
            if (false)
            {
                //write_debug(originalFilename + ", " + palette.Count.ToString());
                write_debug(originalFilename + ", " + new_colors.ToString() + "/" + palette.Count.ToString());
                for (int i = base_count; i < palette.Count; i++)
                    foreach (KeyValuePair<Color, int> entry in palette)
                        if (entry.Value == i)
                        {
                            if (entry.Key.A > 0)
                                write_debug(entry.Key.ToString());
                            break;
                        }
            }*/

            return base.Process(input, context);
        }

        protected void write_debug(string text)
        {
            using (StreamWriter writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\palette test.txt", true))
            {
                writer.WriteLine(text);
            }
        }
    }
}