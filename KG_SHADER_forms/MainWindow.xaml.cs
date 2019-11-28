using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Reflection;
using System.Windows.Forms;
using Application = System.Windows.Application;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;
using PixelFormat = System.Drawing.Imaging.PixelFormat;


//Маршаллинг функций из DLL, написанной на плюсах
internal static class myDll
{
    [DllImport("KG_Shaders.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    public static extern void StartTimer(int i);

    [DllImport("KG_Shaders.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr CreateWnd(IntPtr hInstance, IntPtr Parent);

    [DllImport("KG_Shaders.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr MessageBox(int hWnd, String text, String caption, uint type);

    [DllImport("KG_Shaders.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ex_loadModel(string filename);

    [DllImport("KG_Shaders.dll", CallingConvention = CallingConvention.Cdecl)]
    //public static extern int ex_loadPixShader(IntPtr arr,  int size);
    public static extern int ex_loadPixShader([In, Out] string[] strings,int[] lengths, int size);

    [DllImport("KG_Shaders.dll",  CallingConvention = CallingConvention.Cdecl)]
    //public static extern int ex_loadVertShader(IntPtr arr, int size);
    public static extern int ex_loadVertShader([In, Out] string[] strings, int[] lengths, int size);

    [DllImport("KG_Shaders.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ex_Compile();

    [DllImport("KG_Shaders.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int loadTextute(int chanel, IntPtr texture, int w, int h);

    [DllImport("KG_Shaders.dll",  CallingConvention = CallingConvention.Cdecl)]
    public static extern int deleteTexture(int chanel);

    [DllImport("KG_Shaders.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public static extern string getErrStr();

    [DllImport("KG_Shaders.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern int errLength();

}


namespace KG_SHADER_forms
{

    public class ControlHost : HwndHost
    {
        IntPtr hWnd;

        public delegate void redy_delegate();

        public redy_delegate ready;

        public ControlHost()
        {

        }

        protected unsafe override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            IntPtr hinstance = Marshal.GetHINSTANCE(typeof(App).Module);
            hWnd = myDll.CreateWnd((IntPtr)hinstance.ToPointer() + 6, (IntPtr)hwndParent.Handle);
            ready?.Invoke();
            return new HandleRef(this, hWnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            //DestroyWindow(hwnd.Handle);
        }


    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        enum TextureType
        {
            None,
            File,
            Resource
        };
        string loadedProject = string.Empty;
        Image[] imgArr = new Image[4];
        string[] textures = new string[4];

        private TextureType[] texTypes =
        {
            TextureType.None, TextureType.None, TextureType.None, TextureType.None
        };

        public MainWindow()
        {
            InitializeComponent();
           
            this.Loaded += (s, a) =>
            { 
                ControlHost host = new ControlHost();
                grid.Content = host;
                host.Visibility = Visibility.Visible;

               
                host.ready += () =>
                {
                    LoadTextureFromResourse(0, "deftex.jpg"); 
                    compileShaders();
                };
            };

            imgArr[0] = iTexture0;
            imgArr[1] = iTexture1;
            imgArr[2] = iTexture2;
            imgArr[3] = iTexture3;

            for (int i=0; i<textures.Length; ++i)
            {
                textures[i] = string.Empty;
            }
            
        }

        void btnLoadDefTexture0(object sender, RoutedEventArgs e)
        {
            LoadTextureFromResourse(0, "deftex.jpg");
        }


        void loadProject(string s)
        {
            XDocument xdoc = XDocument.Load(s);
            XElement root = xdoc.Element("project");
            tbPix.Text = (string)root.Attribute("pix");
            tbVert.Text = (string)root.Attribute("vert");

            for (int i=0; i<4; ++i)
            {
                deleteTexture(i);
                
                string fname = (string)root.Attribute("t" + Convert.ToString(i));
                if (fname == string.Empty)
                    continue;
                
                textures[i] = fname;
                TextureType tt = TextureType.None;
                if (root.Attribute("tt" + Convert.ToString(i)) != null)
                {
                    int _tt = Convert.ToInt32(root.Attribute("tt" + Convert.ToString(i)).Value);
                    tt = (TextureType) _tt;
                }
                else
                {
                    tt = TextureType.File;
                }
                  
                texTypes[i] = tt;


                if (tt == TextureType.File)
                {
                    FileInfo fi = new FileInfo(fname);
                    if (!fi.Exists)
                    {
                        string name = fi.Name;
                        string projdir = (new FileInfo(s)).Directory.FullName;
                        fname = projdir + "//" + name;
                    }
                    LoadTextureFromFile(i, fname);
                }
                    
                if (tt == TextureType.Resource)
                    LoadTextureFromResourse(i,fname);
            }

            loadedProject = s;
            this.Title = (new FileInfo(loadedProject).Name) + " - KG Shaders";
            
        }

        void saveProject(string s)
        {
            XDocument xdoc = new XDocument();
            XElement root = new XElement("project");
            XAttribute vert = new XAttribute("vert", tbVert.Text);
            XAttribute pix = new XAttribute("pix", tbPix.Text);
            root.Add(vert);
            root.Add(pix);
            for (int i=0; i<=3; ++i)
            {
                XAttribute t = new XAttribute("t" + Convert.ToString(i), textures[i]);
                XAttribute ttype = new XAttribute("tt"+ Convert.ToString(i),(int)texTypes[i]);
                root.Add(ttype);
                root.Add(t);
            }

            xdoc.Add(root);


            xdoc.Save(s);
            this.Title = (new FileInfo(loadedProject).Name) + " - KG Shaders";
        }

        private void compileShaders()
        {
            tbLog.Text = "";
            tbLog.Text += "Compiling... \n";
            {
                string a = tbVert.Text;
                string[] array = a.Split(new string[] { "\r" }, StringSplitOptions.None);

                List<int> strings_lengts = new List<int>();

                for (int i = 0; i < array.Length; ++i)
                {
                    array[i] += "\n";
                    strings_lengts.Add(array[i].Length);
                }

                myDll.ex_loadVertShader(array, strings_lengts.ToArray(), array.Length);
            }
            {
                string a = tbPix.Text;

                string[] array = a.Split(new string[] { "\r" }, StringSplitOptions.None);

                List<int> strings_lengts = new List<int>();

                for (int i = 0; i < array.Length; ++i)
                {
                    array[i] += "\n";
                    strings_lengts.Add(array[i].Length);
                }

                myDll.ex_loadPixShader(array, strings_lengts.ToArray(), array.Length);
            }

            myDll.ex_Compile();

            if (myDll.errLength() > 0)
            {
                string err = myDll.getErrStr();
                tbLog.Text += "ERRORS!!\r";
                tbLog.Text += err;
            }
            else
                tbLog.Text += "Compiled!";
        }
        private void bApplyShaders_Click(object sender, RoutedEventArgs e)
        {
            compileShaders();
        }

        private void bOpenModel_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDlg = new Microsoft.Win32.OpenFileDialog();
          
            if (openDlg.ShowDialog() == true)
            {
                string s = openDlg.FileName;
                myDll.ex_loadModel(s);
            }
        }

        private void LoadTextureFromFile(int chanel, string fileName)
        {
            System.Drawing.Bitmap b=null;
            System.Windows.Media.Imaging.BitmapSource bs=null;

            if (new FileInfo(fileName).Exists == false)
                return; 
                
            b = new System.Drawing.Bitmap(fileName);


            using (MemoryStream memory = new MemoryStream())
            {
                b.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                imgArr[chanel].Source = bitmapImage;
            }

            textures[chanel] = fileName;
            texTypes[chanel] = TextureType.File;
           

            SentBitmapTo3d(b,chanel);



            // Old texture loading
            /*

            byte[] bb = new byte[b1.Height * b1.Width * 4];

            
            for (int i = 0; i < b.Height; ++i)
            {
                for (int j = 0; j < b.Width; ++j)
                {
                    var c = b1.GetPixel(j, i);
                    int offset = i * b.Width * 4 + j * 4;
                    bb[offset] = c.R;
                    bb[offset + 1] = c.G;
                    bb[offset + 2] = c.B;
                    bb[offset + 3] = c.A;

                }
            }
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(b1.Height * b1.Width * 4);
            Marshal.Copy(bb, 0, unmanagedPointer, b1.Height * b1.Width * 4);
            myDll.loadTextute(chanel, unmanagedPointer, b1.Width, b1.Height);
            Marshal.FreeHGlobal(unmanagedPointer);   
             
             */

        }

        private void LoadTextureFromResourse(int chanel, string resKey)
        {
            var bitmapImage = new BitmapImage(new Uri(@"pack://application:,,,/"
                                                + Assembly.GetExecutingAssembly().GetName().Name
                                                + ";component/"
                                                + resKey, UriKind.Absolute));

            var bitmap = BitmapImage2Bitmap(bitmapImage);
            var hbitmap = bitmap.GetHbitmap();
            var bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hbitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            imgArr[chanel].Source = bitmapImage;

            DeleteObject(hbitmap);

            textures[chanel] = resKey;
            texTypes[chanel] = TextureType.Resource;
            SentBitmapTo3d(bitmap,chanel);


        }
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }

        private void SentBitmapTo3d(System.Drawing.Bitmap b, int chanel)
        {
            var b1 = new System.Drawing.Bitmap(b.Width, b.Height, PixelFormat.Format32bppRgb);
            using (var g = Graphics.FromImage(b1))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.CompositingQuality = CompositingQuality.AssumeLinear;
                g.DrawImage(b, 0, 0, b1.Width, b1.Height);
            }

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, b1.Width, b1.Height);
            var bitmapData = b1.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, b1.PixelFormat);
            myDll.loadTextute(chanel, bitmapData.Scan0, b1.Width, b1.Height);
            b1.UnlockBits(bitmapData);
        }

        private void btnTexLoad0(object sender, RoutedEventArgs e)
        {
            loadTexture(0);
        }

        private void btnTexLoad1(object sender, RoutedEventArgs e)
        {
            loadTexture(1);
        }

        private void btnTexLoad2(object sender, RoutedEventArgs e)
        {
            loadTexture(2);
        }

        private void btnTexLoad3(object sender, RoutedEventArgs e)
        {
            loadTexture(3);
        }

        private void btnTexDelete0(object sender, RoutedEventArgs e)
        {
            deleteTexture(0);
        }
        private void btnTexDelete1(object sender, RoutedEventArgs e)
        {
            deleteTexture(1);
        }
        private void btnTexDelete2(object sender, RoutedEventArgs e)
        {
            deleteTexture(2);
        }
        private void btnTexDelete3(object sender, RoutedEventArgs e)
        {
            deleteTexture(3);
        }

        private void deleteTexture(int number)
        {
            imgArr[number].Source = null;
            textures[number] = string.Empty;
            texTypes[number] = TextureType.None;
            myDll.deleteTexture(number);
        }

        private string lastDir = string.Empty;
        private void loadTexture(int number)
        {
            Microsoft.Win32.OpenFileDialog openDlg = new Microsoft.Win32.OpenFileDialog();

            if (lastDir == String.Empty)
            {
                if (loadedProject == string.Empty)
                {
                    openDlg.InitialDirectory =
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
                else
                {
                    openDlg.InitialDirectory = System.IO.Path.GetDirectoryName(loadedProject);
                }
                
            }
            else
            {
                openDlg.InitialDirectory = lastDir;
            }
            if (openDlg.ShowDialog() == true)
            {
                string s = openDlg.FileName;
                LoadTextureFromFile(number, s);
                lastDir = System.IO.Path.GetDirectoryName(s);
            }

        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDlg = new Microsoft.Win32.SaveFileDialog();

            if (loadedProject == string.Empty)
            {
                saveDlg.InitialDirectory =
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                saveDlg.InitialDirectory = System.IO.Path.GetDirectoryName(loadedProject);
            }

            saveDlg.FileName = "Shader project";
            saveDlg.DefaultExt = "kgs";
            saveDlg.Filter = "Файлы KG-Shader (*.kgs)|*.kgs";
            if (saveDlg.ShowDialog() != true)
                return;

            loadedProject = saveDlg.FileName;
            saveProject(loadedProject);
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            if (loadedProject == string.Empty)
                SaveAs_Click(sender, e);
            else
                saveProject(loadedProject);
        }

        private void bLoadProject_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDlg = new Microsoft.Win32.OpenFileDialog();
            openDlg.Filter = "Файлы KG-Shader (*.kgs)|*.kgs";
            if (loadedProject == string.Empty)
            {
                openDlg.InitialDirectory =
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                openDlg.InitialDirectory = System.IO.Path.GetDirectoryName(loadedProject);
            }
            if (openDlg.ShowDialog() == true)
            {
                string s = openDlg.FileName;
                loadProject(s);
            }
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            helpWnd wnd = new helpWnd();
            wnd.ShowDialog();
        }
    }
}
