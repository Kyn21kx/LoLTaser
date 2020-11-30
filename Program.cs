using System;
using System.Drawing;
using System.IO;
using Windows.Media.Ocr;
using System.Threading;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;
using System.IO.Ports;

namespace LoLTaser {

    static class Program {

        private static bool tasing;
        /// <summary>
        /// This is actually the number of deaths
        /// </summary>
        private static int kda = 0;
        private static SerialPort port;

        static void Main(string[] args) {
            Console.WriteLine("Press any key to start the reading...");
            Console.ReadKey();
            Console.WriteLine("Starting in 3 seconds");
            Thread.Sleep(1000);
            Console.WriteLine("Starting in 2 seconds");
            Thread.Sleep(1000);
            Console.WriteLine("Starting in 1 seconds");
            Thread.Sleep(1000);
            Initialize();
            Loop();
        }
        private static void Initialize () {
            port = new SerialPort();
            port.BaudRate = 9600;
            port.PortName = "COM3";
            port.Open();
            tasing = false;
            kda = GetNewKDA(kda).GetAwaiter().GetResult();
        }

        private static void Loop() {
            int res = 0;
            while (true) {
                if (!tasing) {
                    res = LimitToRange(GetNewKDA(kda).GetAwaiter().GetResult(), kda);
                    if (res == kda + 1) {
                        tasing = true; //Triggers the taser
                        kda = res;
                    }
                }
                Console.WriteLine("Deaths: " + kda);

                if (tasing)
                    Tase();
                
            }
        }
        public static int LimitToRange(this int value, int inclusiveMinimum) {
            if (value < inclusiveMinimum) { return inclusiveMinimum; }
            return value;
        }
        private static void Tase () {
            //port.Write(duration.ToString());
            Console.WriteLine("Tasing...");
            port.Write("1");
            Console.WriteLine("Finished tasing");
            tasing = false;
        }


        private static async Task<int> GetNewKDA (int previousKDA) {
            string fileName = "X:\\Programming\\C#\\LoLTaser\\Captured Pictures\\Capture.png";
            await GetScreen(fileName); // Creates the specified file (image)
            try {
                //Do the thing
                Language language = new Language("en");
                var stream = File.OpenRead(fileName);
                var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
                var bitmap = await decoder.GetSoftwareBitmapAsync();
                var engine = OcrEngine.TryCreateFromLanguage(language);
                var ocrResult = await engine.RecognizeAsync(bitmap).AsTask();
                string r = SplitStart('/', ocrResult.Text);
                r = SplitEnd('/', r);
                stream.Close();
                File.Delete(fileName);
                int result = Convert.ToInt32(r);
                return result;
            }
            catch (Exception err) {
                Console.WriteLine(err.Message);
                File.Delete(fileName);
            }
            return previousKDA;
        }

        private static async Task GetScreen(string fileName) {
            Bitmap memoryImg = new Bitmap(100, 40);
            Size s = new Size(memoryImg.Width, memoryImg.Height);
            Graphics g = Graphics.FromImage(memoryImg);
            g.CopyFromScreen(1650, 0, 0, 0, s);
            //ToGrayScale(memoryImg);
            memoryImg.Save(fileName);
            memoryImg.Dispose();
        }

        private static string SplitEnd (char c, string s) {
            bool found = false;
            string res = "";
            for (int i = 0; i < s.Length; i++) {
                if (!found && s[i] == c) {
                    found = true;
                }
                if (!found)
                    res += s[i];
            }
            return res;
        }
        /// <summary>
        /// Splits the string from the first character until it finds the specified char value
        /// </summary>
        /// <param name="c">Character to identify</param>
        /// <param name="s">String for reference</param>
        /// <returns></returns>
        private static string SplitStart(char c, string s) {
            bool found = false;
            string res = "";
            for (int i = 0; i < s.Length; i++) {
                if (found)
                    res += s[i];
                if (!found && s[i] == c) {
                    found = true;
                }
            }
            return res;
        }

        private static void ToGrayScale(Bitmap Bmp) {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++) {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    Bmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                }
        }

    }
}
