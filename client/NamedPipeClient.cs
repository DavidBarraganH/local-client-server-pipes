using System;
using System.IO.Pipes;
using System.Text;
using System.Diagnostics;
using System.Threading;

class NamedPipeClient
{
    static volatile bool salir = false;

    static void Main()
    {
        Console.WriteLine("[CLIENT] Esperando servidor");
        string mensajeDefault = "Little"; // valor por defecto

        using (var pipeClient = new NamedPipeClientStream(".", "TestPipe", PipeDirection.InOut))
        {
            pipeClient.Connect();
            var buffer = new byte[256];
            var encoder = Encoding.UTF8;

            // Esperar READY del servidor
            int readyBytes = pipeClient.Read(buffer, 0, buffer.Length);
            string readyMessage = encoder.GetString(buffer, 0, readyBytes);
            if (!readyMessage.StartsWith("READY"))
            {
                Console.WriteLine("[CLIENT] El servidor no está listo.");
                return;
            }
            Console.WriteLine("[CLIENT] Servidor está listo.");


            Console.WriteLine("[CLIENT] Conectando a servidor...");

            Console.Write("¿Desea asignar un mensaje del cliente? (s/n): ");
            string respuesta = Console.ReadLine()?.Trim().ToLower();
            if (respuesta == "s")
            {
                Console.Write("Ingrese el mensaje del cliente: ");
                mensajeDefault = Console.ReadLine();
            }

            // Hilo que espera Enter en cualquier momento
            new Thread(() =>
            {
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        salir = true;
                        break;
                    }
                }
            }).Start();

            int mensajeId = 0;

            while (!salir)
            {
                mensajeId++;
                var sentTime = DateTime.Now;
                double sentMsOfDay = sentTime.TimeOfDay.TotalMilliseconds;
                string horaEnvio = sentTime.ToString("HH:mm:ss.fffff");

                string mensajeConId = $"{mensajeId}|{mensajeDefault}";
                byte[] dataToSend = encoder.GetBytes(mensajeConId);

                pipeClient.Write(dataToSend, 0, dataToSend.Length);
                pipeClient.Flush();

                Console.WriteLine($"[CLIENT] Enviado: {mensajeDefault} | ID: {mensajeId} | Hora: {horaEnvio} | ms: {sentMsOfDay:F3} ms");

                int bytesRead = pipeClient.Read(buffer, 0, buffer.Length);
                var receivedTime = DateTime.Now;
                double receivedMsOfDay = receivedTime.TimeOfDay.TotalMilliseconds;
                string horaRecibido = receivedTime.ToString("HH:mm:ss.fffff");

                string response = encoder.GetString(buffer, 0, bytesRead);
                double latenciaCalculada = receivedMsOfDay - sentMsOfDay;

                Console.WriteLine($"[CLIENT] Recibido: {response} | ID: {mensajeId} | Hora: {horaRecibido} | ms del día: {receivedMsOfDay:F3} ms");
                Console.WriteLine($"[CLIENT] Latencia ida/vuelta (calculada): {latenciaCalculada:F5} ms");
                Console.WriteLine("---");

                Thread.Sleep(1000); // Esperar 1 segundo antes del siguiente envío

            }

            string exitMsg = "EXIT";
            byte[] exitData = encoder.GetBytes(exitMsg);
            pipeClient.Write(exitData, 0, exitData.Length);
            pipeClient.Flush();
            Console.WriteLine("[CLIENT] Finalizando cliente y servidor...");
        }
    }
}
