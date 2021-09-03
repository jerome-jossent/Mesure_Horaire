using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace HL.CSharp.Wpf.Icons
{
public class IconBitmapEncoder : System.Windows.Threading.DispatcherObject
{

    #region "Constructor"

    public IconBitmapEncoder()
    {
        _frames = new IconBitmapFramesCollection();
    }

    #endregion

    #region "Fields"


    private IconBitmapFramesCollection _frames;
    #endregion

    #region "Properties"

    public IList<BitmapFrame> Frames
    {
        get { return _frames; }
    }

    #endregion

    #region "Methods"

    public void Save(System.IO.Stream Stream)
    {
        _frames.SortAscending();
        System.IO.BinaryWriter writer = new System.IO.BinaryWriter(Stream, System.Text.Encoding.UTF32);

        ushort FramesCount = Convert.ToUInt16(_frames.Count);
        ushort FileHeaderLength = 6;
        ushort FrameHeaderLength = 16;

        ICONDIR FileHeader = new ICONDIR(FramesCount);
        writer.Write(FileHeader.idReserved);
        writer.Write(FileHeader.idType);
        writer.Write(FileHeader.idCount);

        byte[][] data = new byte[FramesCount][];

        foreach (BitmapFrame Frame in _frames)
        {
            int FrameIndex = _frames.IndexOf(Frame);
            if (Frame.PixelWidth == 256)
            {
                data[FrameIndex] = GetPNGData(Frame);
            }
            else
            {
                data[FrameIndex] = GetBMPData(Frame);
            }
        }

        uint FrameDataOffset = FileHeaderLength;
        FrameDataOffset += (uint)(FrameHeaderLength * FramesCount);

        foreach (BitmapFrame Frame in _frames)
        {
            int FrameIndex = _frames.IndexOf(Frame);
            if (FrameIndex > 0)
            {
                FrameDataOffset += Convert.ToUInt32(data[FrameIndex - 1].Length);
            }
            ICONDIRENTRY FrameHeader = new ICONDIRENTRY((ushort)Frame.PixelWidth, (ushort)Frame.PixelHeight, Convert.ToUInt16(Frame.Format.BitsPerPixel), Convert.ToUInt32(data[FrameIndex].Length), FrameDataOffset);
            writer.Write(FrameHeader.bWidth);
            writer.Write(FrameHeader.bHeight);
            writer.Write(FrameHeader.bColorCount);
            writer.Write(FrameHeader.bReserved);
            writer.Write(FrameHeader.wPlanes);
            writer.Write(FrameHeader.wBitCount);
            writer.Write(FrameHeader.dwBytesInRes);
            writer.Write(FrameHeader.dwImageOffset);
        }

        foreach (byte[] FrameData in data)
        {
            writer.Write(FrameData);
        }

    }

    private byte[] GetPNGData(BitmapFrame Frame)
    {
        System.IO.MemoryStream DataStream = new System.IO.MemoryStream();
        PngBitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(Frame);
        encoder.Save(DataStream);
        encoder = null;
        byte[] Data = DataStream.GetBuffer();
        DataStream.Close();
        return Data;
    }

    private byte[] GetBMPData(BitmapFrame Frame)
    {
        System.IO.MemoryStream DataStream = new System.IO.MemoryStream();
        BmpBitmapEncoder encoder = new BmpBitmapEncoder();
        encoder.Frames.Add(Frame);
        encoder.Save(DataStream);
        encoder = null;
        DataStream.Position = 14;
        System.IO.BinaryReader DataStreamReader = new System.IO.BinaryReader(DataStream, System.Text.UTF32Encoding.UTF32);
        System.IO.MemoryStream OutDataStream = new System.IO.MemoryStream();
        System.IO.BinaryWriter OutDataStreamWriter = new System.IO.BinaryWriter(OutDataStream, System.Text.UTF32Encoding.UTF32);
        OutDataStreamWriter.Write(DataStreamReader.ReadUInt32());
        OutDataStreamWriter.Write(DataStreamReader.ReadInt32());
        int height = DataStreamReader.ReadInt32();
        if (height > 0)
        {
            height = height * 2;
        }
        else if (height < 0)
        {
            height = -(height * 2);
        }
        else
        {
            height = 0;
        }
        OutDataStreamWriter.Write(height);
        for (int i = 26; i <= DataStream.Length - 1; i++)
        {
            OutDataStream.WriteByte((byte)(DataStream.ReadByte()));
        }
        byte[] data = OutDataStream.GetBuffer();
        OutDataStreamWriter.Close();
        OutDataStream.Close();
        DataStreamReader.Close();
        DataStream.Close();
        return data;
    }

    #endregion

    #region "Icon File Structures"

    private struct ICONDIR
    {

        //Reserved (must be 0)
        public readonly ushort idReserved;
        //Resource Type (1 for icons)
        public readonly ushort idType;
        //How many images?

        public readonly ushort idCount;
        public ICONDIR(ushort Count)
        {
            idReserved = Convert.ToUInt16(0);
            idType = Convert.ToUInt16(1);
            idCount = Count;
        }

    }

    private struct ICONDIRENTRY
    {

        //Width, in pixels, of the image
        public readonly byte bWidth;
        //Height, in pixels, of the image
        public readonly byte bHeight;
        //Number of colors in image (0 if >=8bpp)
        public readonly byte bColorCount;
        //Reserved ( must be 0)
        public readonly byte bReserved;
        //Color Planes
        public readonly ushort wPlanes;
        //Bits per pixel
        public readonly ushort wBitCount;
        //How many bytes in this resource?
        public readonly uint dwBytesInRes;
        //Where in the file is this image?

        public readonly uint dwImageOffset;
        public ICONDIRENTRY(ushort Width, ushort Height, ushort BitsPerPixel, uint ResSize, uint ImageOffset)
        {
            if (Width == 256)
            {
                bWidth = Convert.ToByte(0);
            }
            else
            {
                bWidth = Convert.ToByte(Width);
            }
            if (Height == 256)
            {
                bHeight = Convert.ToByte(0);
            }
            else
            {
                bHeight = Convert.ToByte(Height);
            }
            if (BitsPerPixel == 4)
            {
                bColorCount = Convert.ToByte(16);
            }
            else
            {
                bColorCount = Convert.ToByte(0);
            }
            bReserved = Convert.ToByte(0);
            wPlanes = Convert.ToUInt16(1);
            wBitCount = BitsPerPixel;
            dwBytesInRes = ResSize;
            dwImageOffset = ImageOffset;
        }

    }

    #endregion

    #region "Helpers"

    public static BitmapSource Get4BitImage(BitmapSource Source)
    {
        FormatConvertedBitmap @out = new FormatConvertedBitmap(Source, PixelFormats.Indexed4, BitmapPalettes.Halftone8, 0);
        return @out;
    }

    public static BitmapSource Get8BitImage(BitmapSource Source)
    {
        FormatConvertedBitmap @out = new FormatConvertedBitmap(Source, PixelFormats.Indexed8, BitmapPalettes.Halftone256, 0);
        return @out;
    }

    public static BitmapSource Get24plus8BitImage(BitmapSource Source)
    {
        FormatConvertedBitmap @out = new FormatConvertedBitmap(Source, PixelFormats.Pbgra32, null, 0);
        return @out;
    }

    public static BitmapSource GetResized(BitmapSource Source, int Size)
    {
        BitmapSource backup = Source.Clone();
        try
        {
            TransformedBitmap scaled = new TransformedBitmap();
            scaled.BeginInit();
            scaled.Source = Source;
            double scX = (double)Size / (double)Source.PixelWidth;
            double scy = (double)Size / (double)Source.PixelHeight;
            ScaleTransform tr = new ScaleTransform(scX, scy, Source.Width / 2, Source.Height / 2);
            scaled.Transform = tr;
            scaled.EndInit();
            Source = scaled;
        }
        catch (Exception)
        {
            Source = backup;
        }
        return Source;
    }

    #endregion
}
}
