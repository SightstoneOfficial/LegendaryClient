using Sightstone.Logic.SWF;
using Sightstone.Logic.SWF.SWFTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sightstone.Patcher.Logic
{
    class SWFImagePacks
    {
        public static void SWFextract(string pathIn, string pathOut)
        {
            var reader = new SWFReader(pathIn);
            var jpegs = reader.Tags.OfType<JPEG>();
            var lossless = reader.Tags.OfType<Lossless>();
            var symbols = reader.Tags.OfType<Symbols>();
            var imageInfo = new List<Images>();
            var losslessInfo = new List<Images>();


            getJPEGs(imageInfo, jpegs);
            getLossless(imageInfo, lossless);
            var filenames = getFileNames(symbols);

            imageInfo.ForEach(x => x.name = filenames[x.index]);


            foreach (var item in imageInfo)
            {
                if(item.Type == "JPEG")
                    File.WriteAllBytes(Path.Combine(pathOut, item.name + ".jpg"), item.data);
                else
                    File.WriteAllBytes(Path.Combine(pathOut, item.name), item.data);
            }
        }

        private static Dictionary<int, string> getFileNames(IEnumerable<Symbols> symbols)
        {
            var dictionary = new Dictionary<int, string>();
            foreach (var item in symbols)
            {
                using (var binReader = new BinaryReader(new MemoryStream(item.Data)))
                {
                    binReader.BaseStream.Position = 8;
                    while (binReader.BaseStream.Position < binReader.BaseStream.Length)
                    {
                        var key = binReader.ReadByte();
                        binReader.ReadByte();
                        var value = new List<Byte>();
                        while (true)
                        {
                            var charByte = binReader.ReadByte();
                            if (charByte == 0)
                                break;
                            value.Add(charByte);
                        }
                        var fileName = Encoding.ASCII.GetString(value.ToArray());
                        dictionary.Add(key, fileName);
                    }
                }
            }
            return dictionary;
        }

        //TODO: need to extract them somehow
        private static void getLossless(List<Images> imageInfo, IEnumerable<Lossless> lossless)
        {
            foreach (var item in lossless)
            {
                var image = new Images();
                using (var binReader = new BinaryReader(new MemoryStream(item.Data)))
                {
                    binReader.BaseStream.Position = 6;
                    image.index = binReader.Read();
                    binReader.BaseStream.Position = 13;
                    image.data = binReader.ReadBytes(Convert.ToInt32(binReader.BaseStream.Length - binReader.BaseStream.Position));
                }
                image.Type = "Lossless";
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
                    image.index = binReader.Read();
                    if (binReader.ReadByte() == 0)
                    {
                        image.data = binReader.ReadBytes(Convert.ToInt32(binReader.BaseStream.Length - binReader.BaseStream.Position));
                        break;
                    }
                }
                image.Type = "JPEG";
                imageInfo.Add(image);
            }
        }
    }
}
