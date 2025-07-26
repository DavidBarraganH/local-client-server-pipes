

using System;
using System.IO.Pipes;
using System.Text;

class NamedPipeServer
{
    static void Main()
    {
        Console.WriteLine("[SERVER] ¿Desea asignar una respuesta del servidor? (s/n)");
        string respuestaServidor = Console.ReadLine()?.Trim().ToLower() == "s"
            ? LeerInput("Ingrese la respuesta a enviar (servidor):")
            : "Caesars";

        Console.WriteLine("[SERVER] Esperando conexión...");

        using (var pipeServer = new NamedPipeServerStream("TestPipe", PipeDirection.InOut))
        {
            pipeServer.WaitForConnection();
            // Enviar mensaje de listo
            string readyMessage = "READY";
            pipeServer.Write(Encoding.UTF8.GetBytes(readyMessage));
            pipeServer.Flush();
            Console.WriteLine("[SERVER] Esperando mensaje del cliente.");

            var buffer = new byte[256];
            var encoder = Encoding.UTF8;

            while (true)
            {
                int bytesRead = pipeServer.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) continue;

                string recibido = encoder.GetString(buffer, 0, bytesRead);
                string[] partes = recibido.Split('|');
                string id = partes.Length > 1 ? partes[0] : "?";
                string contenido = partes.Length > 1 ? partes[1] : recibido;

                var recibidoAhora = DateTime.Now;
                double recibidoMs = recibidoAhora.TimeOfDay.TotalMilliseconds;

                Console.WriteLine($"[SERVER] Recibido: {contenido} | ID: {id} | Hora: {recibidoAhora:HH:mm:ss.fffff} | ms del día: {recibidoMs:F3} ms");

                string respuesta = respuestaServidor;
                byte[] responseData = encoder.GetBytes(respuesta);

                if (id == "EXIT")
                {
                    Console.WriteLine("[SERVER] Ejecución finalizada");
                    break;
                }

                pipeServer.Write(responseData, 0, responseData.Length);
                pipeServer.Flush();

                var enviadoAhora = DateTime.Now;
                double enviadoMs = enviadoAhora.TimeOfDay.TotalMilliseconds;

                Console.WriteLine($"[SERVER] Enviado: {respuestaServidor} | ID: {id} | Hora: {enviadoAhora:HH:mm:ss.fffff} | ms del día: {enviadoMs:F3} ms");
                Console.WriteLine("---");
            }
        }
    }

    static string LeerInput(string prompt)
    {
        Console.WriteLine(prompt);
        string input;
        do input = Console.ReadLine(); while (string.IsNullOrWhiteSpace(input));
        return input;
    }
}
