using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;

namespace esSocketClient
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ServerIpAddress = "x.x.x.x";
        private int PortNumber = 8001;

        private readonly StringBuilder messageHistory = new StringBuilder();
        public MainWindow()
        {
            InitializeComponent();
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());// Prendi ip locali e tieni il primo
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    ServerIpAddress = addr.ToString();
                    break;
                }
            }

            // Crea socket
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Connessione al server
            IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse(ServerIpAddress), 8000);
            client.Connect(serverEndpoint);
             
            string data = "CONNESSIONE STABILITA CON "+ServerIpAddress+" "+PortNumber;
            byte[] buffer = Encoding.ASCII.GetBytes(data); //Array di byte che contiene il messaggio

            // Invia un buffer di dati al server tramite una connessione client Socket
            client.Send(buffer, buffer.Length, SocketFlags.None);

            // Aggiunge i dati ricevuti alla cronologia dei messaggi
            messageHistory.Append(data);
            messageHistory.Append(Environment.NewLine);

            // Aggiorna la visualizzazione del testo nell'interfaccia utente tramite Dispatcher
            Dispatcher.Invoke(() => testo.Text = messageHistory.ToString());

            // Riceve una risposta dal server
            buffer = new byte[1024];
            EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            int bytesRead = client.ReceiveFrom(buffer, ref remoteEndpoint);

            // Converte i dati ricevuti in una stringa ASCII
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);


            testo.Text = response;


            // Chiudi socket
            client.Close();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // Ottieni il messaggio dalla casella di input dell'utente
            string message = MessageInput.Text;

            // Ottieni l'ora locale attuale
            DateTime localTime = DateTime.Now;

            // Costruisci il messaggio da inviare al server concatenando l'indirizzo IP del server, il numero di porta, l'ora locale e il messaggio dell'utente
            message = ServerIpAddress + " " + PortNumber + " " + localTime + ": " + message;

            // Aggiungi il messaggio alla cronologia dei messaggi
            messageHistory.Append(message);
            messageHistory.Append(Environment.NewLine);

            // Aggiorna la visualizzazione del messaggio nell'interfaccia utente
            AggiornaInterfaccia(message, true);

            // Crea un nuovo thread e esegui il codice per l'invio dei dati al server in quel thread
            Thread sendThread = new Thread(() =>
            {
                // Crea un socket UDP
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

              
                // Connettiti al server
                IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse(ServerIpAddress), 8000);
                client.Connect(serverEndpoint);

                // Invia i dati al server
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                client.Send(buffer, buffer.Length, SocketFlags.None);

                // Chiudi il socket 
                client.Close();
            });

            // Avvia il thread
            sendThread.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Disabilita il pulsante "start"
            start.IsEnabled = false;

            // Crea un nuovo thread per la ricezione dei dati dal server
            Thread receiveThread = new Thread(() =>
            {
                while (true)
                {
                    // Crea un nuovo socket per ricevere i dati
                    Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    receiver.Bind(new IPEndPoint(IPAddress.Any, PortNumber));

                    
                    // Ricevi i dati dal server
                    byte[] buffer = new byte[1024];
                    EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                    int bytesRead = receiver.ReceiveFrom(buffer, ref endPoint);
                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    // Aggiungi i dati alla cronologia dei messaggi
                    messageHistory.Append(data);
                    messageHistory.Append(Environment.NewLine);

                    // Aggiorna la visualizzazione dei messaggi nell'interfaccia utente
                    AggiornaInterfaccia(data, false);

                    // Chiudi il socket di ricezione
                    receiver.Close();
                }
            });

            // Avvia il thread di ricezione
            receiveThread.Start();
        }

        private void AggiornaInterfaccia(string message, bool isSentMessage)
        {
            Dispatcher.Invoke(() =>
            {
                // Crea un nuovo oggetto TextBlock per visualizzare il messaggio
                var textBlock = new TextBlock();
                textBlock.Text = message;
                textBlock.Margin = new Thickness(5);
 
                // Crea un nuovo oggetto Border per contenere il TextBlock
                var border = new Border();
                border.CornerRadius = new CornerRadius(10);
                border.Margin = new Thickness(5);
                border.Child = textBlock;

                // Imposta l'allineamento del border e del textBlock a destra se il messaggio è stato inviato, altrimenti a sinistra
                if (isSentMessage)
                {
                    border.HorizontalAlignment = HorizontalAlignment.Right;
                    textBlock.TextAlignment = TextAlignment.Right;
                    textBlock.Foreground = Brushes.Black;
                    textBlock.Background = Brushes.LightBlue;
                }
                else
                {
                    border.HorizontalAlignment = HorizontalAlignment.Left;
                    textBlock.TextAlignment = TextAlignment.Left;
                    textBlock.Foreground = Brushes.Black;
                    textBlock.Background = Brushes.LightBlue;
                }

                // Aggiungi il border alla StackPanel dell'interfaccia utente
                stackp.Children.Add(border);
            });
        }



    }



}
