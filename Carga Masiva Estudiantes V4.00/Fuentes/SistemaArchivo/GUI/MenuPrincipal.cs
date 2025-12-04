using System;
using System.Windows.Forms;
using InterfaceAlfresco.GUI;
using System.IO;
using RestSharp;
using System.Security.Cryptography;
using ActualizarAlfresco.GUI;


namespace CargaMasiva.GUI
{
    public partial class MenuPrincipal : Form
    {

        string sIpAlfresco = "http://148.202.33.65";
        string sIpBd = "148.202.33.66";
        string sPuertoBd = string.Empty;
        string sUsuarioBd = string.Empty;
        string sPasswordBd = string.Empty;
        string sNombreBd = string.Empty;
        static string sNodo = string.Empty;
        static string bOk = string.Empty;
        string sllave = string.Empty;
        public MenuPrincipal()
        {
            InitializeComponent();
        }

        private void cargaMasivaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Automatico form2 = new Automatico();
            form2.Show(); // Abre Form2
        }

        private void MenuPrincipal_Load(object sender, EventArgs e)
        {

        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MenuPrincipal_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void MenuPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void descargarPlantillaToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void descargarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ruta = "C:\\UDEG\\ArchivoDemoCarga.txt";
            if ((Directory.Exists("C:\\UDEG\\")))
            {
                Directory.CreateDirectory("C:\\UDEG\\");
            }
            using (StreamWriter escritor = new StreamWriter(ruta))
            {
                escritor.WriteLine("IDExpediente|Sitio|TipoDoc|CodigoDocente|RequisitoRubro(1 requisito 0 rubro)|Item|Ruta");
                escritor.WriteLine("152545|EUD|Prueba1|152545|1|144|C:\\UdeG\\Pdfs\\Documento Demo Sperto1.pdf");
                escritor.WriteLine("152545|EUE|Prueba2|152545|0|145|C:\\UdeG\\Pdfs\\Documento Demo Sperto2.pdf");
            }
            MessageBox.Show("C:\\UDEG\\ArchivoDemoCarga.txt", "Archivo Descargado");

        }

        private void acercaDeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("           Versión 1.00 \n           Fecha 21-04-2025 \n           Desarrollo C# \n           Sperto Digital S.A. de C.V","Información Modulo Carga Masiva");
        }

        private void parametrizaciónToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParametrosSistema form2 = new ParametrosSistema();
            form2.Show(); // Abre Form2
        }

        private void EliminaAlfresco(string sNodo, string sToken, StreamWriter writer)
        {
            string sUrlGestor = sIpAlfresco + "/alfresco/service/eliminardocenteudeg?alf_ticket=" + sToken;
            var client = new RestClient(sUrlGestor);
            var request = new RestRequest(Method.POST);
            try
            {
                request.AddHeader("Postman-Token", "4a6ed555-01ef-4a5f-98d0-6b028e42b70f");
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW");
                request.AddParameter("multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW", "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"nodo\"\r\n\r\n" + sNodo + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW--", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                writer.WriteLine("Nodo Eliminado ," + sNodo);
            }
            catch (Exception ex)
            {
                writer.WriteLine("Error no se pudo eliminar el Nodo," + sNodo);
                MessageBox.Show(ex.ToString());
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

        private void DestruyeToken(string sToken)
        {
            //string sUrlGestor = "http://" + sIp + ":" + sPuerto + "/alfresco/service/api/login/ticket/" + sToken + "?alf_ticket=" + sToken;
            string sUrlGestor = sIpAlfresco + "/alfresco/service/api/login/ticket/" + sToken + "?alf_ticket=" + sToken;
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
            string sUrlGestor = sIpAlfresco + "/alfresco/service/api/login?u=" + VariablesGlobales.Usuario + "&pw=" + VariablesGlobales.Contraseña;
            var client = new RestClient(sUrlGestor);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Postman-Token", "35bc446e-69a8-4a48-bc52-520a70c005aa");
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = client.Execute(request);
            string sTmp = response.Content;
            if (sTmp.Length > 10)
            {
                int iInicio = sTmp.IndexOf(">T") + 1;
                int iFinal = sTmp.IndexOf("</t");
                sToken = sTmp.Substring(iInicio, iFinal - iInicio);
            }
            else
            {
                //MessageBox.Show(sTmp);
            }
            return sToken;
        }

        private void eliminacionNodosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string FileToRead = @"C:\UdeG\Parametros\Parametros.ini";
            StreamReader ReaderObject = new StreamReader(FileToRead);
            if (File.Exists(FileToRead))
            {
                int lineas = File.ReadAllLines(FileToRead).Length;
                if (lineas == 6)
                {
                    StreamReader file = new StreamReader(FileToRead);
                    string sSitio = file.ReadLine();
                    string sIP = file.ReadLine();
                    string sPuerto = file.ReadLine();
                    string sUsuario = file.ReadLine();
                    string sPassword = file.ReadLine();
                    string sNombre = file.ReadLine();
                    file.Close();
                    sIpAlfresco = Cifrado(2, sSitio, "JCGM1969", "JCGM1969");
                    sIpBd = Cifrado(2, sIP, "JCGM1969", "JCGM1969");
                    sPuertoBd = Cifrado(2, sPuerto, "JCGM1969", "JCGM1969");
                    sUsuarioBd = Cifrado(2, sUsuario, "JCGM1969", "JCGM1969");
                    sPasswordBd = Cifrado(2, sPassword, "JCGM1969", "JCGM1969");
                    sNombreBd = Cifrado(2, sNombre, "JCGM1969", "JCGM1969");

                    Cursor.Current = Cursors.WaitCursor;
                    string FileToRead2 = @"C:\UdeG\EliminarNodos.txt";
                    StreamReader ReaderObject2 = new StreamReader(FileToRead2);
                    StreamWriter writer = new StreamWriter(FileToRead2.Substring(0, FileToRead2.Length - 4) + "_Resultado.csv");
                    string Line;
                    string sToken = ObtenToken();
                    sToken = ObtenToken();
                    if (sToken.IndexOf("TICKET") > -1)
                    {
                        while ((Line = ReaderObject2.ReadLine()) != null)
                        {
                            EliminaAlfresco(Line, sToken, writer);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No se pudo accesar al gestor documental Alfresco", "Error en Comunicación");
                    }
                    DestruyeToken(sToken);
                    //Application.Exit();
                    Cursor.Current = Cursors.Arrow;
                    ReaderObject.Close();
                    ReaderObject2.Close();
                    writer.Close();
                    MessageBox.Show("Archivo Procesado");
                    //Application.Exit();
                }
                else
                {
                    ReaderObject.Close();
                    MessageBox.Show("Archivo C:\\UdeG\\Parametros\\Parametros.ini con información mal definida, favor de verificar", "Error");
                    //Application.Exit();
                }
            }
            else
            {
                ReaderObject.Close();
                MessageBox.Show("No existe el Archivo de Parametros favor de colocarlo en la ruta definida C:\\UdeG\\Url.txt", "Error");
                //Application.Exit();
            }
        }
    }
}
