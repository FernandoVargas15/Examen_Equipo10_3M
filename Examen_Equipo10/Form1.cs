using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient; 

namespace Examen_Equipo10
{
    public partial class Form1 : Form
    {
        private SerialPort puertoSerial;
        private string ConectarSQL = "Server=localhost; Port=3306; Database=ExaPractica_04; Uid=root; Pwd=Rocket2014;";
        private double temperaturaActual; 

        public Form1()
        {
            InitializeComponent();
            btnApagar.Enabled = false;

            
            puertoSerial = new SerialPort("COM4", 9600); 
            puertoSerial.DataReceived += new SerialDataReceivedEventHandler(DatosRecibidos);
            puertoSerial.Open(); 
        }

        private void btnEncender_Click(object sender, EventArgs e)
        {
            if (puertoSerial.IsOpen)
            {
                puertoSerial.WriteLine("ENCENDER");
                btnEncender.Enabled = false;
                btnApagar.Enabled = true;

                estado.ForeColor = Color.Green;
                estado.Text = "Encendido"; 

                MessageBox.Show("Comando para encender enviado.");
            }
        }

        private void btnApagar_Click(object sender, EventArgs e)
        {
            if (puertoSerial.IsOpen)
            {
                puertoSerial.WriteLine("APAGAR");
                btnApagar.Enabled = false;
                btnEncender.Enabled = true;

                estado.ForeColor = Color.Red;
                estado.Text = "Apagado"; 

                MessageBox.Show("Comando para apagar enviado.");
            }
        }


        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string nombreUsuario = txtUsuario.Text; 
            DateTime fechaRegistro;

            if (DateTime.TryParse(txtFecha.Text, out fechaRegistro))
            {
                if (string.IsNullOrWhiteSpace(nombreUsuario))
                {
                    MessageBox.Show("Por favor ingresa un nombre de usuario.");
                    return;
                }

                try
                {
                    using (MySqlConnection conexion = new MySqlConnection(ConectarSQL))
                    {
                        conexion.Open();

                        string insertarUsuario = "INSERT INTO Usuarios (Nombre, FechaRegistro) VALUES (@Nombre, @Fecha)";
                        MySqlCommand cmdUsuario = new MySqlCommand(insertarUsuario, conexion);
                        cmdUsuario.Parameters.AddWithValue("@Nombre", nombreUsuario);
                        cmdUsuario.Parameters.AddWithValue("@Fecha", fechaRegistro);
                        cmdUsuario.ExecuteNonQuery();

                        int usuarioId = (int)cmdUsuario.LastInsertedId; 

                        string insertarLectura = "INSERT INTO Lecturas (UsuarioId, Temperatura, FechaHora) VALUES (@UsuarioId, @Temperatura, @FechaHora)";
                        MySqlCommand cmdLectura = new MySqlCommand(insertarLectura, conexion);
                        cmdLectura.Parameters.AddWithValue("@UsuarioId", usuarioId);
                        cmdLectura.Parameters.AddWithValue("@Temperatura", temperaturaActual);
                        cmdLectura.Parameters.AddWithValue("@FechaHora", DateTime.Now);
                        cmdLectura.ExecuteNonQuery();

                        MessageBox.Show("Datos guardados correctamente.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar los datos: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("La fecha no es válida.");
            }
        }

        private void DatosRecibidos(object sender, SerialDataReceivedEventArgs e)
        {
            string data = puertoSerial.ReadLine(); 

            if (data.StartsWith("Temperatura (C):"))
            {

                string tempString = data.Substring("Temperatura (C):".Length).Trim();
                double temperatura;
                if (double.TryParse(tempString, out temperatura))
                {
                    temperaturaActual = temperatura; 
                    this.Invoke(new Action(() => lbTemperatura.Text = temperatura.ToString("F1") + " °C"));
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (puertoSerial != null && puertoSerial.IsOpen)
            {
                puertoSerial.Close();
            }
        }
    }
}
