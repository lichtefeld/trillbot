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

namespace trillbot.Classes
{
    public enum Discipline
    {
        Biopsionics,
        Metapsionics,
        Precognition,
        Telekinesis,
        Telepathy,
        Teleportation,
        None
    }
    public static class DisciplineExtensions
    {
        public static string ToFriendlyString(this Discipline me)
        {
            switch (me)
            {
                case Discipline.Biopsionics:
                    return "Biopsionics";
                case Discipline.Metapsionics:
                    return "Metapsionics";
                case Discipline.Precognition:
                    return "Precognition";
                case Discipline.Telekinesis:
                    return "Telekinesis";
                case Discipline.Telepathy:
                    return "Telepathy";
                case Discipline.Teleportation:
                    return "Teleportation";
                default:
                    return "None";
            }
        }

        public static Discipline StringToDiscipline(string input)
        {
            switch (input)
            {
                case "Biopsionics":
                    return Discipline.Biopsionics;
                case "Metapsionics":
                    return Discipline.Metapsionics;
                case "Precognition":
                    return Discipline.Precognition;
                case "Telekinesis":
                    return Discipline.Telekinesis;
                case "Telepathy":
                    return Discipline.Telepathy;
                case "Teleportation":
                    return Discipline.Teleportation;
                default:
                    return Discipline.None;
            }
        }
    }

    public class psiball_player
    {
        public int ID { get; set; }
        public string name { get; set; }
        public Discipline discipline { get; set; }
        public int effort_pool { get; set; }
        public bool active { get; set; }
        public bool reserve { get; set; }
        public psiball_player(string name, string Discipline)
        {
            this.name = name;
            this.discipline = DisciplineExtensions.StringToDiscipline(Discipline);
            this.active = false;
            this.reserve = false;
        }
    }

    public class psiball_team
    {
        public int ID { get; set; }
        public List<psiball_player> players { get; set; }
        public int points{get;set;}

        public psiball_team()
        {
            this.players = new List<psiball_player>();
        }
    }

    public class psiball_game
    {
        public psiball_team team1 { get; set; }
        public psiball_team team2 { get; set; }
        public int round_number { get; set; }
    }



}