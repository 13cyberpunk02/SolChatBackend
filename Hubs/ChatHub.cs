using Microsoft.AspNetCore.SignalR;
using System.Globalization;
namespace Service.Hubs
{
    public class ChatHub : Hub
    {
        private readonly string _botUser;
        private readonly IDictionary<string, UserConnection> _connections;
        public ChatHub(IDictionary<string,UserConnection> con)
        {
            _botUser = DateTime.Now.ToString("D", CultureInfo.CurrentCulture) 
                + " " + DateTime.Now.TimeOfDay.Hours.ToString() + ":" + DateTime.Now.TimeOfDay.Minutes.ToString(); 
            _connections = con;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {

            if(_connections.TryGetValue(Context.ConnectionId, out UserConnection? userConnection))
            {
                _connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.Room)
                .SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} покинул чат");


                SendUsersConnected(userConnection.Room);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string msg)
        {
            if(_connections.TryGetValue(Context.ConnectionId, out UserConnection? userConnection))
            {
                await Clients.Group(userConnection.Room)
                .SendAsync("ReceiveMessage", userConnection.User, msg);
            }
        }
        public async Task JoinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);

            _connections[Context.ConnectionId] = userConnection;
            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser,
            $"{userConnection.User} присоединился к {userConnection.Room}");

            await SendUsersConnected(userConnection.Room);
        }

        public Task SendUsersConnected(string room)
        {
            var users = _connections.Values
            .Where(c => c.Room == room)
            .Select(c => c.User);

            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }
    }
}