using System.Collections.Generic;
using System.Linq;
using JsonFlatFileDataStore;
using Newtonsoft.Json;

namespace trillbot.Classes
{
  public class textVersion {
    [JsonProperty ("ID")]
    public int ID { get; set; }
    [JsonProperty("name")]
    public string name { get; set; } = "Trilliant Grand Prix";
    [JsonProperty("description")]
    public string desc { get; set; } = "Default Flavoring of the mechanical game.";
    [JsonProperty("output")]
    private Dictionary<string, List<string>> output { get; set; } = new Dictionary<string, List<string>>();
    [JsonProperty("cardStore")]
    public string cardStore { get; set; } = "card.json";
    [JsonProperty("abilityStore")]
    public string abilityStore { get; set; } = "ability.json";
    [JsonProperty("id_to_condition")]
    public Dictionary<int, string> id_to_condition { get; set; } = new Dictionary<int, string> {
            {5,"You cannot move until you play a Dodge card."},
            {6, "You cannot move until you play a Dodge card."},
            {8, "You cannot move until you play a Tech Savvy card."},
            {9, "Can be removed by a Tech Savvy card. If you end your turn with both Sabotage and another Hazard, you explode."},
            {10, "Can be removed by a Cyber Healthcare card."},
            {11, "You cannot play Movement cards higher than 2. Can be removed by a Cyber Healthcare card."},
            {16, "You can not move this turn. Automatically clears after this turn without a remedy."},
            {17, "Can be removed by a Tech Savvy Card. You have 2 turns to solve this issue or you die."}
        };
    private static List<string> verify = new List<string>{"coreSyncFail","conditionDeath","abilitySave","escapePod","crash","escapePodEscape","peek","causeCrash","passiveTwo","luckPassive","fourUnitStart","gameOver","coreSync","switchPositionFail","stun","stunCounter","alive","dead"};
    
    private string reply(List<string> inputs, string org, bool inFirst) {
      string rtn = "";
      for(int i = 0; i < inputs.Count || i < output[org].Count; i++) {
        if (inFirst) {
          if (inputs.Count > i) rtn += inputs[i];
          if (output[org].Count > i ) rtn += output[org][i];
        } else {
          if (output[org].Count > i ) rtn += output[org][i];
          if (inputs.Count > i) rtn += inputs[i];
        }
      }
      return rtn;
    }

    public string coreSyncFailure(string name1, string name2) {
      List<string> input = new List<string>();
      input.Add(name1);
      input.Add(name2);
      return reply(input,"coreSyncFail",true);
    }

    public string deathByCondition(string name, string condition) {
      List<string> input = new List<string>();
      input.Add(name);
      input.Add(condition);
      return reply(input,"conditionDeath",true);
    }

    public string abilitySave(string name, string save, string condition) {
      List<string> input = new List<string>();
      input.Add(name);
      input.Add(save);
      input.Add(condition);
      return reply(input,"abilitySave",true);
    }

    public string escapePod(string name) {
      List<string> input = new List<string>();
      input.Add(name);
      return reply(input,"escapePod",false);
    }

    public string crash(string name, string crashed) {
      List<string> input = new List<string>();
      input.Add(name);
      input.Add(crashed);
      return reply(input,"crash",true);
    }

    public string escapePodEscape(string name) {
      List<string> input = new List<string>();
      input.Add(name);
      return reply(input,"escapePodEscape",false);
    }

    public string peek(string abName, string peekedInfo) {
      List<string> input = new List<string>();
      input.Add(abName);
      input.Add(peekedInfo);
      return reply(input,"peek",false);
    }

    public string causeCrash(string name, string distance) {
      List<string> input = new List<string>();
      input.Add(name);
      input.Add(distance);
      return reply(input,"causeCrash",true);
    }

    public string passiveTwo(string name, string ability) {
      List<string> input = new List<string>();
      input.Add(name);
      input.Add(ability);
      return reply(input,"passiveTwo",true);
    }

    public string luckPassive(string name, string roll) {
      List<string> input = new List<string>();
      input.Add(name);
      input.Add(roll);
      return reply(input,"luckPassive",true);
    }

    public string fourUnitStart(string name, string abName) {
      List<string> input = new List<string>();
      input.Add(name);
      input.Add(abName);
      return reply(input,"fourUnitStart",true);
    }

    public string gameOver() {
      return reply(new List<string>(),"gameOver",false);
    }

    public string coreSync(string name1, string name2) {
      List<string> input = new List<string>();
      input.Add(name1);
      input.Add(name2);
      return reply(input,"coreSync",true);
    }

    public string switchPositionFail(string name, string ability, string roll) {
      List<string> input = new List<string>();
      input.Add(name);
      input.Add(ability);
      input.Add(roll);
      return reply(input,"switchPositionFail",true);
    }

    public string stun(string name1, string name2) {
      List<string> input = new List<string>();
      input.Add(name1);
      input.Add(name2);
      return reply(input,"stun",true);
    }

    public string stunCounter(string name1, string abName, string name2) {
      List<string> input = new List<string>();
      input.Add(name1);
      input.Add(abName);
      input.Add(name2);
      return reply(input,"stunCounter",true);
    }

    public string leaderBoardAlive(bool alive) {
      List<string> input = new List<string>();
      if (alive) return reply(input,"alive",false);
      else return reply(input,"dead",false);
    }

    public static textVersion[] FromJson(string json) => JsonConvert.DeserializeObject<textVersion[]>(json, Converter.Settings);

    public override string ToString() {
        return "**" + name + "** : " + desc;
    }

    public static List<textVersion> get_textVersion () {
        var store = new DataStore ("textVersion.json");

        // Get employee collection
        var rtrner = store.GetCollection<textVersion> ().AsQueryable ().ToList();
        store.Dispose();
        return rtrner;
    }

    public static textVersion get_textVersion (int id) {
        var store = new DataStore ("textVersion.json");

        // Get employee collection
        var rtrner = store.GetCollection<textVersion> ().AsQueryable ().FirstOrDefault (e => e.ID == id);
        store.Dispose();
        return rtrner;
    }

    public static textVersion get_textVersion (string name) {
        var store = new DataStore ("textVersion.json");

        // Get employee collection
        var rtrner = store.GetCollection<textVersion> ().AsQueryable ().FirstOrDefault (e => e.name == name);
        store.Dispose();
        return rtrner;
    }

    public static void insert_textVersion (textVersion textVersion) {
        var store = new DataStore ("textVersion.json");

        // Get employee collection
        store.GetCollection<textVersion> ().InsertOneAsync (textVersion);

        store.Dispose();
    }

    public static void update_textVersion (textVersion textVersion) {
        var store = new DataStore ("textVersion.json");

        store.GetCollection<textVersion> ().ReplaceOneAsync (e => e.ID == textVersion.ID, textVersion);
        store.Dispose();
    }

    public static void delete_textVersion (textVersion textVersion) {
        var store = new DataStore ("textVersion.json");

        store.GetCollection<textVersion> ().DeleteOne (e => e.ID == textVersion.ID);
        store.Dispose();
    }
  }
}