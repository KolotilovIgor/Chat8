using ChatCommon;

namespace ChatApp
{
    public class Client<T>
    {
        private readonly string _name;
        private readonly IClientMessageSource<T> _messageSource;
        private T _endPoint;
        private bool _work = true;

        public Client(string name, IClientMessageSource<T> source)
        {
            _name = name;
            _messageSource = source;
            _endPoint = _messageSource.GetServer();
        }

        public async Task Start()
        {
            var sendThread = new Thread(async () => await SendAsync());
            sendThread.Start();
            var listenThread = new Thread(async () => await Listen());
            listenThread.Start();
        }

        public void Stop() => _work = false;

        private async Task Listen()
        {
            Console.WriteLine("Client is waiting for messages");
            while (_work)
            {
                try
                {
                    var messageReceived = await Task.Run(() => _messageSource.Receive(ref _endPoint));
                    Console.WriteLine($"Received message from '{messageReceived.From}':");
                    Console.WriteLine(messageReceived.Text);

                    await Confirm(messageReceived, _endPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private async Task SendAsync()
        {
            await Register(_endPoint);
            while (_work)
            {
                try
                {
                    Console.WriteLine("Enter recipient name:");
                    var recName = Console.ReadLine();

                    Console.WriteLine("Enter message text:");
                    var mesText = Console.ReadLine();

                    var message = new NetMessage(mesText, Command.Message, _name, recName);

                    await _messageSource.SendAsync(message, _endPoint);

                    Console.WriteLine("Message sent");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private async Task Confirm(NetMessage msg, T endPoint)
        {
            msg.Command = Command.Confirmation;

            await _messageSource.SendAsync(msg, endPoint);
        }

        private async Task Register(T endPoint)
        {
            var msg = new NetMessage("", Command.Register, _name, "");

            await _messageSource.SendAsync(msg, endPoint);
        }
    }
}
