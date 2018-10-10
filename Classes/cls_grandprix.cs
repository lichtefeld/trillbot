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

    public class GrandPrix {
        private List<racer> racers = new List<racer>();
        private Stack<Card> cards = new Stack<Card>();
        private bool runningGame = false;
        private int position = 0;
        private int round = 1;
        public string channelName { get; set; } = "";
        private Dictionary<int, Tuple<int,int>> remedy_to_hazards = new Dictionary<int, Tuple<int,int>> {
            {0,new Tuple<int, int>(5,6)},
            {1,new Tuple<int,int>(8,9)},
            {2,new Tuple<int,int>(10,11)}
        };
        private Dictionary<int, string> target_hazard_output = new Dictionary<int, string> {
            {0, ". They are unable to move until they remove this Hazard."},
            {1, ". They better not have any other Hazards applied!"},
            {2,". They have 3 turns to remove this Hazard."},
            {3, ". They are unable to move more than two spaces!"}
        };


    }

}