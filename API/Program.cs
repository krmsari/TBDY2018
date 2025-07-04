using API.Services;
using System;
using System.Windows.Forms;

namespace API.WinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Gerekli olan bütün yardımcı servisleri burada oluşturuyoruz.

            // Ana servisi, bu yardımcılarla birlikte oluşturuyoruz.
            ISap2000ApiService etabsApiService = new Sap2000ApiService();

            // Ana formu, ana servisi kullanarak çalıştırıyoruz.
            Application.Run(new Form1(etabsApiService));
        }
    }
}