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
using System.Windows.Shapes;
using System.IO;

namespace KG_SHADER_forms
{
    /// <summary>
    /// Логика взаимодействия для helpWnd.xaml
    /// </summary>
    public partial class helpWnd : Window
    {
        public helpWnd()
        {
            InitializeComponent();

            Uri uri = new Uri(@"pack://application:,,,/help.txt", UriKind.Absolute);


            Stream stream = Application.GetResourceStream(uri).Stream;
            using (StreamReader sr = new StreamReader(stream, Encoding.GetEncoding(1251)))
            {
                help.Text = sr.ReadToEnd();
            }

           
        }
    }
}
