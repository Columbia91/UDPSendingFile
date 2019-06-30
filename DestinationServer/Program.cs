using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace DestinationServer
{
    class Program
    {
        static Socket srvSock;
        static string ipServer = "0.0.0.0";
        static int portServer = 12345;
        static void Main(string[] args)
        {
            srvSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            srvSock.Bind(new IPEndPoint(IPAddress.Parse(ipServer), portServer));
            EndPoint ClientEndPoint = new IPEndPoint(0, 0);

            byte[] buffer = new byte[64*1024];
            // получить имя файла
            int recSize = srvSock.ReceiveFrom(buffer, ref ClientEndPoint);
            string FileName = Encoding.UTF8.GetString(buffer, 0, recSize);
            Console.WriteLine("File name: " + FileName);

            // Папка Документы
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + FileName;
            using (FileStream outFile = new FileStream(path, FileMode.OpenOrCreate))
            {
                // получить количество посылок
                srvSock.ReceiveFrom(buffer, ref ClientEndPoint);
                int cntRecive = BitConverter.ToInt32(buffer, 0);
                Console.WriteLine("Blocks count: " + cntRecive);

                // прием частей файла
                int fullSize = 0;
                for (int i = 0; i < cntRecive; i++)
                {
                    recSize = srvSock.ReceiveFrom(buffer, ref ClientEndPoint);
                    if (recSize > 0)
                    {
                        Console.WriteLine($"{i+1}) Receive {recSize} bytes   \r");
                        outFile.Write(buffer, 0, recSize);
                        fullSize += recSize;
                    }
                    else
                        break;
                }
                Console.WriteLine("\nFull size of file = {0}", fullSize);
                //outFile.Flush(true); // принудительный сброс данных
                //outFile.Close(); принудительное закрытие потока
            }
        }
    }
}
