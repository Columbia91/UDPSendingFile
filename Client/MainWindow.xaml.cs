using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string fileName;
        private string filePath;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Текстовые файлы|*.txt|Все файлы|*.*";
                if (dialog.ShowDialog() == true)
                {
                    using (FileStream fileStream = new FileStream(dialog.FileName, FileMode.Open))
                    {
                        fileName = System.IO.Path.GetFileName(dialog.FileName);
                        filePath = filePathTextBox.Text = dialog.FileName;
                    }
                }
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show(exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ipTextBox.Text) && !string.IsNullOrWhiteSpace(portTextBox.Text)
                && !string.IsNullOrWhiteSpace(blockSizeTextBox.Text)
                && !string.IsNullOrWhiteSpace(filePathTextBox.Text))
            {
                try
                {
                    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    string ipServer = ipTextBox.Text;
                    int port = int.Parse(portTextBox.Text);
                    EndPoint srvEP = new IPEndPoint(IPAddress.Parse(ipServer), port);

                    client.SendTo(Encoding.UTF8.GetBytes(fileName), srvEP);

                    using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
                    {
                        long cntRecive = stream.Length / int.Parse(blockSizeTextBox.Text);
                        long remainder = stream.Length % int.Parse(blockSizeTextBox.Text);

                        if (remainder > 0) cntRecive++;
                        client.SendTo(BitConverter.GetBytes((int)cntRecive), srvEP);

                        byte[] buffer = new byte[int.Parse(blockSizeTextBox.Text)];

                        for (int i = 0; i < (remainder > 0 ? cntRecive - 1 : cntRecive); i++)
                        {
                            stream.Read(buffer, 0, int.Parse(blockSizeTextBox.Text));
                            client.SendTo(buffer, srvEP);
                        }

                        if (remainder > 0)
                        {
                            stream.Read(buffer, 0, (int)remainder);
                            client.SendTo(buffer, 0, (int)remainder, SocketFlags.None, srvEP);
                        }
                    }
                }
                catch (ArgumentNullException ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (SocketException ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                MessageBox.Show("Не все поля заполнены", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
