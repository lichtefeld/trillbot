using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using trillbot.Classes;

namespace trillbot.Commands
{
    public class Slots : ModuleBase<SocketCommandContext>
    {
        [Command("enableslots")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task enableSlotsAsync(int i) {
            var slot = Program.slots.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (slot.Value != null) {
                await Context.Channel.SendMessageAsync("Woah there, a slot machine is already initialized in this channel.");
                return;
            }
            var game = slotMachine.get_slotMachine(i);
            if (game == null) {
                await ReplyAsync(Context.User.Mention + ", you didn't select a valid slot machine ID");
            }
            var smr = new slotMachineRunner(Context.Guild.GetTextChannel(Context.Channel.Id),game);
            Program.slots.Add(Context.Channel.Id, smr);
            await Context.Channel.SendMessageAsync("Slot machine added. Use `ta!payouts` to determin the payouts of this machine.");
        }

        [Command("removeslots")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task removeSlotsAsync() {
            var slot = Program.slots.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (slot.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't slot machine in this channel.");
                return;
            }
            Program.slots.Remove(Context.Channel.Id);
            await Context.Channel.SendMessageAsync("Slot machine removed.");
        }

    }
}