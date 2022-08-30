using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace HL.CSharp.Wpf.Icons
{
internal class IconBitmapFramesCollection : List<BitmapFrame>
{

    public new void Add(BitmapFrame item)
    {
        if (item == null)
            throw new NullReferenceException("BitmapFrame cannot be null");
        if (item.PixelWidth > 256)
            throw new InvalidOperationException("The width of the frame cannot be greater than 256");
        if (item.PixelHeight > 256)
            throw new InvalidOperationException("The height of the frame cannot be greater than 256");
        if (item.PixelWidth < 16)
            throw new InvalidOperationException("The frame width cannot be less than 16");
        if (item.PixelHeight < 16)
            throw new InvalidOperationException("The frame height cannot be less than 16");
        if (item.PixelWidth != item.PixelHeight)
            throw new InvalidOperationException("BitmapFrame must be square");
        base.Add(item);
     }

    public new void Insert(int Index, BitmapFrame Item)
    {
        if (Item == null)
            throw new NullReferenceException("BitmapFrame cannot be null");
        if (Item.PixelWidth > 256)
            throw new InvalidOperationException("The width of the frame cannot be greater than 256");
        if (Item.PixelHeight > 256)
            throw new InvalidOperationException("The height of the frame cannot be greater than 256");
        if (Item.PixelWidth < 16)
            throw new InvalidOperationException("The frame width cannot be less than 16");
        if (Item.PixelHeight < 16)
            throw new InvalidOperationException("The frame height cannot be less than 16");
        if (Item.PixelWidth != Item.PixelHeight)
            throw new InvalidOperationException("BitmapFrame must be square");
        base.Insert(Index, Item);
    }

    public void SortDescending()
    {
        this.Sort(new Comparison<BitmapFrame>((BitmapFrame x, BitmapFrame y) =>
        {
            if (x.PixelWidth > y.PixelWidth)
            {
                return -1;
            }
            else if (x.PixelWidth < y.PixelWidth)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }));
    }

    public void SortAscending()
    {
        this.Sort(new Comparison<BitmapFrame>((BitmapFrame x, BitmapFrame y) =>
        {
            if (x.PixelWidth > y.PixelWidth)
            {
                return 1;
            }
            else if (x.PixelWidth < y.PixelWidth)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }));
    }

}
}
