using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MekoramaReader
{
    public class Lookup
    {

        public static string LookupBlock(int id)
        {
            // An underscore after the name means it can be rotated, and therefore requires an extra byte
            // Apparently 67 is the maximum ID before it thinks the data is corrupt. Basically everything after 52 is unused and glitchy content, I took some liberty with the naming
            // I did this in a weekend and didn't have time to implement all the special blocks, so the rotation of some things get messed up
            switch (id)
            {
                case 1: return "Stone";
                case 2: return "Bricks";
                case 3: return "Stone";
                case 4: return "Green Goal";
                case 5: return "Stair_";
                case 6: return "Trash";
                case 7: return "Stone Wedge_";
                case 8: return "Dirt Wedge_";
                case 9: return "Yellow Ball";
                case 10: return "Weird Goal";
                case 12: return "Grass";
                case 13: return "Glitched Pillar";
                case 14: return "Stone Rounded_";
                case 15: return "Bee__";
                case 16: return "Zapper_";
                case 17: return "Draggable";
                case 18: return "Dirt";
                case 19: return "GLITCHED";
                case 20: return "Metal Stair_";
                case 21: return "Metal Rounded_";
                case 22: return "Motor_";
                case 23: return "Metal Crate";
                case 24: return "Stone";
                case 25: return "Metal";
                case 26: return "Robot Turns Right__";
                case 27: return "Eye";
                case 28: return "Air";
                case 29: return "GLITCHED";
                case 32: return "Metal Half Pillar_";
                case 33: return "Slider Rail_";
                case 34: return "Stone Half Pillar_";
                case 35: return "Stone Pillar_";
                case 36: return "Draggable Pillar_";
                case 37: return "Ball";
                case 38: return "Stone";
                case 39: return "Metal Pillar_";
                case 40: return "Metal Crate";
                case 41: return "Slider_";
                case 42: return "Metal Crate";
                case 43: return "Fence_";
                case 44: return "Metal Crate";
                case 45: return "Metal Crate";
                case 46: return "Metal Crate";
                case 47: return "Draggable Metal Crate";
                case 48: return "Metal Crate";
                case 49: return "Metal Wedge_";
                case 50: return "Robot Turns Left__";
                case 51: return "Star";
                case 52: return "Dirt";
                case 53: return "Metal Frame";
                case 54: return "Metal Frame";
                case 55: return "S-Block Light_";
                case 56: return "S-Block Dark_";
                case 57: return "Brick Motor_";
                case 58: return "Candy Cane Motor_";
                case 59: return "Candy Cane Motor_";
                case 60: return "Glitched Motor_";
                case 61: return "Thin Rail_";
                case 62: return "Thin Rail Curved_";
                case 63: return "Thin Rail T_";
                case 64: return "Letter Block_";
                case 65: return "Blue Motor_";
                case 66: return "Undraggable Metal Crate_";
                case 67: return "Zapper Motor_";
                default: return "Air";
            }
        }

        public static int MinecraftToMekoramaID(int id)
        {
            switch (id)
            {
                case 0: return 0; // Air
                case 1: return 1; // Stone
                case 2: return 12; // Grass
                case 3: return 52; // Dirt
                case 33: return 22; // Piston - Motor (orientation is a bit messed up)
                case 67: return 5; // Cobble stairs - Stone stairs
                default: return 0; 
            }
        }

        public static int MinecraftToMekoramaOrientation(int orientation)
        {
            // Tried some stuff but it wasn't consitent, best of luck
            switch (orientation)
            {
                case 0: return 3;
                case 1: return 1;
                case 2: return 2;
                case 3: return 0;
                case 4: return 7;
                case 5: return 5;
                case 6: return 6;
                case 7: return 4;
                default: return 0;
            }
        }
    }

    public class MekoBlock
    {
        public int id = 0;
        public string name;
        public int orientation = 0;
        public bool rotatable = false;

        public MekoBlock(Stream dataStream = null)
        {
            if (dataStream != null)
                id = dataStream.ReadByte();

            string blockInfo = Lookup.LookupBlock(id);
            name = blockInfo.TrimEnd(new char[] { '_' });

            if (blockInfo.EndsWith("_"))
            {
                rotatable = true;

                if (dataStream != null)
                    orientation = dataStream.ReadByte();
            }
        }

        public MekoBlock(int _id)
        {
            id = _id;
            string blockInfo = Lookup.LookupBlock(id);
            name = blockInfo.TrimEnd(new char[] { '_' });
            if (blockInfo.EndsWith("_"))
                rotatable = true;
        }
    }
    public class MekoWorld
    {
        private MekoBlock[,,] blocks = new MekoBlock[16, 16, 16];
        private Stream dataStream;

        public string levelName;
        public string authorName;

        public MekoWorld(byte[] data = null)
        {
            dataStream = data != null ? new MemoryStream(data) : null;

            Array.Clear(blocks, 0, blocks.Length);

            if (data != null)
                ReadWorld();
            else
            {
                levelName = "New Level";
                authorName = "Unknown Author";
            }
        }

        byte[] ReadBytes(int count)
        {
            int[] result = new int[count];
            while (count > 0)
            {
                result[result.Length - count] = dataStream.ReadByte();
                count -= 1;
            }
            return result.Select(x => (byte)x).ToArray();
        }

        string ReadString()
        {
            int length = (int)dataStream.ReadByte();
            return Encoding.UTF8.GetString(ReadBytes(length));
        }

        void ReadWorld()
        {
            levelName = ReadString();
            authorName = ReadString();
            
            int i = 0;
            while (dataStream.Position < dataStream.Length && i < 16 * 16 * 16) // Makes sure we dont over-extend and loop around the coordinates
            {
                int x = i % 16;
                int y = ((int)i / 16) % 16;
                int z = ((int)i / 256) % 16;

                blocks[x, y, z] = new MekoBlock(dataStream);
;
                i += 1;

            }
        }

        public MekoBlock GetBlock(int x, int y, int z)
        {
            return blocks[x, y, z];
        }

        public void SetBlock(int x, int y, int z, MekoBlock block)
        {
            blocks[x, y, z] = block;
        }

        public byte[] SaveWorld()
        {
            List<byte> result = new List<byte>();

            // Write level name
            result.Add((byte)levelName.Length);
            result.AddRange(Encoding.Default.GetBytes(levelName));

            // Write author name
            result.Add((byte)authorName.Length);
            result.AddRange(Encoding.Default.GetBytes(authorName));

            // Write all blocks
            for (int z = 0; z < 16; z++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        result.Add((byte)blocks[x, y, z].id);

                        // Write an extra byte with orientation if needed
                        if (blocks[x, y, z].rotatable)
                        { result.Add((byte)blocks[x, y, z].orientation); }
                    }
                }
            }
            result.AddRange(new byte[10000 - result.Count]);
            return result.ToArray();
        }
    }
}
