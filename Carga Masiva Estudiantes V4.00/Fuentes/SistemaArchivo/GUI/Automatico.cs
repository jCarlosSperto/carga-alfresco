using System;
using System.IO;
using System.Threading;
using System.Collections.Specialized;
using RestSharp;
using System.Windows.Forms;
using System.Web;
using System.Net;
using ActualizarAlfresco.GUI;
using System.Security.Cryptography;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace CargaMasiva.GUI
{
    public partial class Automatico : Form
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
        Boolean bExiste = false;
        string connectionString = null;
        string basedatos = null;
        int iTotalReg = 1500;
        string[,] DatosEstudianteProcesar = new string[1500, 6];

        public Automatico()
        {
            InitializeComponent();
        }

        private void GeneraCadenaAppConfig()
        {
            connectionString = $"Data Source={ConfigurationManager.AppSettings["DataSource"].ToString()};" +
               $"Initial Catalog={ConfigurationManager.AppSettings["InitialCatalog"].ToString()};" +
               $"User Id={ConfigurationManager.AppSettings["UserId"].ToString()};" +
               $"Password={ConfigurationManager.AppSettings["Password"].ToString()};" +
               $"Timeout={ConfigurationManager.AppSettings["TimeOut"].ToString()};";
            basedatos = $"{ConfigurationManager.AppSettings["InitialCatalog"].ToString()}";
            //unidad = $"{ConfigurationManager.AppSettings["Unidad"].ToString()}";
        }
        private void GeneraCadena()
        {
            connectionString = "Data Source=" + sIpBd + ";" +
               "Initial Catalog=" + sNombreBd  + ";" +
               "User Id=" + sUsuarioBd + ";" +
               "Password=" + sPasswordBd + ";" +
               "Timeout=120;";
        }
        private void Inicializa()
        {
            //for (int i = 0; i < Estudiantes.GetLength(0); i++)
            //for (int j = 0; j < Estudiantes.GetLength(1); j++)
            for (int i = 0; i < iTotalReg; i++)
            {
                DatosEstudianteProcesar[i, 0] = string.Empty;
                DatosEstudianteProcesar[i, 1] = string.Empty;
                DatosEstudianteProcesar[i, 2] = string.Empty;
                DatosEstudianteProcesar[i, 3] = string.Empty;
                DatosEstudianteProcesar[i, 4] = string.Empty;
                DatosEstudianteProcesar[i, 5] = string.Empty;
            }
        }
        private void ObtenRegistrosProcesar()
        {
            Inicializa();
            string sQuery = "SELECT top 1500 NombrePdf, CodigoEscolar, CodigoEscolarCorregido, Carrera, NombreCompleto, FechaGraduacion FROM " + basedatos + ".[dbo].[Captura] where (EstatusAlfresco is null and ImagenRotada ='NO' and HojasBlancas ='NO' and EstatusCalidad is null and not ComentarioExpediente like '%EXPEDIENTE CON MAS DE 1 PERSONA%') or (EstatusAlfresco is null and EstatusCalidad = 'CORREGIDO')";
            int iIndice = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sQuery, connection))
                    {
                        //command.Parameters.AddWithValue("@id", id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DatosEstudianteProcesar[iIndice, 0] = reader["NombrePdf"].ToString();
                                DatosEstudianteProcesar[iIndice, 1] = reader["CodigoEscolar"].ToString();
                                DatosEstudianteProcesar[iIndice, 2] = reader["CodigoEscolarCorregido"].ToString();
                                DatosEstudianteProcesar[iIndice, 3] = reader["Carrera"].ToString();
                                DatosEstudianteProcesar[iIndice, 4] = reader["NombreCompleto"].ToString();
                                DatosEstudianteProcesar[iIndice, 4] = DatosEstudianteProcesar[iIndice, 4].Replace("\\", "_");
                                DatosEstudianteProcesar[iIndice, 4] = DatosEstudianteProcesar[iIndice, 4].Replace("/", "_");
                                DatosEstudianteProcesar[iIndice, 4] = DatosEstudianteProcesar[iIndice, 4].Replace(":", "_");
                                DatosEstudianteProcesar[iIndice, 4] = DatosEstudianteProcesar[iIndice, 4].Replace("*", "_");
                                DatosEstudianteProcesar[iIndice, 4] = DatosEstudianteProcesar[iIndice, 4].Replace("?", "_");
                                DatosEstudianteProcesar[iIndice, 4] = DatosEstudianteProcesar[iIndice, 4].Replace("\"", "_");
                                DatosEstudianteProcesar[iIndice, 4] = DatosEstudianteProcesar[iIndice, 4].Replace(">", "_");
                                DatosEstudianteProcesar[iIndice, 4] = DatosEstudianteProcesar[iIndice, 4].Replace("<", "_");
                                DatosEstudianteProcesar[iIndice, 4] = DatosEstudianteProcesar[iIndice, 4].Replace("|", "_");
                                DatosEstudianteProcesar[iIndice, 5] = reader["FechaGraduacion"].ToString();
                                iIndice = iIndice + 1;
                            }
                        }
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ocurrió un error: " + ex.Message);
                }
            }
        }

        private void ActualizaEstatusAlfresco(string valor, string sNombrePdf, string sFechaHora)
        {
            string updateQuery = "UPDATE " + basedatos + ".[dbo].[Captura] SET EstatusAlfresco = @newValue, FechaHora = @newFechaHora  WHERE NombrePdf = @id";
            string newValue = valor;
            string id = sNombrePdf;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@newValue", newValue);
                        command.Parameters.AddWithValue("@newFechaHora", sFechaHora);
                        command.Parameters.AddWithValue("@id", id);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("Dato actualizado correctamente.");
                        }
                        else
                        {
                            MessageBox.Show("No se encontró el registro para actualizar.");
                        }
                    }
                    connection.Close();
                }
                catch (Exception x)
                {
                    MessageBox.Show("Error " + x.Message);
                }
            }
        }

        private void InsertaLog(string sLlave, string sUsuario, string sRegistro)
        {
            string insertQuery = "INSERT INTO"  + basedatos + ".[dbo].[LogCarga](llave, Usuario, Registro) VALUES (@llave, @usuario, @registro)";
            try
            {
                string llave = sLlave;
                string usuario = sUsuario;
                string registro = sRegistro;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open(); //Conexión a la base de datos                    
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("llave", llave); // Agregar el parámetro al comando codigo
                        command.Parameters.AddWithValue("usuario", usuario); // Agregar el parámetro al comando id_asociado
                        command.Parameters.AddWithValue("registro", registro); // Agregar el parámetro al comando nodo
                        int rowsAffected = command.ExecuteNonQuery(); // Ejecutar el comando
                        Console.WriteLine($"Filas insertadas: {rowsAffected}");
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
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


        public static Boolean HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc, string TipoDocumento, string Codigo, string Nombre)
        {
            Boolean bRespuesta = false;
            sNodo = string.Empty;
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;
            Stream rs = wr.GetRequestStream();
            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);
            DateTime ahora = DateTime.Now;
            //string formatoPersonalizado = ahora.ToString("yyyyMMddHHmmss");
            string file1 = Nombre + ".pdf";
            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file1, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();
            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                string sRespuesta = reader2.ReadToEnd();
                sNodo = sRespuesta;
                if (sNodo.IndexOf("</nodo>") > 0)
                {
                    int iInicio = sNodo.IndexOf("<nodo>");
                    int iFinal = sNodo.IndexOf("</nodo>");
                    sNodo = sNodo.Substring(iInicio + 6, iFinal - iInicio - 6);
                    bRespuesta = true;
                }
                else
                {
                    bRespuesta = false;
                }
                //MessageBox.Show(sRespuesta);
                //MessageBox.Show(sNodo);

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
            Thread.Sleep(1000);
            return bRespuesta;
        }

        public static class MimeHelper
        {
            public static string GetMimeType(string fileName)
            {
                return MimeMapping.GetMimeMapping(fileName);
            }
        }

        private void ProcesaAlfresco(string Codigo, string AñoTitulacion, string TipoDocumento, string Ruta, StreamWriter writer, string Line, string Nombre)
        {
            string sToken = ObtenToken();
            sToken = ObtenToken();
            if (sToken.IndexOf("TICKET") > -1)
            {
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("sitionombre", "EUE");
                nvc.Add("IDEXPEDIENTE", AñoTitulacion);
                nvc.Add("TIPODOCUMENTO", TipoDocumento);
                //var mimeType = MimeMapping.GetMimeMapping(Ruta);
                string mimeType = MimeHelper.GetMimeType(Ruta);
                nvc.Add("CODIGOESTUDIANTE", Codigo);
                if (HttpUploadFile(sIpAlfresco + "/alfresco/service/altaestudianteudeg?alf_ticket=" + sToken, @Ruta, "filedata", mimeType, nvc, TipoDocumento, Codigo, Nombre))
                {
                    //Line = Line.Replace('|', ',');
                    writer.WriteLine(Line + "|Cargado Exitosamente|" + sNodo);
                    bOk = "SI";
                }
                else
                {
                    //Line = Line.Replace('|', ',');
                    writer.WriteLine(Line + "|No se Pudo Cargar |Carpeta Tal vez existente");
                    bOk = "NO";
                }
                DestruyeToken(sToken);
                nvc.Clear();
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

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            string rutaArchivo1 = @"C:\UDEG\PROCESAR.TXT";
            string rutaArchivo2 = @"C:\UDEG\PROCESAR_RESULTADOS.CSV";
            string dFechaHora = DateTime.Now.ToString("yyyyMMddHHmmss");
            Boolean bAvanzar = false;
            if (!(rutaArchivo1 == string.Empty))
            {
                DialogResult resultado = MessageBox.Show(
                                        "¿Está seguro de que desea procesar la carga?",
                                        "Confirmación",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Warning
                                        );
                if (resultado == DialogResult.Yes)
                {
                    int iAvance = 0;
                    string[] parts = rutaArchivo1.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    int NoCarpetas = parts.Length;
                    string sArchivo = parts[NoCarpetas - 1];
                    DateTime fechaModificacion = File.GetLastWriteTime(rutaArchivo1);
                    bExiste = false;
                    if (bExiste == false)
                    {
                        bAvanzar = true;
                    }
                    else
                    {
                        DialogResult resultado2 = MessageBox.Show(
                        "¿Archivo cargado con anteriodidad, Está seguro de que desea cargar el archivo?",
                        "Confirmación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                        );
                        if (resultado2 == DialogResult.Yes)
                        {
                            bAvanzar = true;
                        }
                    }
                    if (bAvanzar == true)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        ObtenRegistrosProcesar();
                        if (File.Exists(rutaArchivo1))
                        {
                            File.Delete(rutaArchivo1);
                            Console.WriteLine("Archivo eliminado.");
                        }
                        if (File.Exists(rutaArchivo2))
                        {
                            File.Delete(rutaArchivo2);
                            Console.WriteLine("Archivo eliminado.");
                        }
                        StreamWriter writera = new StreamWriter(rutaArchivo1, false, System.Text.Encoding.Default);
                        writera.WriteLine("Codigo Escolar,Fecha Titulacion, Tipo de Documento,Ruta");
                        for (int iIndice = 0; iIndice < iTotalReg; iIndice++)
                        {
                            if (!DatosEstudianteProcesar[iIndice, 0].Equals(string.Empty))
                            {
                                string RutaOriginal = "C:\\xampp\\htdocs\\CapturaUdeG\\pdf\\";
                                string RutaFinal = "C:\\xampp\\htdocs\\CapturaUdeG\\Cargados\\";
                                string ArchivoOrigen = RutaOriginal + DatosEstudianteProcesar[iIndice, 0];
                                string ArchivoDestino = RutaFinal;
                                string Codigo = string.Empty;
                                Codigo = DatosEstudianteProcesar[iIndice, 0];
                                Codigo = Codigo.Substring(0, Codigo.Length - 4);
                                Codigo = Codigo.ToUpper();
                                Codigo = Codigo.Replace("_", "-");
                                Codigo = Codigo.Replace(" ", "");
                                if (Codigo.IndexOf("CAJA") > -1)
                                {
                                    Codigo = Codigo.Replace("CAJA", "G");
                                }
                                else
                                {
                                    if (Codigo.IndexOf("ESPECIAL") > -1)
                                    {
                                        Codigo = Codigo.Replace("ESPECIAL", "G");
                                    }
                                    else
                                    {
                                        Codigo = "G" + Codigo;
                                    }
                                }
                                if (File.Exists(ArchivoOrigen))
                                {
                                    if (string.IsNullOrEmpty(DatosEstudianteProcesar[iIndice, 2])) //si es Null el Codigo Escolar corregido entonces tomamos el original
                                    {
                                        ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 1] + "_" + DatosEstudianteProcesar[iIndice, 4] + ".pdf";
                                        if (File.Exists(ArchivoDestino))
                                        {
                                            ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 1] + "_" + DatosEstudianteProcesar[iIndice, 4] + "_1.pdf";
                                            if (File.Exists(ArchivoDestino))
                                            {
                                                ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 1] + "_" + DatosEstudianteProcesar[iIndice, 4] + "_2.pdf";
                                                if (File.Exists(ArchivoDestino))
                                                {
                                                    ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 1] + "_" + DatosEstudianteProcesar[iIndice, 4] + "_3.pdf";
                                                    if (File.Exists(ArchivoDestino))
                                                    {
                                                        ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 1] + "_" + DatosEstudianteProcesar[iIndice, 4] + "_4.pdf";
                                                        if (File.Exists(ArchivoDestino))
                                                        {
                                                            ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 1] + "_" + DatosEstudianteProcesar[iIndice, 4] + "_5.pdf";
                                                            if (File.Exists(ArchivoDestino))
                                                            {
                                                                ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 1] + "_" + DatosEstudianteProcesar[iIndice, 4] + "_6.pdf";
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 2] + "_" + DatosEstudianteProcesar[iIndice, 4] + ".pdf";
                                        if (File.Exists(ArchivoDestino))
                                        {
                                            ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 2] + "_" + DatosEstudianteProcesar[iIndice, 4] + "_1.pdf";
                                            if (File.Exists(ArchivoDestino))
                                            {
                                                ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 2] + "_" + DatosEstudianteProcesar[iIndice, 4] + "_2.pdf";
                                                if (File.Exists(ArchivoDestino))
                                                {
                                                    ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 2] + "_" + DatosEstudianteProcesar[iIndice, 4] + "_3.pdf";
                                                    if (File.Exists(ArchivoDestino))
                                                    {
                                                        ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 2] + "_" + DatosEstudianteProcesar[iIndice, 4] + "_4.pdf";
                                                        if (File.Exists(ArchivoDestino))
                                                        {
                                                            ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 2] + "_" + DatosEstudianteProcesar[iIndice, 4] + "_5.pdf";
                                                            if (File.Exists(ArchivoDestino))
                                                            {
                                                                ArchivoDestino = RutaFinal + Codigo + "_" + DatosEstudianteProcesar[iIndice, 2] + "_" + DatosEstudianteProcesar[iIndice, 4] + "_6.pdf";
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    /*string rutaArchivo = "C:\\xampp\\htdocs\\CapturaUdeG\\Cargados\\Archivos.txt";
                                    File.AppendAllText(rutaArchivo, "ArchivoOrigen " + ArchivoOrigen + " ArchivoDestino " + ArchivoDestino + Environment.NewLine);*/
                                    if (File.Exists(ArchivoDestino))
                                    {
                                        File.Delete(ArchivoDestino);
                                        Thread.Sleep(200);
                                    }                                    
                                    if (File.Exists(ArchivoOrigen))
                                    {
                                        File.Move(ArchivoOrigen, ArchivoDestino);
                                        Thread.Sleep(300);
                                    } else
                                    {
                                        MessageBox.Show("No se encontro Archivo Origen ==> " + ArchivoOrigen);
                                    }                                                                        
                                    ActualizaEstatusAlfresco("EN GENERACION DE ARCHIVO", DatosEstudianteProcesar[iIndice, 0], dFechaHora);
                                    if (string.IsNullOrEmpty(DatosEstudianteProcesar[iIndice, 2]))
                                    {
                                        writera.WriteLine(DatosEstudianteProcesar[iIndice, 0] + "|" + DatosEstudianteProcesar[iIndice, 1] + "|" + DatosEstudianteProcesar[iIndice, 3] + "|" + DatosEstudianteProcesar[iIndice, 5].Substring(6,4) + "|" + ArchivoDestino);
                                    }
                                    else
                                    {
                                        writera.WriteLine(DatosEstudianteProcesar[iIndice, 0] + "|" + DatosEstudianteProcesar[iIndice, 2] + "|" + DatosEstudianteProcesar[iIndice, 3] + "|" + DatosEstudianteProcesar[iIndice, 5].Substring(6, 4) + "|" + ArchivoDestino);
                                    }
                                }
                                else
                                {
                                    ActualizaEstatusAlfresco("ARCHIVO PDF NO EXISTENTE EN EL ORIGEN", DatosEstudianteProcesar[iIndice, 0], dFechaHora);
                                }
                            }
                        }
                        writera.Close();
                        int lineas = File.ReadAllLines(rutaArchivo1).Length - 1;
                        textBox3.Text = lineas.ToString();
                        textBox3.Refresh();
                        int iTotal = int.Parse(textBox3.Text);
                        int cociente = 0;
                        if (iTotal > 99)
                        {
                            cociente = iTotal / 100;
                        }
                        else
                        {
                            cociente = 100 / iTotal;
                        }
                        progressBar1.Value = 0; // Reiniciar barra
                        Cursor.Current = Cursors.WaitCursor;
                        StreamReader ReaderObject = new StreamReader(rutaArchivo1, System.Text.Encoding.Default);
                        StreamWriter writer = new StreamWriter(rutaArchivo2, true,System.Text.Encoding.Default);
                        writer.WriteLine("Nombre Pdf,Codigo Escolar,Tipo de Documento,Año Titulacion, Ruta Archivo,Resultado,Nodo");
                        string Line;
                        int iLine = 0;
                        while ((Line = ReaderObject.ReadLine()) != null)
                        {
                            if (iLine > 0)
                            {
                                textBox2.Text = iLine.ToString();
                                textBox2.Refresh();
                                textBox4.Text = Line;
                                textBox4.Refresh();
                                string[] campos = Line.Split('|');
                                if (campos.Length > 2)
                                {
                                    //IDExpediente|Sitio|TipoDoc|CodigoDocente|RequisitoRubro(0 requisito 1 rubro)|Item|Ruta
                                    string Codigo = campos[1];
                                    string TipoDocumento = campos[2];
                                    string AñoTitulacion = campos[3];
                                    string Ruta = campos[4];
                                    string sError = string.Empty;
                                    string[] campos2 = Ruta.Split('\\');
                                    string Nombre = campos2[campos2.Length - 1];
                                    Nombre = Nombre.Substring(0, Nombre.Length - 4);
                                    if (Ruta.Equals(""))
                                    {
                                        sError = sError + "|El Documento a cargar es Obligatorio;";
                                        //Line = Line.Replace('|', ',');
                                        writer.WriteLine(Line + sError);
                                    }
                                    else
                                    {
                                        if (!File.Exists(Ruta))
                                        {
                                            sError = sError + "|El Documento a cargar No Existe;";
                                            //Line = Line.Replace('|', ',');
                                            writer.WriteLine(Line + sError);
                                        }
                                        else
                                        {
                                            FileInfo fileInfo = new FileInfo(Ruta);
                                            string sExt = fileInfo.Extension;
                                            long fileSize = fileInfo.Length; // Tamaño del archivo en bytes
                                            if (sExt.Equals(".pdf"))
                                            {
                                                if (TipoDocumento.Equals(""))
                                                {
                                                    sError = sError + "El Tipo de Documento es Obligatorio;";
                                                }
                                                if (Codigo.Equals(""))
                                                {
                                                    sError = sError + "El Codigo es Obligatorio;";
                                                }
                                                if (sError == string.Empty)
                                                {
                                                    ProcesaAlfresco(Codigo, AñoTitulacion, TipoDocumento, Ruta, writer, Line, Nombre);
                                                    if (bOk.Equals("SI"))
                                                    {
                                                        //Quitar insertaRegistroCargaMasiva(int.Parse(Codigo), int.Parse(Item), sNodo, short.Parse(Requisito));
                                                        if (iTotal > 99)
                                                        {
                                                            int residuo = iLine % cociente;
                                                            if (residuo == 0)
                                                            {
                                                                if (iLine == int.Parse(textBox3.Text))
                                                                {
                                                                    iAvance = 100;
                                                                }
                                                                else
                                                                {
                                                                    iAvance = iAvance + 1;
                                                                    if (iAvance > 100)
                                                                    {
                                                                        iAvance = 100;
                                                                    }
                                                                }
                                                                progressBar1.Value = iAvance;
                                                                Application.DoEvents(); // Permite actualizar la interfaz gráfica
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (iLine == int.Parse(textBox3.Text))
                                                            {
                                                                iAvance = 100;
                                                            }
                                                            else
                                                            {
                                                                iAvance = iLine * cociente;
                                                                if (iAvance > 100)
                                                                {
                                                                    iAvance = 100;
                                                                }
                                                            }
                                                            progressBar1.Value = iAvance;
                                                            Application.DoEvents(); // Permite actualizar la interfaz gráfica
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //Line = Line.Replace('|', ',');
                                                    writer.WriteLine(Line + " |" + sError);
                                                }
                                            }
                                            else
                                            {
                                                //Line = Line.Replace('|', ',');
                                                writer.WriteLine(Line + "|Extension de archivo no valido");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //Line = Line.Replace('|', ',');
                                    writer.WriteLine(Line + "|Faltan Datos (algunos campos no se incluyeron)");
                                }
                                iLine = iLine + 1;
                            }
                            else
                            {
                                iLine = iLine + 1;
                            }
                        }
                        //Application.Exit();
                        Cursor.Current = Cursors.Arrow;
                        ReaderObject.Close();
                        writer.Close();
                        StreamReader ReaderObject2 = new StreamReader(rutaArchivo2, System.Text.Encoding.Default);
                        int iLine2 = 0;
                        while ((Line = ReaderObject2.ReadLine()) != null)
                        {
                            if (iLine2 > 0)
                            {
                                string[] partes = Line.Split('|');
                                string sNombrePdf = partes[0];
                                string sCodigoEscolar = partes[1];
                                string sArchivoPdf = partes[4];
                                string sEstatusAlfresco = partes[5];
                                ActualizaEstatusAlfresco(sEstatusAlfresco, sNombrePdf, dFechaHora);
                                InsertaLog(dFechaHora + iLine2.ToString(), VariablesGlobales.Usuario, Line);
                                if (!sEstatusAlfresco.StartsWith("Cargado Exitosamente"))
                                {
                                    if (File.Exists(sArchivoPdf))
                                    {
                                        string sRegreso = "C:\\xampp\\htdocs\\CapturaUdeG\\pdf\\" + sNombrePdf;
                                        if (File.Exists(sRegreso))
                                        {
                                            File.Delete(sRegreso);
                                            Thread.Sleep(200);
                                        }
                                        if (File.Exists(sArchivoPdf))
                                        {
                                            File.Move(sArchivoPdf, sRegreso);
                                            Thread.Sleep(300);
                                        }
                                        else
                                        {
                                            MessageBox.Show("No se encontro Archivo Regreso ==> " + sRegreso);
                                        }                                        
                                    }
                                }
                            }
                            iLine2 = iLine2 + 1;
                        }
                        ReaderObject2.Close();
                        Cursor.Current = Cursors.Arrow;
                        button1.Enabled = true;
                        MessageBox.Show("Archivo Procesado");
                        //Application.Exit();
                    }
                }
                else
                {
                    Cursor.Current = Cursors.Arrow;
                    MessageBox.Show("Operación cancelada.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    button1.Enabled = true;
                }
            }
            else
            {
                Cursor.Current = Cursors.Arrow;
                MessageBox.Show("No se ha seleccionado archivo a procesar", "Estatus de Archivo Seleccionado");
                button1.Enabled = true;
            }
        }

        private void Automatico_Load(object sender, EventArgs e)
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
            GeneraCadena();
            //MessageBox.Show(connectionString);
        }
    }
}
