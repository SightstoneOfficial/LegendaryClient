﻿using Sightstone.Logic.SWF;
using Sightstone.Logic.SWF.SWFTypes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Sightstone.Patcher.Logic
{
    static class SWFImagePacks
    {
        public static void SWFextract(string pathIn, string pathOut)
        {
            var reader = new SWFReader(pathIn);
            var jpegs = reader.Tags.OfType<JPEG>();
            var lossless = reader.Tags.OfType<Lossless>();
            var symbols = reader.Tags.OfType<Symbols>();
            var binary = reader.Tags.OfType<Binary>();
            var imageInfo = new List<Images>();
            var losslessInfo = new List<Images>();
            if (binary.Count() > 0)
                getBinary(binary, pathOut);
            getJPEGs(imageInfo, jpegs);
            getLossless(imageInfo, lossless);
            var filenames = getFileNames(symbols, pathIn);

            imageInfo.ForEach(x => x.name = filenames[x.index]);


            foreach (var item in imageInfo)
            {
                if (!Directory.Exists(pathOut))
                    Directory.CreateDirectory(pathOut);
                if (item.type == "JPEG")
                    File.WriteAllBytes(Path.Combine(pathOut, item.name + ".jpg"), item.data);
                else if (item.type == "Lossless") //We have to create bitmap ourselves
                {

                    int[] bytesAsInts = item.data.Select(x => (int)x).ToArray();
                    var bmp = new Bitmap(item.width, item.height);
                    for (int y = 0; y < item.height; y++)
                    {
                        for (int x = 0; x < item.width; x++)
                        {
                            var color = Color.FromArgb(bytesAsInts[4 * (y * item.width + x)],
                                                       bytesAsInts[4 * (y * item.width + x) + 1],
                                                       bytesAsInts[4 * (y * item.width + x) + 2],
                                                       bytesAsInts[4 * (y * item.width + x) + 3]);
                            bmp.SetPixel(x, y, color);
                        }
                    }
                    bmp.Save(Path.Combine(pathOut, item.name) + ".bmp");
                }
            }
        }

        private static void getBinary(IEnumerable<Binary> binary, string pathOut)
        {
            foreach (var item in binary)
            {

                using (var binReader = new BinaryReader(new MemoryStream(item.Data)))
                {
                    binReader.BaseStream.Position = 12;
                    var bytes = binReader.ReadBytes(Convert.ToInt32(binReader.BaseStream.Length - binReader.BaseStream.Position));

                    File.WriteAllBytes(pathOut, bytes);
                }
            }
        }

        private static Dictionary<int, string> getFileNames(IEnumerable<Symbols> symbols, string pathIn)
        {
            var dictionary = new Dictionary<int, string>();
            foreach (var item in symbols)
            {
                using (var binReader = new BinaryReader(new MemoryStream(item.Data)))
                {
                    binReader.BaseStream.Position = 8;
                    while (binReader.BaseStream.Position < binReader.BaseStream.Length)
                    {
                        var key = binReader.ReadUInt16();
                        var value = new List<Byte>();
                        while (true)
                        {
                            var charByte = binReader.ReadByte();
                            if (charByte == 0)
                                break;
                            value.Add(charByte);
                        }
                        var fileName = Encoding.ASCII.GetString(value.ToArray());
                        fileName.Replace(Path.GetFileNameWithoutExtension(pathIn) + "_Embeds__e_", string.Empty);
                        if (dictionary.ContainsKey(key))
                            continue;
                        dictionary.Add(key, fileName);
                    }
                }
            }
            return dictionary;
        }

        //We have to create bitmap ourselves
        private static void getLossless(List<Images> imageInfo, IEnumerable<Lossless> lossless)
        {
            foreach (var item in lossless)
            {
                var image = new Images();
                using (var binReader = new BinaryReader(new MemoryStream(item.Data)))
                {
                    binReader.BaseStream.Position = 6;
                    image.index = binReader.ReadUInt16();
                    binReader.BaseStream.Position = 9;
                    image.width = binReader.ReadUInt16();
                    image.height = binReader.ReadUInt16();
                    var compressed = binReader.ReadBytes(Convert.ToInt32(binReader.BaseStream.Length - binReader.BaseStream.Position));
                    image.data = Ionic.Zlib.ZlibStream.UncompressBuffer(compressed);
                    image.type = "Lossless";
                }
                imageInfo.Add(image);
            }
        }

        private static void getJPEGs(List<Images> imageInfo, IEnumerable<JPEG> jpegs)
        {
            foreach (var item in jpegs)
            {
                var image = new Images();
                using (var binReader = new BinaryReader(new MemoryStream(item.Data)))
                {
                    binReader.BaseStream.Position = 6;
                    image.index = binReader.ReadUInt16();
                    image.data = binReader.ReadBytes(Convert.ToInt32(binReader.BaseStream.Length - binReader.BaseStream.Position));
                }
                image.type = "JPEG";
                imageInfo.Add(image);
            }
        }
    }
}
