using System;
using RestSharp;
using System.IO;
using System.Windows.Forms;
using CargaMasiva.GUI;
using InterfaceAlfresco.GUI;
using System.Security.Cryptography;
using System.Linq;




namespace ActualizarAlfresco.GUI
{
    public partial class Password : Form
    {
        string sIp = "http://148.202.33.65";

        public Password()
        {
            InitializeComponent();
        }


        private void DestruyeToken(string sToken)
        {
            //string sUrlGestor = "http://" + sIp + ":" + sPuerto + "/alfresco/service/api/login/ticket/" + sToken + "?alf_ticket=" + sToken;
            string sUrlGestor = sIp + "/alfresco/service/api/login/ticket/" + sToken + "?alf_ticket=" + sToken;
            var client = new RestClient(sUrlGestor);
            var request = new RestRequest(Method.DELETE);
            request.AddHeader("Postman-Token", "35bc446e-69a8-4a48-bc52-520a70c005aa");
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = client.Execute(request);
            string sTmp = response.Content;
        }

        private string ObtenToken()
        {
            string sToken = string.Empty;
            //string sUrlGestor = "http://" + sIp + ":" + sPuerto + "/alfresco/service/api/login?u=" + sUsuario + "&pw=" + sPassword;
            string sUrlGestor = sIp + "/alfresco/service/api/login?u=" + tUsuario.Text + "&pw=" + tContraseña.Text;
            var client = new RestClient(sUrlGestor);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Postman-Token", "35bc446e-69a8-4a48-bc52-520a70c005aa");
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = client.Execute(request);
            string sTmp = response.Content;
            //MessageBox.Show(sTmp);
            if (sTmp.Length > 10)
            {
                if (sTmp.IndexOf("</ticket>") > 0)
                {
                    int iInicio = sTmp.IndexOf(">T") + 1;
                    int iFinal = sTmp.IndexOf("</t");
                    sToken = sTmp.Substring(iInicio, iFinal - iInicio);
                }
            }
            else
            {
                MessageBox.Show(sTmp);
            }
            return sToken;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            MenuPrincipal form2 = new MenuPrincipal();
            string sToken = ObtenToken();
            if (sToken.IndexOf("TICKET") > -1)
            {
                VariablesGlobales.Usuario = tUsuario.Text;
                VariablesGlobales.Contraseña = tContraseña.Text;
                VariablesGlobales.sParametrizar = "1";
                this.Hide(); // Cierra el formulario actual (Form1)
                form2.Show(); // Abre Form2
            }
            else
            {
                MessageBox.Show("No se pudo accesar al gestor documental Alfresco", "Error en Comunicación");
            }
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

        private void Password_Load(object sender, EventArgs e)
        {
            string FileToRead = @"C:\UdeG\Parametros\Parametros.ini";
            if (File.Exists(FileToRead))
            {
                int lineas = File.ReadAllLines(FileToRead).Length;
                if (lineas == 6)
                {
                    StreamReader file = new StreamReader(FileToRead);
                    string sSitio = file.ReadLine();
                    file.Close();
                    sIp = Cifrado(2, sSitio, "JCGM1969", "JCGM1969");
                }
                else
                {
                    MessageBox.Show("Archivo C:\\UdeG\\Parametros\\Parametros.ini con información mal definida, favor de verificar", "Error");
                    //Application.Exit();
                }
            }
            else
            {
                MessageBox.Show("Archivo C:\\UdeG\\Parametros\\Parametros.ini No Existente", "Error");
                VariablesGlobales.sParametrizar = "0";
                button1.Enabled = false;
                button1.Visible = false;
                button2.Enabled = true;
                button2.Visible = true;
                tUsuario.Enabled = false;
                tContraseña.Enabled = false;
                button2.Focus();
            }
        }
        private void tUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Evita el sonido de "ding"
                tContraseña.Focus(); // Mueve el foco al siguiente TextBox
            }
        }

        private void tContraseña_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Evita el sonido de "ding"
                if (tUsuario.Text == string.Empty)
                {
                    MessageBox.Show("Usuario Invalido", "Teclear Usuario");
                    tUsuario.Focus(); // Mueve el foco al siguiente TextBox
                }
                else
                {
                    MenuPrincipal form2 = new MenuPrincipal();
                    string sToken = ObtenToken();
                    if (sToken.IndexOf("TICKET") > -1)
                    {
                        VariablesGlobales.Usuario = tUsuario.Text;
                        VariablesGlobales.Contraseña = tContraseña.Text;
                        this.Hide(); // Cierra el formulario actual (Form1)
                        form2.Show(); // Abre Form2
                    }
                    else
                    {
                        MessageBox.Show("No se pudo accesar al gestor documental Alfresco", "Error en Comunicación");
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide(); // Ocultamos antes para evitar que se vea el cierre
            // Verifica si hay otro formulario abierto que no sea este
            // Abre Form2
            ParametrosSistema nuevoFormulario = new ParametrosSistema();
            nuevoFormulario.Show();
            // Cierra el formulario actual (Form1)
        }
    }
}
