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
    class TextureProcessorFacePalette : TextureProcessor
    {
        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            string[] strs = context.OutputFilename.Split('\\');
            /* // If assigning this processor to all textures, use something like this to only apply to certain folders
            if (strs.Length > 2 && strs[strs.Length - 2] == "Animations")
            { } */
            //write_debug(context.OutputDirectory);
            //write_debug(context.OutputFilename);

            Dictionary<Color, int> palette = new Dictionary<Color, int>();

            Dictionary<string, Color[]> palette_data = context.BuildAndLoadAsset<string, Dictionary<string, Color[]>>(
                new ExternalReference<string>(@"Face_Palette_Data.xml"), "");
            string filename = Path.GetFileNameWithoutExtension(context.OutputFilename);
            string doop = filename;
            // If palette data for the face exists
            if (palette_data.ContainsKey(filename) && palette_data[filename].Length > 0)
            {
                // Add the colors to a dictionary
                for (int i = 0; i < palette_data[filename].Length; i++)
                    if (!palette.ContainsKey(palette_data[filename][i]))
                        palette.Add(palette_data[filename][i], i);
            }
            // The process for getting to this point has changed, so now if there is no palette entry for a texture it shouldn't be edited at all
            else
            {
                return base.Process(input, context);
            }

            // Otherwise we have palette data and the image will be remapped to color indices on the red channel
            input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            int base_count = palette.Count;
            int new_colors = 0;
            foreach (MipmapChain image_face in input.Faces)
            {
                for (int i = 0; i < image_face.Count; i++)
                {
                    PixelBitmapContent<Color> mip = (PixelBitmapContent<Color>)image_face[i];
                    Color original, edited;

                    for (int y = 0; y < mip.Height; y++)
                    {
                        for (int x = 0; x < mip.Width; x++)
                        {
                            original = mip.GetPixel(x, y);
                            // Checks for new colors not in the palette data
                            // Why would those exist? //Yeti
                            if (!palette.ContainsKey(original))
                            {
                                // Gets the smallest whole number that is not already in the palette values
                                int index = 0;
                                while (palette.Values.ToList().Contains(index))
                                    index++;
                                // Adds a new color
                                palette.Add(original, index);
                                // Keeps track of how many new colors have been added
                                new_colors++;
                            }
                            edited = new Color((palette[original] * 1) % 256, 0, 0, original.A);
                            mip.SetPixel(x, y, edited);
                        }
                    }
                }
            }

            if (false)
            {
                //write_debug(doop + ", " + palette.Count.ToString());
                write_debug(doop + ", " + new_colors.ToString() + "/" + palette.Count.ToString());
                for (int i = base_count; i < palette.Count; i++)
                    foreach (KeyValuePair<Color, int> entry in palette)
                        if (entry.Value == i)
                        {
                            if (entry.Key.A > 0)
                                write_debug(entry.Key.ToString());
                            break;
                        }
            }

            return base.Process(input, context);
        }

        protected void write_debug(string text)
        {
            using (StreamWriter writer = new StreamWriter(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\palette test.txt", true))
            {
                writer.WriteLine(text);
            }
        }
    }
}