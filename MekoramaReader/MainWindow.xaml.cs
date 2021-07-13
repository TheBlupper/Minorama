using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip.Compression;
using QRCodeEncoderLibrary;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Path = System.IO.Path;
using Substrate;
using Substrate.Core;

namespace MekoramaReader
{
    /// <summary>
    /// This mess of a window
    /// Couldn't bother to write my own Minecraft world reader and Substrate was the only one I found that worked
    /// Problem is it only works up until Minecraft version 1.8 so you have to use 1.7.10 or lower with this tool :/
    /// </summary>
    public partial class MainWindow : Window
    {
        // Uses the deflate algorithm, same with decompress (duh)
        public static byte[] Compress(byte[] buffer)
        {
            using (MemoryStream inStream = new MemoryStream(buffer))
            {
                MemoryStream outStream = new MemoryStream();
                Deflater d = new Deflater(5);
                DeflaterOutputStream compressStream = new DeflaterOutputStream(outStream, d);
                int mSize;
                byte[] mWriteData = new byte[4096 * 2];
                while ((mSize = inStream.Read(mWriteData, 0, 4096)) > 0)
                {
                    compressStream.Write(mWriteData, 0, mSize);
                }
                compressStream.Finish();
                inStream.Close();
                return new byte[] { 120, 1 }.Concat(outStream.ToArray().Skip(2).ToArray()).ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            byte[] decompressedData = new byte[10000];
            int decompressedLength;
            using (MemoryStream memory = new MemoryStream(data))
            using (InflaterInputStream inflater = new InflaterInputStream(memory))
                decompressedLength = inflater.Read(decompressedData, 0, decompressedData.Length);
            return decompressedData;
        }


        void ConvertWorld(string path, int cx, int cy, int cz)
        {
            NbtWorld map = NbtWorld.Open(path);
            IChunk chunk = map.GetChunkManager().GetChunk(cx, cz);
            MekoWorld mekoWorld = new MekoWorld();

            for (int z = 0; z < 16; z++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        // Substrate doesn't use vertical chunks so I do it manually by adding cy
                        AlphaBlock block = chunk.Blocks.GetBlock(x, y + cy, z);
                        MekoBlock mekoBlock = new MekoBlock(Lookup.MinecraftToMekoramaID(block.ID));
                        mekoBlock.orientation = Lookup.MinecraftToMekoramaOrientation(int.Parse(block.Data.ToString()));
                        mekoWorld.SetBlock(x, y, z, mekoBlock);
                    }
                }
            }
            byte[] saved = mekoWorld.SaveWorld();
            saved = Compress(saved);

            byte[] header = new byte[] { 2, 19, 13, 252 };
            saved = header.Concat(saved).ToArray();

            QREncoder Encoder = new QREncoder();
            Encoder.ErrorCorrection = ErrorCorrection.Q;
            Encoder.ModuleSize = 4;
            Encoder.QuietZone = 16;

            Encoder.Encode(saved);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG file (*.png)|*.png"; ;
            if (saveFileDialog.ShowDialog() == true)
            {
                Encoder.SaveQRCodeToPngFile(saveFileDialog.FileName);

            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        void Convert(object sender, RoutedEventArgs e)
        {
            if(worldPath != null)
            {
                int cx = int.Parse(xTextbox.Text);
                int cy = int.Parse(yTextbox.Text);
                int cz = int.Parse(zTextbox.Text);
                ConvertWorld(worldPath, cx, cy, cz);

            }
        }

        bool ValidateWorldPath(string path)
        {
            bool dat = File.Exists(Path.Combine(path, "level.dat"));
            bool region = Directory.Exists(Path.Combine(path, "region/"));

            return dat && region;
        }

        string worldPath = null;

        public void SelectWorld(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Select world folder";
            dialog.UseDescriptionForTitle = true;
            dialog.RootFolder = System.Environment.SpecialFolder.ApplicationData;

            if (ValidateWorldPath(dialog.SelectedPath))
            {
                worldName.Content = Path.GetFileName(dialog.SelectedPath);
                worldPath = dialog.SelectedPath;
            } else
            {
                MessageBox.Show(this, "Not a minecraft world", "Error");
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("/^[a-zA-Z\\d\\s-]*$/");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}

