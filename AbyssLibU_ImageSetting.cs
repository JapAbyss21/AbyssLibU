namespace AbyssLibU
{
    /// <summary>
    /// イメージの設定です。
    /// </summary>
    public struct ImageSetting
    {
        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName;
        /// <summary>
        /// 矩形領域のX座標、矩形領域のY座標、矩形領域の横幅、矩形領域の縦幅
        /// </summary>
        public int X, Y, Width, Height;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="fileName">ファイル名を指定します。</param>
        /// <param name="x">矩形領域のX座標を指定します。</param>
        /// <param name="y">矩形領域のY座標を指定します。</param>
        /// <param name="width">矩形領域の横幅を指定します。</param>
        /// <param name="height">矩形領域の縦幅を指定します。</param>
        public ImageSetting(string fileName, int x, int y, int width, int height)
        {
            FileName = fileName;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override readonly bool Equals(object obj)
        {
            if (obj is not ImageSetting)
            {
                return false;
            }
            var setting = (ImageSetting)obj;
            return FileName == setting.FileName &&
                   X == setting.X &&
                   Y == setting.Y &&
                   Width == setting.Width &&
                   Height == setting.Height;
        }
        public override readonly int GetHashCode()
        {
            return System.HashCode.Combine(FileName, X, Y, Width, Height);
        }
        public static bool operator ==(ImageSetting setting1, ImageSetting setting2)
        {
            return setting1.Equals(setting2);
        }
        public static bool operator !=(ImageSetting setting1, ImageSetting setting2)
        {
            return !(setting1 == setting2);
        }
    }
}