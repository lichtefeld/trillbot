/*SocketGuildUser usr = Context.Guild.GetUser(ID);
 */

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
    public class RacerCreation : ModuleBase<SocketCommandContext>
    {

        public static List<racer> allRacers = new List<racer>();
        private static Dictionary<int, string> racer_to_Image = new Dictionary<int, string>() {
            {1, "https://vignette.wikia.nocookie.net/far-verona/images/d/d1/VladHumble.png/revision/latest?cb=20181029143832"},
            {2, "https://vignette.wikia.nocookie.net/far-verona/images/1/11/Franciszek.png/revision/latest?cb=20181025131015"},
            {3, "https://vignette.wikia.nocookie.net/far-verona/images/1/12/StrixVulpine.png/revision/latest?cb=20181025131654"},
            {4, "https://vignette.wikia.nocookie.net/far-verona/images/9/90/PhasTee.png/revision/latest?cb=20181027151400"},
            {5, "https://vignette.wikia.nocookie.net/far-verona/images/f/f7/AnthonyGonzales.png/revision/latest?cb=20181030132033"}, //Anthony
            {6, "https://vignette.wikia.nocookie.net/far-verona/images/2/26/DulaImay.png/revision/latest?cb=20181025125348"},
            {7, "https://vignette.wikia.nocookie.net/far-verona/images/7/73/CocoCobra.png/revision/latest?cb=20181025133404"},
            {8, "https://vignette.wikia.nocookie.net/far-verona/images/8/86/KeslerTiyamike.png/revision/latest?cb=20181101185940"}, //Crux Kresler
            {9, "https://vignette.wikia.nocookie.net/far-verona/images/8/81/RutileVenus.png/revision/latest?cb=20181025132952"},
            {10, "https://vignette.wikia.nocookie.net/far-verona/images/7/7c/SJacobson.png/revision/latest?cb=20181025125606"},
            {11, "https://vignette.wikia.nocookie.net/far-verona/images/6/61/DeciusTulliusCrispus.png/revision/latest?cb=20181025131248"},
            {12, "https://vignette.wikia.nocookie.net/far-verona/images/0/03/RacerIX.png/revision/latest?cb=20181025133954"},
            {13, "https://vignette.wikia.nocookie.net/far-verona/images/d/d5/DeciusCato.png/revision/latest?cb=20181025131900"},
            {14, "https://vignette.wikia.nocookie.net/far-verona/images/c/c5/LionoPanthra.png/revision/latest?cb=20181025132105"},
            {15, "https://vignette.wikia.nocookie.net/far-verona/images/1/17/JaxtonBenson.png/revision/latest?cb=20181029144044"},
            {16, "https://vignette.wikia.nocookie.net/far-verona/images/0/01/Amber.png/revision/latest?cb=20181025134236"},
            {17, "https://vignette.wikia.nocookie.net/far-verona/images/3/38/TheMoose.png/revision/latest?cb=20181025133616"},
            {18, "https://vignette.wikia.nocookie.net/far-verona/images/4/43/CruxPanda.png/revision/latest?cb=20181025131452"},
            {19, "https://vignette.wikia.nocookie.net/far-verona/images/6/6e/TyrenePrayla.png/revision/latest?cb=20181025132657"},
            {20, "https://vignette.wikia.nocookie.net/far-verona/images/a/a4/RhodesBiggles.png/revision/latest?cb=20181025132247"},
            {21, "https://vignette.wikia.nocookie.net/far-verona/images/6/63/MongrelJimTimo.png/revision/latest?cb=20181025195613"},
            {22, "https://vignette.wikia.nocookie.net/far-verona/images/a/ac/RichieSteel.png/revision/latest?cb=20181025132505"},
            {23, "https://vignette.wikia.nocookie.net/far-verona/images/0/0e/Fuschia.png/revision/latest?cb=20181030131822"}, //Vela
            {24, "https://vignette.wikia.nocookie.net/far-verona/images/6/63/GuillaumeValls.png/revision/latest?cb=20181025130707"},
            {25, "https://vignette.wikia.nocookie.net/far-verona/images/5/54/Nikita.png/revision/latest?cb=20181025130043"}
        };

        private static Dictionary<int, string> racer_to_Description = new Dictionary<int, string>() {
            {1, "The ninth child of the infamous and un-killable mercenary Dimi of the Deathless, Vlad is pumped full of gene mods and ego. He has an unhealthy confidence in his virility and physique and is not ashamed to let everyone know it; \"humble\" he certainly is not. We all know that muscles, boasts and gene mods don't win races but as a racer in this year Prix, Dimi's son does have one thing to brag about: he is an extraordinary precog trained on Hroa. Most years the Prix sees large numbers of precogs enter the lists as their reaction times and predictive driving techniques prove incredibly advantageous when traveling in the shrapnel filled Lightway at ludicrous speeds. This year however, the only precog openly advertising themselves is Vlad Humble himself, it may be that the race this year is stacked with precogs too clever to out themselves publicly, but who can say. Vlad claims to have been told his whole life that he is absolutely perfect in every way: physically, mentally and emotionally; perhaps he has been treated with favoritism due to his father's deadly fame but maybe Vlad truly has nothing to be humble about. Is Vlad practically perfect in every way? Will Dimi find you and kill you if you say otherwise? Get your betting money ready and find out!"},
            {2,"Franciszek has been racing the Lightway in the Grand Prix for most of his adult life, and though he has never won, he has survived more races than any other racer in history. He races to win fame and glory to the Messiah-Emperox; to whom he dedicates all his triumphs. He has not raced since the assassination of Cygnus and claims to have been recognizing a self-imposed period of mourning, though others say he’s been too afraid to touch a light runner without the Emperox’s protection. Will the Emporox protect him this year or will we finally watch the un-killable Knight-Errant crash and burn Find out at this year's Grand Prix!!!"},
            {3, "Coming in at the bottom of the charts as the man voted most likely to die on the finish line (yes even below the panda), Strix Vulpine has no race experience whatsoever and only first touched a race ship a few months ago! Many believe that Vulpine’s surprise entrance into the Grand Prix is a ploy by House Eridanus to play the bookies and undermine the betting market, but what are the odds of that being true? PLACE YOUR BETS!"},
            {4, "Mister Phas'Tee comes from a family of Grand Prix racers that goes back six generations. He personally runs the Trilliant Grand Prix Training Program and has been involved with all Lightrunner testing and development for this year's race. He has aced all the simulations. He is a home-grown Trillian in every sense of the word. No one knows how cybered he is beneath that flight suit but everyone knows how psyched we are to have him compete. Open your wallets to Mr. Phas'Tee! Its his year to win folks, you heard it here first!"},
            {5, "f you have watched a film on any PRISM channel in the last quarter century then you have seen Anthony Gonzales THE stunt man to the stars. He is perhaps most famous for his work on the hit film of 3199 'Synthspiracy'. He has been burnt and dozens of his bones have broken, he has been kicked from airplaines, ejected into space without a suit, lit on fire and endured a live streamed vivisection. No matter what stunt  he pulls off, he always comes out looking better than before.  When asked about his motivation to continue working he said: \"An adrenaline junky? Hell no I'm no loco junky. I do this because its fun and because I'm the only one who hasn't died doing the impossible. It is all about what the director wants.\"  Will this years Grand Prix be his last and greatist stunt? Or will we get to see him return to the Prismacolor screen in next years 'Synthspiracy 2: the swans of vengence'."}, //PRISM
            {6, "The racer with the most storied past in this year's race, Dula is a Hong Lu orphan. She showed a gift for racing grav bikes in the early days following the revolt and made a name for herself in the violent street races of Hong Lu at the early age of 13. When a Red Dogs charity spokesperson offered her the chance to train to race on Trillia for the Grand Prix several years ago, she jumped at the opportunity. 10% of all bets on Dula will go to The Red Dogs Charity, and she herself plans to donate all of her winnings should she win."},
            {7,"Coco Cobra is the Chief Virtualizer of the Trilliant Ring and an exceptional engineer. She has turned her skills toward the Prix and has run simulations and virtual reconstructions of all previous Grand Prix races. Having watched the - according to her - embarrassingly awful displays of past racers, Coco signed up for this year's Grand Prix. Her friends and family are worried for her safety but pleased that she finally found something that motivates her. Her mother stated that \"Coco is very excited to try something new,\" or in Coco's own words: \"I can't flarking wait to show these flarking dub scrubs how to flarking wrangle a Lightway.\""},
            {8, "Tiyamike Kesler joined the House Crux Judicial Enforcement Services to help keep his family and Hiera safe from the synthetic menace during the War Against the Artificials. From a young age, Tiyamike loved to drive fast. After the war, his talents for high-speed chases behind the wheel of gravcar or the stick of a gravflyer were both a hindrance and a benefit to his career with JES, with his superiors having to balance his many arrests with the fact that many criminals trying to escape his pursuits ended up flying to their deaths. That being said, when criminals make a run for it, it’s usually the fast and furious driving of Tiyamike that bring them back to justice. With his career not showing much prospect for advancement, Tiyamike declares his intention to join the Grand Prix. With the added fame and fortune from the win, he plans to try and convince his superiors to build a division of JES that focuses on hot pursuits. Justice is not always fast; but when it is, it comes on the heels of Tiyamike."}, //Crux Kresler
            {9, "An independent racer, Rutile was originally from House Serpens but moved to Trillia IX in order to get pampered the right way! As a throne-fearing member of the nobility, he hates synths, despises serfs, and spits on the acronym \"STO.\" Rutile wants you all to know that he is entirely cybernetic from the waist down and that when he wins the Grand Prix, he will be staying in the Emperox Suite of the Paradiso Facet on Trillia XI. Is Rutile full of himself or simply aware of his abilities?"},
            {10, "S. Jacobson was once a humble A.C.R.E. Junior Vat Pulp Strategy Associate. But after scoring well in the A.C.R.E. Light Race Simulator in 3196 he was selected for A.C.R.E.'s five year elite Grand Prix training program. One of ten thousand candidates for the 3200 racing season, the candidates were whittled down until only one candidate remained. Now S. Jacobson has the honor of representing the A.C.R.E. family for all the sector to see. When asked if he feared death, Jacobson replied \"Try A.C.R.E. F.O.O.D: It's F.O.O.D!\" In preparation for the Grand Prix, he has been cybernetically grafted to his chair, which will be installed in his vehicle before the race, and are on a strict IV-administered nutrition and drug regime. Nonetheless, Jacobson is known to be a prolific drinker of Fizzy Cheese."},
            {11, "Freeman Decius Tullius Crispus, sponsored by Aquilla, is an experienced pilot in his late 40’s who was upgraded from Serfdom for his years of valiant service in the Synthetic War."},
            {12, "They have no name, they need no identity, they are Racer IX. We all watched in horror as their light speeder tore itself to pieces in the firestorm of dying racers and broken dreams that was the 83rd Grand Prix. Our eyes grew wide in shock when we realized it was their own brother who rammed them into the fatal pileup. We learned the thrill of vengeance when they slammed the disintegrating conflagration of their own ship into their bother's path and crushed him against the light way. They are the phantom, craving blood and speed and death, a surprise entry into races across the sector. They're 80% cybernetic and 20% burned flesh and 100% speed."},
            {13, "The second-born son of the Decius family, this man of iron and blood aims for victory at any cost. He may not be the most experienced or accomplished racer but in technical racing knowledge he is in a class alone. Can the other racers compete with his hard edged determination, mechanical proficiency, and single minded pursuit of victory?"},
            {14, "One thing was certain about Panthra, the young and upcoming noble always wanted to race. Whether to win in the most stylish and over the top fashion or to finally meet the After’s grace, no one was quite sure. The simple fact that he lived through so many races was a shock even to the racer himself. At the start of each race he repeats his catchphrase \"I’ll keep the After warm for you,\" usually said to his biggest rival of the event. No one can really tell how serious he is about that statement, but if you have seen him in enough races, you would very well know he means it."},
            {15, "Jaxton was always a prankster and a show man. Rarely sitting still long enough to focus on his studies, he was always thinking up his next absurd stunt to impress anyone who would watch him. After an unfortunate incident involving a grav car and the High Energy Physics building of the Delta Institute, Jaxton was marked as “Unsuitable for Research” by the Institute leadership. The noble Ramella family caught word of this, and decided that Jaxton had the perfect kind of personality for the upcoming Grand Prix. Now, after a rigorous cyber-enhancement regime, the Ramellas just need to get Jaxton to stop flirting with everyone around him long enough to stuff him into the lightrunner."},
            {16, "Amber has lived her life on the run. She grew up on Yakiyah and worked hard to become a pilot until getting kidnapped by pirates and beginning a lifetime of close calls and near escapes across the sector. House Crux officials have notified the Trilliant Prix Registration office that she is suspected of a number of crimes and may have some connection with a recent violent incident upon an Aquillian warship. Trilliant's advice to Amber? Speed solves all problems in life, never stop racing!"},
            {17, "The Moose is the culmination of the Trilliant Ring's \"Animalistic Fantasy\"-body alteration line. She is also the unofficial mascot of the Trilliant Q&A department. A member of Trilliant Wallflower security delivered a security video record of her registration to the Grand Prix Hype department, perhaps hoping to convince us to remove her from this year's Prix roster; however, we feel this transcript shows exactly why the Grand Prix needs a racer like her this year" + System.Environment.NewLine + "Ranking Q&A manager: \"Hey Moose! Do you like speed?\"." + System.Environment.NewLine + "The Moose:\".. s-sure, the drug messes up my wings and my horns get all droopy, but I like it..\" "  + System.Environment.NewLine + "Ranking Q&A manager: \"Great! Sign here for the Race of Speed.\"" + System.Environment.NewLine + "The Moose:\"uhh.. What race are you..\"" + System.Environment.NewLine + "Ranking Q&A manager: \"No questions. Just sign the papers please, we need you.\"" + System.Environment.NewLine + "The Moose:\"I guess this is ok.\""},
            {18, "House Crux is openly taunting the Grand Prix this year by sponsoring a common panda. We have no idea if this furry fellow can win the race, but it has already conquered our hearts. Who knows folks! If every other racer dies, the panda may coast across the finish line. Place your bets!"},
            {19, "Seer Serpens Tyrene Prayla, Medical Officer, Cybernetics Division. Her reflexes, enhanced reaction speed and cybernetic implants make her an easy favorite this year!"},
            {20, "Biggles is a known daredevil, having flown some of the most dangerous communication missions recorded in the war. A committed adherent to the High Church and traditional imperial values, he saw it as his duty to serve the Empire with full force during the War Against the Artificials. He is very much an outsider in House Pyxis and is not much loved; however, he is one of the best pilots of his generation, both in and out of spike space. Has his House sponsored him because they believe he can claim victory, or do they hope that this staunch conservative never returns home? Who knows?!"},
            {21, "Long a test pilot for the Shan Institute for Singularity Research, Jim is out to make his fortune in the Grand Prix after a lifetime of hard luck. A Scoundrel with a literal heart of carbon and a liver battle-scarred by a war with alcohol. No overt Vagrant ties can be proved, but he definitely represents them well with single minded determination for money, and his dogged commitment to doing whatever it takes to get that next Score. Usually a racer in the Grand Prix with a hook for a hand would be considered that year's 'heel' but in this year's Prix, with its prevalence of claw hands, bloodstained amour and dog scarves, ”Mongrel” Jim Timo genuinely appears to be a decent fellow."},
            {22, "Once the fastest delivery driver on Trillia, but after years proving himself on light cycles he has finally made it big. Recruited, trained and outfitted by House Reticulum, Richie is sure to take sector by storm!"},
            {23, "This Velen is notoriously mute, he rarely speaks; preferring to communicate with gestures and noncommittal grunts. When he has been heard talking it has always been in blunt, clipped language. While he is no great speaker, his actions on the Lightway shout with an eloquence that would strike the most practiced Lyran conversationalist dumb and leave the most stonefaced Crux judge slack jawed in awe. Fuschia's command of the race track is calm, deliberate and sweepingly decisive. Watch this one race fans, the Grand Prix has not seen a racer of his caliber in over a century."}, //Vela
            {24, "The Deathless do not die. He is payed to race and so he shall. A Soldier is a Soldier. There is nothing more to say."},
            {25, "The shy Xenoengineer nicknamed \"Doc\" racing under a the CHR banner this year loves aliens and warm socks and doesn't care much for people. However there is one person that Nikita simply cannot stand, a person with whom she has shared a long passive aggressive rivalry. When Coco Cobra announced herself as one of Trilliant's racers, Nikita couldn't resist the chance to compete with her nemesis. Neither Coco nor Nikita have historically talked much smack, but a joint press conference has been scheduled before the race so that the two hot blooded rivals may stare at one another with ever increasing amounts of disinterest."}
        };

        [Command("createracer")]
        public async Task NewracerAsync(string name, string faction, int ID = -1)
        {
            var r = allRacers.FirstOrDefault(e=> e.player_discord_id == Context.Message.Author.Id);//racer.get_racer(Context.Message.Author.Id);
            if(r != null) {
                await ReplyAsync("You already have a racer. Please use `ta!deleteracer` to remove your old one first");
                return;
            }
            var usr = Context.Guild.GetUser(Context.Message.Author.Id);
            if (name == null) {
                name = usr.Nickname != null ? usr.Nickname : usr.Username;
            }
            if(usr.Roles.FirstOrDefault(e=>e.Name == "Racer") == null) 
            { 
                await ReplyAsync(name + ", Please contact a moderator if you should be a racer");
            } 
            else 
            {
                if(ID == -1) {
                    r = new racer
                    {
                        name = name,
                        faction = faction
                    };
                } else {
                    var a = Ability.get_ability(--ID);
                    r = new racer
                    {
                        name = name,
                        faction = faction,
                        ability = a
                    };
                }

                r.player_discord_id = Context.Message.Author.Id;
                allRacers.Add(r);
                racer.insert_racer(r);

                await ReplyAsync(name + ", You've got a racer!");
            }

        }

        [Command("showracer")]
        public async Task showRacerAsync(int i = -1) {
            racer r = new racer();
            if (i < 0) r = allRacers.FirstOrDefault(e=> e.player_discord_id == Context.Message.Author.Id);//racer.get_racer(Context.Message.Author.Id);
            else r = allRacers[i];

            if ( r == null ) {
                await ReplyAsync(Context.User.Mention + ", you don't have a current racer");
                return;
            }

            var embed = new EmbedBuilder();

            embed.Title = "Grand Prix Racer: " + r.name;
            if (r.ID <= 25 && r.ID != 0) {
                embed.WithThumbnailUrl(racer_to_Image[r.ID]);
                embed.WithDescription(racer_to_Description[r.ID]);
            }
            embed.AddField("Sponsor",r.faction, true);
            embed.AddField("Ability: " + r.ability.Title, r.ability.Description, true);
            embed.AddField("ID",r.ID.ToString(),true);
            if( i < 0 ) {
                embed.AddField("Player",Context.User.Mention,true);
            } else {
                var usr = Context.Guild.GetUser(r.player_discord_id);
                embed.AddField("Player",usr.Mention,true);
            }
            embed.Build();
            await Context.Channel.SendMessageAsync("", false, embed, null);
        }

        [Command("updateability")]
        public async Task UpdateAbilityAsync(int ID) {
            var r = allRacers.FirstOrDefault(e=> e.player_discord_id == Context.Message.Author.Id);//racer.get_racer(Context.Message.Author.Id);

            if(r == null) {
                await ReplyAsync("No racer found for you");
            } else {
                if (r.inGame) {
                    await ReplyAsync("You can't modify your racer while racing!");
                    return;
                }
                var a = Ability.get_ability(--ID);
                r.ability = a;
                racer.replace_racer(r);
                await ReplyAsync(Context.User.Mention + ", Ability changed to " + a.Title);
            }
            
        }

        [Command("showabilities")]
        public async Task DisplayAbilitiesAsync() {
            var abilities = Ability.get_ability();
            var str = new List<string>();
            var count = 21;
            str.Add("**Special Abilities**");
            for(int i = 0; i < abilities.Count; i++) {
                var active = "Passive";
                if (abilities[i].Active){
                    active = "Active";
                }
                var s = "**#" + (i+1) + ":** " + abilities[i].Title + " (" + active + ") - *" +abilities[i].Description + "*";
                count += s.Length;
                if (count > 1950) {
                    var temp_output_string = String.Join(System.Environment.NewLine,str);
                    await Context.User.SendMessageAsync(temp_output_string);
                    count = s.Length;
                    str = new List<string>();
                }
                str.Add(s);
            }
            var output_string = String.Join(System.Environment.NewLine,str);
            await Context.User.SendMessageAsync(output_string);
        }

        [Command("showcards")]
        public async Task showCardsAsync() {
            var cards = Card.get_card();
            var str = new List<string>();
            var count = 13;
            str.Add("**Card List**");
            for(int i = 0; i < cards.Count; i++) {
                var s = "**" + cards[i].title + "** - " + cards[i].description;
                count += s.Length;
                if (count > 2000) {
                    var temp_output_string = String.Join(System.Environment.NewLine,str);
                    await Context.User.SendMessageAsync(temp_output_string);
                    count = s.Length;
                    str = new List<string>();
                }
                str.Add(s);
            }
            var output_string = String.Join(System.Environment.NewLine,str);
            await Context.User.SendMessageAsync(output_string);
        }

        [Command("deleteracer")]
        public async Task DeleteRacerAsync()
        {
            var r = allRacers.FirstOrDefault(e=> e.player_discord_id == Context.Message.Author.Id);//racer.get_racer(Context.Message.Author.Id);

            if(r == null) {
                await ReplyAsync("No racer found for you");
            } else {

                Classes.racer.delete_racer(r);
                allRacers.Remove(r);
                await ReplyAsync("Racer Deleted.");
            }
        }

        [Command("resetracer")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task resetOneRacer(int i) {
            var r = allRacers.FirstOrDefault(e=> e.ID == i);
            if (r == null) {
                await ReplyAsync("No racer with that ID");
                return;
            }
            r.reset();
            racer.replace_racer(r);
            await ReplyAsync(r.nameID() + " has been reset");
        }

        [Command("resetracers")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task resetAllRacers() {
            allRacers.ForEach(e=> {
                e.reset();
            });
            helpers.UpdateRacersDatabase();            
            await ReplyAsync("All Racers Reset");
        }

        [Command("grandprix")]
        public async Task ListGrandPrixRacersAsync() {
            var s = new List<string>();
            s.Add("Racers for the Grand Prix!");
            s.Add("```Name (ID) | Ability" );
            for(int i = 1; i <= 25; i++) {
                s.Add(allRacers[i].nameID() + " | " + allRacers[i].ability.Title);
            }
            s.Add("```");
            await ReplyAsync(String.Join(System.Environment.NewLine,s));
            return;
        }

        [Command("listracers")]
        public async Task ListRacersAsync() //Need to make this DM & account for more than 2k characters. Using a list to build output strings.
        {
            var s = new List<string>();
            s.Add("Racers for the Grand Prix!");
            s.Add("```" );
            foreach(Classes.racer r in allRacers) {
                s.Add("ID: #" + r.ID + " | " + r.name);
            }
            s.Add("```");
            await ReplyAsync(String.Join(System.Environment.NewLine,s));
            return;
        }

        [Command("updatelist")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task updateListAsync() {
            allRacers = racer.get_racer().OrderBy(e=>e.ID).ToList();
            await ReplyAsync("List Updated)");
        }
    }
}