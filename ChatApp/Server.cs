using ChatCommon;
using ChatDB;

namespace ChatApp
{
    public class Server<T>
    {
        private readonly Dictionary<string, T> _clients = new();
        private readonly IServerMessageSource<T> _messageSource;
        private T _endPoint;
        private bool _work;

        public Server(IServerMessageSource<T> messageSource)
        {
            _messageSource = messageSource;
            _endPoint = _messageSource.CreateEndPoint();
        }

        public async Task Start()
        {
            _work = true;
            await Listen();
        }

        private async Task ProcessMessage(NetMessage message)
        {
            if (message == null) return;

            switch (message.Command)
            {
                case Command.Register:
                    await Register(message);
                    break;
                case Command.Message:
                    await Reply(message);
                    break;
                case Command.Confirmation:
                    await ConfirmMessageReceived(message.Id);
                    break;
            }
        }

        private async Task Register(NetMessage message)
        {
            Console.WriteLine($"User registered: {message.From}");
            if (_clients.TryAdd(message.From, _endPoint))
            {
                using (var context = new ChatContext())
                {
                    context.Users.Add(new User { FullName = message.From });
                    await context.SaveChangesAsync();

                    message.To = message.From;
                    message.From = "Server";
                    message.Text = "You are registered";
                    message.Command = Command.Confirmation;

                    await _messageSource.SendAsync(message, _endPoint);
                }
            }
        }

        private async Task Reply(NetMessage message)
        {
            if (_clients.TryGetValue(message.To, out T endPoint))
            {
                int id = 0;
                using (var context = new ChatContext())
                {
                    var fromUser = context.Users.FirstOrDefault(x => x.FullName == message.From);
                    var toUser = context.Users.FirstOrDefault(x => x.FullName == message.To);

                    if (fromUser != null && toUser != null)
                    {
                        var msg = new Message
                        {
                            From = fromUser,
                            To = toUser,
                            IsSent = false,
                            Text = message.Text
                        };

                        context.Messages.Add(msg);
                        await context.SaveChangesAsync();
                        id = msg.Id;
                    }
                }

                message.Id = id;
                await _messageSource.SendAsync(message, endPoint);
                Console.WriteLine($"Message replied from: {message.From} to: {message.To}");
            }
            else
            {
                Console.WriteLine("User not found");
            }
        }

        private async Task ConfirmMessageReceived(int id)
        {
            Console.WriteLine($"Message confirmation id: {id}");
            using (var context = new ChatContext())
            {
                var msg = context.Messages.FirstOrDefault(x => x.Id == id);
                if (msg != null)
                {
                    msg.IsSent = true;
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task Listen()
        {
            Console.WriteLine("Server is waiting for messages");

            while (_work)
            {
                try
                {
                    var message = await Task.Run(() => _messageSource.Receive(ref _endPoint));
                    Console.WriteLine(message);
                    await ProcessMessage(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public void Stop()
        {
            _work = false;
        }
    }
}
