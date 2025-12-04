using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using ActualizarAlfresco.GUI;

namespace InterfaceAlfresco.GUI
{
    public partial class ParametrosSistema : Form
    {
        public ParametrosSistema()
        {
            InitializeComponent();
        }

        private string Cifrado(Byte modo, string cadena, string key, string VecI)
        {
            Byte[] plaintext;
            Byte[] keys = System.Text.Encoding.Default.GetBytes(key);
            MemoryStream memdata = new MemoryStream();
            ICryptoTransform transforma;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            if (modo == 1)
            {
                plaintext = System.Text.Encoding.Default.GetBytes(cadena);
            }
            else
            {
                plaintext = Convert.FromBase64String(cadena);
            }
            des.Mode = CipherMode.CBC;
            if (modo == 1)
            {
                transforma = des.CreateEncryptor(keys, System.Text.Encoding.Default.GetBytes(VecI));
            }
            else
            {
                transforma = des.CreateDecryptor(keys, System.Text.Encoding.Default.GetBytes(VecI));
            }
            CryptoStream encstream = new CryptoStream(memdata, transforma, CryptoStreamMode.Write);
            encstream.Write(plaintext, 0, plaintext.Length);
            encstream.FlushFinalBlock();
            encstream.Close();
            if (modo == 1)
            {
                cadena = Convert.ToBase64String(memdata.ToArray());
            }
            else
            {
                cadena = System.Text.Encoding.Default.GetString(memdata.ToArray());
            }
            return cadena;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            if (tSitio.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("El Sitio de Trabajo de Alfresco es Invalido", "Administración de Parametros del Sistema");
                tSitio.Focus();
                return;
            }
            if (tIP.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("La Direccion IP introducida es Invalida", "Administración de Parametros del Sistema");
                tIP.Focus();
                return;
            }
            if (tPuerto.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("El puerto de comunicación es Invalido", "Administración de Parametros del Sistema");
                tPuerto.Focus();
                return;
            }
            if (tUsuario.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("El Usuario es Invalido", "Administración de Parametros del Sistema");
                tUsuario.Focus();
                return;
            }
            if (tPassword.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("El Password es Invalido", "Administración de Parametros del Sistema");
                tPassword.Focus();
                return;
            }
            if (tNombre.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("El Nombre de la Base de Datos es Invalido", "Administración de Parametros del Sistema");
                tNombre.Focus();
                return;
            }
            string sPath = "C:\\UDEG\\Parametros";
            try { if (!Directory.Exists(sPath)) Directory.CreateDirectory(sPath); }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            finally { sPath = "C:\\UDEG\\Parametros"; }
            string sArchivoBase = sPath + "\\Parametros.ini";
             if (File.Exists(sArchivoBase)) File.Delete(sArchivoBase);
            string sUsuario = tUsuario.Text.Trim();
            string sPassword = tPassword.Text.Trim();
            string ssitioencrioptado = Cifrado(1, tSitio.Text.Trim(), "JCGM1969", "JCGM1969");
            string sipencriptado = Cifrado(1, tIP.Text.Trim(), "JCGM1969", "JCGM1969");
            string spuertoencriptado = Cifrado(1, tPuerto.Text.Trim(), "JCGM1969", "JCGM1969");
            string susuarioEncriptado = Cifrado(1, sUsuario, "JCGM1969", "JCGM1969");
            string sPasswordEncriptado = Cifrado(1, sPassword, "JCGM1969", "JCGM1969");
            string snombreencriptado = Cifrado(1, tNombre.Text.Trim(), "JCGM1969", "JCGM1969");
            StreamWriter sw = new StreamWriter(sArchivoBase);
            sw.WriteLine(ssitioencrioptado);
            sw.WriteLine(sipencriptado);
            sw.WriteLine(spuertoencriptado);
            sw.WriteLine(susuarioEncriptado);
            sw.WriteLine(sPasswordEncriptado);
            sw.WriteLine(snombreencriptado);
            sw.Close();
            MessageBox.Show("Actualización Realizada", "Administración de Parametros del Sistema");
        }

        private void Limpiar()
        {
            tIP.Text = string.Empty;
            tPuerto.Text = string.Empty;
            tUsuario.Text = string.Empty;
            tPassword.Text = string.Empty;
            tNombre.Text = string.Empty;
            tSitio.Text = string.Empty;
        }


        private void Consulta(string sArchivo)
        {
            Limpiar();
            StreamReader file = new StreamReader(sArchivo);
            string sSitio = file.ReadLine();
            string sIP = file.ReadLine();
            string sPuerto = file.ReadLine();
            string sUsuarioAlfresco = file.ReadLine();
            string sPasswordAlfresco = file.ReadLine();
            string sNombre = file.ReadLine();
            file.Close();
            tSitio.Text = Cifrado(2, sSitio, "JCGM1969", "JCGM1969");
            tIP.Text = Cifrado(2, sIP, "JCGM1969", "JCGM1969");
            tPuerto.Text = Cifrado(2, sPuerto, "JCGM1969", "JCGM1969");
            tUsuario.Text = Cifrado(2, sUsuarioAlfresco, "JCGM1969", "JCGM1969");
            tPassword.Text = Cifrado(2, sPasswordAlfresco, "JCGM1969", "JCGM1969");
            tNombre.Text = Cifrado(2, sNombre, "JCGM1969", "JCGM1969");
        }
        private void ParametrosSistema_Load(object sender, EventArgs e)
        {
            string sPath = "C:\\UDEG\\Parametros";
            try { if (!Directory.Exists(sPath)) Directory.CreateDirectory(sPath); }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            finally { sPath = "C:\\UDEG\\Parametros"; }
            string sArchivo = sPath + "\\Parametros.ini";
            if (File.Exists(sArchivo))
            {
                Consulta(sArchivo);
            }
        }

        private void ParametrosSistema_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (VariablesGlobales.sParametrizar == "0")
            {
                Application.Exit();
            }
        }

        private void ParametrosSistema_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (VariablesGlobales.sParametrizar == "0")
            {
                Application.Exit();
            }
        }
    }
}
