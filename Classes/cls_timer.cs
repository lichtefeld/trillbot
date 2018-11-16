using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace trillbot.Classes {
    public class timer {
        public IGuildUser user { get; set; }

        public void StartTimer (int dueTime) {
            Timer t = new Timer (new TimerCallback (TimerProc));
            t.Change (dueTime, System.Threading.Timeout.Infinite);
        }

        private void TimerProc (object state) {
            // The state object is the Timer object.
            Timer t = (Timer) state;
            t.Dispose ();
            send_complete_message ();
        }

        public void send_complete_message ()

        {
            this.user.SendMessageAsync ("You are out of time.");
        }
    }

    public class rouletteTimer {
        public ISocketMessageChannel channel { get; set; }
        public Classes.roulette roulette { get; set;}

        public rouletteTimer(ISocketMessageChannel c, Classes.roulette r) {
            channel = c;
            roulette = r;
        }

        public void startTimer() {
            Timer t1 = new Timer (new TimerCallback (oneMinute));
            t1.Change(60 * 1000, System.Threading.Timeout.Infinite);
            Timer t30 = new Timer (new TimerCallback (thirtySeconds));
            t30.Change(90 * 1000, System.Threading.Timeout.Infinite);
            Timer t10 = new Timer (new TimerCallback (tenSeconds));
            t10.Change(110 * 1000, System.Threading.Timeout.Infinite);
            Timer t = new Timer (new TimerCallback (numberCallback));
            t.Change(120 * 1000, System.Threading.Timeout.Infinite);
            channel.SendMessageAsync(roulette.dealerName + " reminds everyone you have **TWO MINUTES** left to place your bets!");
        }
        
        private void oneMinute(object state) {
            Timer t = (Timer) state;
            t.Dispose();
            channel.SendMessageAsync(roulette.dealerName + " reminds everyone you have **ONE MINUTE** left to place your bets!");
        }

        private void thirtySeconds(object state) {
            Timer t = (Timer) state;
            t.Dispose();
            channel.SendMessageAsync(roulette.dealerName + " reminds everyone you have **30 SECONDS** left to place your bets!");
        }

        private void tenSeconds(object state) {
            Timer t = (Timer) state;
            t.Dispose();
            channel.SendMessageAsync(roulette.dealerName + " reminds everyone you have **10 SECONDS** left to place your bets!");
        }

        private void numberCallback(object state) {
            Timer t = (Timer) state;
            t.Dispose();
            roulette.payouts(Program.rand.Next(38));
        }
    }
}