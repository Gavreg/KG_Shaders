using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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


//Маршаллинг функций из DLL, написанной на плюсах
class myDll
{
    [DllImport("KG_Shaders.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    public static extern void StartTimer(int i);

    [DllImport("KG_Shaders.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr CreateWnd(IntPtr hInstance, IntPtr Parent);

    [DllImport("KG_Shaders.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr MessageBox(int hWnd, String text, String caption, uint type);

    [DllImport("KG_Shaders.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ex_loadModel(string filename);

    [DllImport("KG_Shaders.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ex_loadPixShader(IntPtr arr, int size);

    [DllImport("KG_Shaders.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ex_loadVertShader(IntPtr arr, int size);

    [DllImport("KG_Shaders.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ex_Compile();

    [DllImport("KG_Shaders.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern int loadTextute(int chanel, IntPtr texture, int w, int h);

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

        public ControlHost()
        {

        }

        protected unsafe override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            IntPtr hinstance = Marshal.GetHINSTANCE(typeof(App).Module);
            hWnd = myDll.CreateWnd((IntPtr)hinstance.ToPointer() + 6, (IntPtr)hwndParent.Handle);
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

        string loadedProject = string.Empty;
        Image[] imgArr = new Image[4];
        string[] textures = new string[4];

        public MainWindow()
        {
            InitializeComponent();
           
            this.Loaded += (s, a) =>
            { 
                ControlHost host = new ControlHost();
                grid.Content = host;
                host.Visibility = Visibility.Visible;
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


        void loadProject(string s)
        {
            XDocument xdoc = XDocument.Load(s);
            XElement root = xdoc.Element("project");
            tbPix.Text = (string)root.Attribute("pix");
            tbVert.Text = (string)root.Attribute("vert");

            for (int i=0; i<4; ++i)
            {
                string fname = (string)root.Attribute("t" + Convert.ToString(i));
                if (fname == string.Empty)
                    continue;
                FileInfo fi = new FileInfo(fname);
                textures[i] = fname;

                if (!fi.Exists)
                {
                    string name = fi.Name;
                    string projdir = (new FileInfo(s)).Directory.FullName;
                    fname = projdir + "//" + name;
                }

                LoadTexture(i, fname);
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
                root.Add(t);
            }

            xdoc.Add(root);


            xdoc.Save(s);
            this.Title = (new FileInfo(loadedProject).Name) + " - KG Shaders";
        }
            

        private void bApplyShaders_Click(object sender, RoutedEventArgs e)
        {
            {
                string a = tbVert.Text;
                Encoding enc = Encoding.GetEncoding(1251);
                byte[] b = enc.GetBytes(a);
                int s = b.Count<byte>();
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(s);
                Marshal.Copy(b, 0, unmanagedPointer, s);
                myDll.ex_loadVertShader(unmanagedPointer, s);
                Marshal.FreeHGlobal(unmanagedPointer);
            }
            {
                string  a = tbPix.Text;
                Encoding enc = Encoding.GetEncoding(1251);
                byte[] b = enc.GetBytes(a);
                int s = b.Count<byte>();
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(s);
                Marshal.Copy(b, 0, unmanagedPointer, s);
                myDll.ex_loadPixShader(unmanagedPointer, s);
                Marshal.FreeHGlobal(unmanagedPointer);
            }
            
            myDll.ex_Compile();
            
            if (myDll.errLength() > 0)
            {
                string err = myDll.getErrStr();
                tbLog.Text = err;
            }               
            else
                tbLog.Text = "ok-ok";
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

        private void LoadTexture(int chanel, string fileName)
        {


            System.Drawing.Bitmap b=null;
            System.Windows.Media.Imaging.BitmapSource bs=null;
            try
            {
                if (new FileInfo(fileName).Exists == false)
                    return;
                b = new System.Drawing.Bitmap(fileName);
               bs =
                        System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            b.GetHbitmap(),
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions()
                            );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            imgArr[chanel].Source = (ImageSource)bs;

            byte[] bb = new byte[b.Height * b.Width * 4];
            for (int i = 0; i < b.Height; ++i)
            {
                for (int j = 0; j < b.Width; ++j)
                {
                    var c = b.GetPixel(j, i);
                    int offset = i * b.Width * 4 + j * 4;
                    bb[offset] = c.R;
                    bb[offset + 1] = c.G;
                    bb[offset + 2] = c.B;
                    bb[offset + 3] = c.A;

                }
            }
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(b.Height * b.Width * 4);
            Marshal.Copy(bb, 0, unmanagedPointer, b.Height * b.Width * 4);
            myDll.loadTextute(chanel, unmanagedPointer, b.Width, b.Height);
            Marshal.FreeHGlobal(unmanagedPointer);
        }

        private void btnTexLoad1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDlg = new Microsoft.Win32.OpenFileDialog();

            if (openDlg.ShowDialog() == true)
            {
                string s = openDlg.FileName;
                textures[1] = s;
                LoadTexture(1, s);                
            }
        }


        private void btnTexLoad0(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDlg = new Microsoft.Win32.OpenFileDialog();

            if (openDlg.ShowDialog() == true)
            {
                string s = openDlg.FileName;
                textures[0] = s;
                LoadTexture(0, s);
            }
        }

        private void btnTexLoad2(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDlg = new Microsoft.Win32.OpenFileDialog();

            if (openDlg.ShowDialog() == true)
            {
                string s = openDlg.FileName;
                textures[2] = s;
                LoadTexture(2, s);
            }
        }

        private void btnTexLoad3(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDlg = new Microsoft.Win32.OpenFileDialog();

            if (openDlg.ShowDialog() == true)
            {
                string s = openDlg.FileName;
                textures[3] = s;
                LoadTexture(3, s);
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDlg = new Microsoft.Win32.SaveFileDialog();
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
