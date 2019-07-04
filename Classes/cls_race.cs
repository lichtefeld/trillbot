using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JsonFlatFileDataStore;
using Newtonsoft.Json;
using trillbot.Classes;

namespace trillbot.Classes {

    public class race {
      [JsonProperty("ID")]
      public int ID { get; set; }
      [JsonProperty("serverID")]
      public ulong discord_server_id { get; set; }
      [JsonProperty("racersBets")]
      public List<racerBet> racersWithBets { get; set; } = new List<racerBet>();
      [JsonProperty("bets")]
      public List<Bet> bets { get; set; } = new List<Bet>();
      [JsonProperty("totalPool")]
      public int[] totalPool { get; set; } = {0,0,0};
      [JsonProperty("cut")]
      public double cut { get; set; } = 0.1;
      [JsonProperty("acceptingBets")]
      public bool acceptingBets { get; set; } = true;

      public void makePayouts(int win, int place, int show, SocketCommandContext Context) {
        racerBet winner = racersWithBets.FirstOrDefault(e=> e.ID == win);
        racerBet second = racersWithBets.FirstOrDefault(e=> e.ID == place);
        racerBet third = racersWithBets.FirstOrDefault(e=> e.ID == show);

        foreach (var b in bets) {
          var c = Character.get_character(b.characterID);
          int amount = 0;
          if (b.RacerID == win) {
            switch(b.Type) {
              case "win":
                amount += b.Amount + (int)Math.Floor(b.Amount * winner.payouts[0]);
              break;
              case "place":
                amount += b.Amount + (int)Math.Floor(b.Amount * winner.payouts[1]);
              break;
              case "show":
                amount += b.Amount + (int)Math.Floor(b.Amount * winner.payouts[2]);
              break;
            }
          } else if (b.RacerID == place) {
              switch(b.Type) {
              case "place":
                amount += b.Amount + (int)Math.Floor(b.Amount * second.payouts[1]);
              break;
              case "show":
                amount += b.Amount + (int)Math.Floor(b.Amount * second.payouts[2]);
              break;
            }
          } else if (b.RacerID == show) {
            switch(b.Type) {
              case "show":
                amount += b.Amount + (int)Math.Floor(b.Amount * third.payouts[2]);
              break;
            }
          }
          if (amount > 0) {
            c.balance += amount;
            Character.update_character(c);
            var usr = Context.Guild.GetUser(c.player_discord_id);
            if (usr != null) usr.SendMessageAsync("You won a " + b.Type + " bet on race ID: " + ID + ". It paid you " + amount + " of imperial credits.");
            Thread.Sleep(100);
          }
        }
      }

      public void updatePayouts() {
        foreach (var r in racersWithBets) {
          for(int i = 0; i < 3; i++) {
            double amt = r.bets[i] * (1-cut);
            r.payouts[i] = amt / (double) totalPool[i];
          }
        }
      }

      public void addBet(Bet b) {
        bets.Add(b);
        var r = racersWithBets.FirstOrDefault(e=>e.ID == b.RacerID);
        switch (b.Type) {
          case "win":
            totalPool[0] += b.Amount;
            r.bets[0] += b.Amount;
          break;
          case "place":
            totalPool[1] += b.Amount;
            r.bets[1] += b.Amount;
          break;
          case "show":
            totalPool[2] += b.Amount;
            r.bets[2] += b.Amount;
          break;
        }
      }

      public void displayPayouts(SocketCommandContext Context) {
        //   Racer ID  | ID1 | ID2 | IDi
        //   Win Odds  |  5  | 
        //  Place Odds |  3  |
        //   Show Odds |  2  |
        List<string> str = new List<string>();
        str.Add("```md");
        str.Add("/* Race ID: " + ID + " *");
        str.Add("< Odds are listed per 1 credit bet. Minimum bet is 2 credits. >");
        List<string> strs = new List<string>();
        for (int z = 0; z < 4; z++ ) {
          for(int i = 0; i < racersWithBets.Count; i++) {
            if(z == 0) {
              if (i == 0 ) {
                strs.Add(helpers.center(payouts_to_line[i],10));
              }
              strs.Add(helpers.center(racersWithBets[i].ID.ToString(),5));
            } else {
              if (i == 0) {
                strs.Add(helpers.center(payouts_to_line[i],10));
              }
              strs.Add(helpers.center(((int)Math.Round(racersWithBets[i].payouts[z-1])).ToString(),5));
            }
          }
          str.Add(String.Join(" | ",strs));
        }
        str.Add("```");
        helpers.output(Context.Channel,str);
      }

      public static Dictionary<int,string> payouts_to_line = new Dictionary<int, string> {
        {0,"RacerID"},
        {1,"Win Odds"},
        {2,"Place Odds"},
        {3,"Show Odds"}
      };

      public static List<race> get_race () {
        var store = new DataStore ("race.json");

          var rtner = store.GetCollection<race> ().AsQueryable ().ToList();

          store.Dispose();

          // Get employee collection
          return rtner;
      }

      public static race get_race (int id) {
          var store = new DataStore ("race.json");

          // Get employee collection
          var rtner = store.GetCollection<race> ().AsQueryable ().FirstOrDefault (e => e.ID == id);
          store.Dispose();

          // Get employee collection
          return rtner;
      }

      public static race get_race (ulong id) {
          var store = new DataStore ("race.json");

          // Get employee collection
          var rtner = store.GetCollection<race> ().AsQueryable ().FirstOrDefault (e => e.discord_server_id == id);
          store.Dispose();

          // Get employee collection
          return rtner;
      }

      public static void insert_race (race race) {
          var store = new DataStore ("race.json");

          // Get employee collection
          store.GetCollection<race> ().InsertOneAsync (race);

          store.Dispose();
      }

      public static void update_race (race race) {
          var store = new DataStore ("race.json");

          store.GetCollection<race> ().ReplaceOne (e => e.ID == race.ID, race);
          store.Dispose();
      }

      public static void replace_race(race race) {
          var store = new DataStore ("race.json");

          store.GetCollection<race> ().ReplaceOne (e => e.ID == race.ID, race);
          store.Dispose();
      }

      public static void delete_race (race race) {
          var store = new DataStore ("race.json");

          store.GetCollection<race> ().DeleteOne (e => e.ID == race.ID);
          store.Dispose();
      }
    }

    public class racerBet {
      public int ID;
      public int[] bets = {0,0,0};
      public double[] payouts = {0.0,0.0,0.0};

      public racerBet (int i) {
        ID = i;
      }
    }
}