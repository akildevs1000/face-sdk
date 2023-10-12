namespace FCardProtocolAPI.Common
{
    public class ImageTool
    {
        public static byte[] ConvertImage(byte[] bData, float minWidth = 480, float maxHeight = 640, int imageSizeMax = 150 * 1024)
        {
            using MemoryStream bImage = new MemoryStream(bData);
            var result = ConvertImage(bImage, minWidth, maxHeight, imageSizeMax);
            if (result == null)
            {
                result = bData;
            }
            return result;
        }

        public static byte[] ConvertImage(MemoryStream bData, float minWidth, float maxHeight, int imageSizeMax = 150 * 1024)
        {
            using var img = Image.Load(bData);
            float rate = 1;
            if (!CheckSize(img, minWidth, maxHeight, bData.Length, imageSizeMax, ref rate))
            {
                return null;
            }
            int iWidth = img.Width, iHeight = img.Height;
            iWidth = (int)(iWidth * rate);
            iHeight = (int)(iHeight * rate);
            var newimage = img.Clone(i =>
            {
                i.AutoOrient();
                i.Resize(iWidth, iHeight);
            });
            var quality = 100;
            byte[] result = DeleteriouQualitys(newimage, imageSizeMax);


            return result;
        }
        /// <summary>
        /// 检查图片大小
        /// </summary>
        /// <param name="img"></param>
        /// <param name="dataLen"></param>
        /// <param name="maxSize"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        private static bool CheckSize(Image img, float minWidth, float maxHeight, long dataLen, long maxSize, ref float rate)
        {
            if (img.Width > minWidth || img.Height > maxHeight || dataLen > maxSize)
            {
                float rate1, rate2;

                rate1 = minWidth / img.Width;
                rate2 = maxHeight / img.Height;
                rate = rate1 > rate2 ? rate2 : rate1;
                if (rate > 1) rate = 1;
                return true;
            }
            return false;
        }
        public static byte[] ChangeImage(byte[] img, int width = 480, int height = 640, int imageSizeMax = 150 * 1024)
        {
            using var mData = new MemoryStream(img);
            Image img2 = Image.Load(mData);
            if (img2.Width == width && img2.Height == height && mData.Length < imageSizeMax)
            {
                return img;
            }
            Image newimage;
            if (img2.Width != width || img2.Height != height)
            {
                newimage = img2.Clone(i =>
                {
                    i.AutoOrient();
                    i.Resize(width, height);
                });
            }
            else
            {
                newimage = img2;
            }
            byte[] result;
            if (mData.Length > imageSizeMax)
            {
                result = DeleteriouQualitys(newimage, imageSizeMax);
            }
            else
            {
                using var imgData = new MemoryStream();
                newimage.SaveAsJpeg(imgData);
                result = imgData.ToArray();
            }
            return result;
        }

        /// <summary>
        /// 递减质量
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageSizeMax"></param>
        /// <returns></returns>
        private static byte[] DeleteriouQualitys(Image image, int imageSizeMax)
        {
            var quality = 100;
            byte[] result = null;
            while (true)
            {
                using var memory = new MemoryStream();
                image.SaveAsJpeg(memory, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                {
                    Quality = quality
                });
                quality -= 5;
                if (memory.Length <= imageSizeMax || quality < 20)
                {
                    result = memory.ToArray();
                    break;
                }
            }
            return result;
        }
    }
}
