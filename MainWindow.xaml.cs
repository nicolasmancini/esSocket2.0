using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;

namespace esSocketServer
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int PortNumber = 8000;
        private readonly StringBuilder messageHistory = new StringBuilder();
        private string ServerIpAddress = "x.x.x.x";



      


        public MainWindow() 
        { 
            InitializeComponent();

            // Creazione di un socket UDP in ascolto
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Prendi ip locali e fermati al primo
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in localIPs)
            {
                //per ip v4
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    ServerIpAddress = addr.ToString();
                    break;
                }
            }

            // Associazione del socket ad un endpoint locale e attesa di connessioni in ingresso
            listener.Bind(new IPEndPoint(IPAddress.Any, PortNumber));

            // Creazione di un thread che si mette in ascolto di eventuali richieste di connessione
            Task.Run(() =>
            {
                // Ricezione di dati dal client
                byte[] buffer = new byte[1024];
                EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                int bytesRead = listener.ReceiveFrom(buffer, ref endPoint);
                string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
 
                // Aggiunta dei dati ricevuti alla cronologia dei messaggi e aggiornamento della visualizzazione
                messageHistory.Append(data);
                messageHistory.Append(Environment.NewLine);
                Dispatcher.Invoke(() => testo.Text = messageHistory.ToString());

                // Invio di una risposta al client
                string response = "CONNESSIONE STABILITA CON " + ServerIpAddress + " " + PortNumber;
                byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                listener.SendTo(responseBytes, endPoint);

                // Chiusura del socket di ascolto
                listener.Close();
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           // Disabilita il pulsante di avvio
           start.IsEnabled = false;

            // Crea un nuovo thread per ricevere i dati in arrivo
            Thread receiveThread = new Thread(() =>
            {
                while (true)
                {
                    // Crea un nuovo socket per ricevere i dati
                    Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    receiver.Bind(new IPEndPoint(IPAddress.Any, PortNumber));

                  
                    // Ricevi i dati dal client
                    byte[] buffer = new byte[1024];
                    EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                    int bytesRead = receiver.ReceiveFrom(buffer, ref endPoint);
                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    // Aggiungi il messaggio alla cronologia dei messaggi
                    messageHistory.Append(data);
                    messageHistory.Append(Environment.NewLine);

                    // Aggiorna la visualizzazione del messaggio
                    AggiornaInterfaccia(data, false);

                    // Chiudi il socket del ricevitore
                    receiver.Close();
                }
            });

            // Avvia il thread
            receiveThread.Start();







        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

            // Prende il testo inserito dall'utente
            string message = MessageInput.Text;
            // Prende il tempo attuale
            DateTime localTime = DateTime.Now;
            // Crea il messaggio con indirizzo IP del server, porta, tempo e il messaggio stesso
            message = ServerIpAddress + " " + PortNumber + " " + localTime + ": " + message;
            // Aggiunge il messaggio alla cronologia dei messaggi
            messageHistory.Append(message);
            messageHistory.Append(Environment.NewLine);
            // Aggiorna la visualizzazione dei messaggi
            AggiornaInterfaccia(message, true);

            // Crea un nuovo thread e esegue il codice per l'invio di dati al server in quel thread
            Thread sendThread = new Thread(() =>
            {
                // Crea un socket UDP
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                
                // Connette il client al server
                IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse(ServerIpAddress), 8001);
                client.Connect(serverEndpoint);

                // Invia i dati al server
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                client.Send(buffer, buffer.Length, SocketFlags.None);

                // Chiude il socket del client
                client.Close();
            });

            // Avvia il thread
            sendThread.Start();
        }
        private void AggiornaInterfaccia(string message, bool isSentMessage)
        {
            Dispatcher.Invoke(() =>
            {

                // Creo un nuovo oggetto TextBlock per contenere il messaggio
                var textBlock = new TextBlock();
                textBlock.Text = message;
                textBlock.Margin = new Thickness(5);

                // Creo un nuovo oggetto Border per inserire il TextBlock e dare una forma al messaggio
                var border = new Border();
                border.CornerRadius = new CornerRadius(10);
                border.Margin = new Thickness(5);
                border.Child = textBlock;

                // Controllo se il messaggio è stato inviato o ricevuto per impostare la sua posizione e l'allineamento del testo
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

                // Aggiungo il bordo al contenitore StackPanel per visualizzare il messaggio sulla finestra
                stackp2.Children.Add(border);
            });
        }

      
    }

}
