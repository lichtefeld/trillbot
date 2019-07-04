using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JsonFlatFileDataStore;
using Newtonsoft.Json;

namespace trillbot.Classes
{
  public partial class Server
    {
        [JsonProperty("id")]
        public long ID { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("snowflake")]
        public ulong snowflake { get; set; }
        [JsonProperty("admins")]
        public List<ulong> adminSnowflakes { get; set; } = new List<ulong>();
        [JsonProperty("adminRole")]
        public List<ulong> adminRoleSnowflakes { get; set; } = new List<ulong>();
        [JsonProperty("racingChannels")]
        public List<ulong> racingChannelSnowflakes { get; set; } = new List<ulong>();
        [JsonProperty("racingVersionDefault")]
        public int racingVersionDefault { get; set; }
        [JsonProperty("startingBalance")]
        public int startingBalance { get; set; }

        public bool isAdmin(IGuildUser User) {
          foreach (var r in User.RoleIds) {
            if (adminRoleSnowflakes.Contains(r)) return true;
          }
          if (adminSnowflakes.Contains(User.Id)) return true;
          if (User.Id == 106768024857501696) return true;
          return false;
        }

        public bool isRacingChannel(SocketChannel Channel) {
          return racingChannelSnowflakes.Contains(Channel.Id);
        }
    }
  public partial class Server
    {
        public static Server[] FromJson(string json) => JsonConvert.DeserializeObject<Server[]>(json, Converter.Settings);
        public static List<Server> get_Server () {
            var store = new DataStore ("Server.json");

            var rtrnr = store.GetCollection<Server> ().AsQueryable ().ToList();
            store.Dispose();

            // Get employee collection
            return rtrnr;
        }

        public static Server get_Server (int id) {
            var store = new DataStore ("Server.json");

            // Get employee collection
            var rtrnr = store.GetCollection<Server> ().AsQueryable ().FirstOrDefault (e => e.ID == id);
            store.Dispose();
            return rtrnr;
        }

        public static Server get_Server (string name) {
            var store = new DataStore ("Server.json");

            // Get employee collection
            var rtrnr = store.GetCollection<Server> ().AsQueryable ().FirstOrDefault (e => e.Title == name);
            store.Dispose();
            return rtrnr;
        }

        public static Server get_Server (ulong snowflake) {
            var store = new DataStore ("Server.json");

            // Get employee collection
            var rtrnr = store.GetCollection<Server> ().AsQueryable ().FirstOrDefault (e => e.snowflake == snowflake);
            store.Dispose();
            return rtrnr;
        }

        public static void insert_Server (Server Server) {
            var store = new DataStore ("Server.json");

            // Get employee collection
            store.GetCollection<Server> ().InsertOneAsync (Server);

            store.Dispose();
        }

        public static void replace_Server (Server Server) {
            var store = new DataStore ("Server.json");

            store.GetCollection<Server> ().ReplaceOneAsync (e => e.ID == Server.ID, Server);
            store.Dispose();
        }

        public static void delete_card (Server Server) {
            var store = new DataStore ("Server.json");

            store.GetCollection<Card> ().DeleteOne (e => e.ID == Server.ID);
            store.Dispose();
        }
    }
}