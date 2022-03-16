using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Cores.Helpers
{
    public static class ImageHelper
    {
        public static List<string> ImageFileExtentionList = new List<string>() { "jpg", "jpeg", "jfif", "pjpeg", "pjp", "png", "bmp", "gif" };
        public static bool IsImage(string fileExtension)
        {
            return ImageFileExtentionList.Contains(fileExtension); 
        }

        public static byte[] MakeThumbnail(byte[] myImage, int thumbWidth, int thumbHeight)
        {
            //Skip check
            if (myImage == null || myImage.Length == 0) return new byte[0];

            //Create image
            Image image = Image.FromStream(new MemoryStream(myImage));
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            Image thumbnail = new Bitmap(thumbWidth, thumbHeight); // changed parm names
            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(thumbnail);

            //graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.InterpolationMode = InterpolationMode.Bicubic;
            graphic.SmoothingMode = SmoothingMode.HighSpeed;
            graphic.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            //graphic.CompositingQuality = CompositingQuality.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighSpeed;
            /* ------------------ new code --------------- */

            // Figure out the ratio
            double ratioX = (double)thumbWidth / (double)originalWidth;
            double ratioY = (double)thumbHeight / (double)originalHeight;
            // use whichever multiplier is smaller
            double ratio = ratioX < ratioY ? ratioX : ratioY;

            // now we can get the new height and width
            int newHeight = Convert.ToInt32(originalHeight * ratio);
            int newWidth = Convert.ToInt32(originalWidth * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero)
            int posX = Convert.ToInt32((thumbWidth - (originalWidth * ratio)) / 2);
            int posY = Convert.ToInt32((thumbHeight - (originalHeight * ratio)) / 2);

            graphic.Clear(Color.White); // white padding
            graphic.DrawImage(image, posX, posY, newWidth, newHeight);

            /* ------------- end new code ---------------- */
            MemoryStream ms = new MemoryStream();
            thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }


    }
}
