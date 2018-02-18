﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using RestSharp;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace MinitoriCore.Modules.DiscordBots
{
    public class DiscordBots : ModuleBase
    {
        // Non-testing 132106771975110656
        // Full mute 132106637614776320
        // Unverified 318748748010487808

        // No emoji 241256979840892939
        // No embed 178823209217556480

        private async Task ChangeRoles(string remainder, ulong role, bool addRole = true)
        {
            var args = remainder.Split(' ').Where(x => x.Length > 0).ToList();
            string reason = "";
            var users = new List<IGuildUser>();

            int i = 0;
            foreach (var s in new List<string>(args))
            {
                var id = s.TrimStart('<').TrimStart('@').TrimStart('!').TrimEnd('>');
                ulong temp;
                if (ulong.TryParse(id, out temp))
                {
                    var user = await Context.Guild.GetUserAsync(temp);

                    if (user != null && user.IsBot)
                        users.Add(user);
                    
                    args.RemoveAt(i);
                    i++;
                }
                else
                    break;
            }

            if (users.Count() == 0)
            {
                await ReplyAsync("You need to mention some bots for this to work!");
                return;
            }

            string action = "";
            
            switch (role)
            {
                case 132106771975110656:
                case 132106637614776320:
                    if (addRole)
                        action = "Mute";
                    else
                        action = "Unmute";
                    break;
                default:
                    action = "Role changed";
                    break;
            }

            reason = $"[ {action} by {Context.User.Username}#{Context.User.Discriminator} ] {string.Join(" ", args)}".Trim();
            
            //int mutedUsers = 0;
            var r = Context.Guild.GetRole(role);
            
            //StringBuilder output = new StringBuilder();
            //output.AppendLine($"Added `{r.Name}` to the following bots:");

            var roledBots = new List<IGuildUser>();
            var unroledBots = new List<IGuildUser>(); // for bots that are unaffected by the requested action

            foreach (var u in users)
            {
                if (addRole)
                {
                    if (!u.RoleIds.Contains(r.Id))
                    {
                        if (r.Id == 132106637614776320) // Full mute
                        {
                            if (u.RoleIds.Contains((ulong)132106771975110656))
                            {
                                await u.RemoveRoleAsync(Context.Guild.GetRole(132106771975110656),
                                    new RequestOptions() { AuditLogReason = $"[ Roleswap by {Context.User.Username}#{Context.User.Discriminator} ] {string.Join(" ", args)}".Trim() });
                            }
                            await u.AddRoleAsync(r, new RequestOptions() { AuditLogReason = reason });
                            roledBots.Add(u);
                        }
                        else if (r.Id == 132106771975110656) // Non-testing mute
                        {
                            if (u.RoleIds.Contains((ulong)132106637614776320))
                                unroledBots.Add(u);
                            else
                            {
                                await u.AddRoleAsync(r, new RequestOptions() { AuditLogReason = reason });
                                roledBots.Add(u);
                            }
                        }
                        else
                        {
                            await u.AddRoleAsync(r, new RequestOptions() { AuditLogReason = reason });
                            roledBots.Add(u);
                        }
                    }
                    else
                        unroledBots.Add(u);
                }
                else
                {
                    if (r.Id == 132106637614776320 ||
                        r.Id == 132106771975110656)
                    {
                        await u.RemoveRolesAsync(new List<IRole>() { Context.Guild.GetRole(132106637614776320), Context.Guild.GetRole(132106771975110656) }, 
                            new RequestOptions() { AuditLogReason = reason });
                        roledBots.Add(u);
                    }
                    else if (u.RoleIds.Contains(r.Id))
                    {
                        await u.RemoveRoleAsync(r, new RequestOptions() { AuditLogReason = reason });
                        roledBots.Add(u);
                    }
                    else
                        unroledBots.Add(u);
                }
            }

            if (roledBots.Count() == 0)
            {
                await ReplyAsync("None of those mentioned were affected.");
                return;
            }

            // I probably shouldn't care this much about formatting
            StringBuilder output = new StringBuilder();

            if (addRole)
                output.Append($"Added `{r.Name}` to the following bot");
            else
                output.Append($"Removed `{r.Name}` from the following bot");
            
            if (roledBots.Count() > 1)
                output.Append("s:\n");
            else
                output.Append(":\n");

            output.AppendLine(string.Join(", ", roledBots.Select(x => $"**{x.Username}#{x.Discriminator}**")));

            if (unroledBots.Count() > 0)
            {
                if (unroledBots.Count() > 1)
                    output.Append("These bots ");
                else
                    output.Append("This bot ");

                if (addRole)
                    output.AppendLine("already had that role:");
                else
                    output.AppendLine("didn't have that role to start:");

                output.AppendLine(string.Join(", ", unroledBots.Select(x => $"**{x.Username}#{x.Discriminator}**")));
            }

            await ReplyAsync(output.ToString().Trim());
        }

        [Command("mute")]
        [Summary("Place a non-testing mute on a bot")]
        public async Task NormalMute([Remainder]string remainder = "")
        {
            if (Context.Guild.Id != 110373943822540800)
                return;

            if (((IGuildUser)Context.User).RoleIds.ToList().Contains(407326634819977217) ||
                ((IGuildUser)Context.User).RoleIds.ToList().Contains(113379036524212224))
            {
                await ChangeRoles(remainder, 132106771975110656);
            }
        }

        [Command("supermute")]
        [Summary("Place a full testing mute on a bot")]
        public async Task FullMute([Remainder]string remainder = "")
        {
            if (Context.Guild.Id != 110373943822540800)
                return;

            if (((IGuildUser)Context.User).RoleIds.ToList().Contains(407326634819977217) ||
                ((IGuildUser)Context.User).RoleIds.ToList().Contains(113379036524212224))
            {
                await ChangeRoles(remainder, 132106637614776320);
            }
        }

        [Command("unmute")]
        [Summary("Remove a mute from a bot")]
        public async Task Unmute([Remainder]string remainder = "")
        {
            if (Context.Guild.Id != 110373943822540800)
                return;

            if (((IGuildUser)Context.User).RoleIds.ToList().Contains(407326634819977217) ||
                ((IGuildUser)Context.User).RoleIds.ToList().Contains(113379036524212224))
            {
                await ChangeRoles(remainder, 132106637614776320, false);
            }
        }

        [Command("sandbox")]
        [Summary("Remove a mute from a bot")]
        public async Task AddUnverified([Remainder]string remainder = "")
        {
            if (Context.Guild.Id != 110373943822540800)
                return;

            if (((IGuildUser)Context.User).RoleIds.ToList().Contains(407326634819977217) ||
                ((IGuildUser)Context.User).RoleIds.ToList().Contains(113379036524212224))
            {
                await ChangeRoles(remainder, 318748748010487808);
            }
        }

        [Command("unsandbox")]
        [Summary("Remove a mute from a bot")]
        public async Task RemoveUnverified([Remainder]string remainder = "")
        {
            if (Context.Guild.Id != 110373943822540800)
                return;

            if (((IGuildUser)Context.User).RoleIds.ToList().Contains(407326634819977217) ||
                ((IGuildUser)Context.User).RoleIds.ToList().Contains(113379036524212224))
            {
                await ChangeRoles(remainder, 318748748010487808, false);
            }
        }

        [Command("noreactions")]
        [Summary("Remove a mute from a bot")]
        public async Task AddNoReaction([Remainder]string remainder = "")
        {
            if (Context.Guild.Id != 110373943822540800)
                return;

            if (((IGuildUser)Context.User).RoleIds.ToList().Contains(407326634819977217) ||
                ((IGuildUser)Context.User).RoleIds.ToList().Contains(113379036524212224))
            {
                await ChangeRoles(remainder, 241256979840892939);
            }
        }

        [Command("yesreactions")]
        [Summary("Remove a mute from a bot")]
        public async Task RemoveNoReaction([Remainder]string remainder = "")
        {
            if (Context.Guild.Id != 110373943822540800)
                return;

            if (((IGuildUser)Context.User).RoleIds.ToList().Contains(407326634819977217) ||
                ((IGuildUser)Context.User).RoleIds.ToList().Contains(113379036524212224))
            {
                await ChangeRoles(remainder, 241256979840892939, false);
            }
        }

        [Command("botneedshelp")]
        [Summary("pollr machine broke")]
        public async Task AddBotRole([Remainder]string remainder = "")
        {
            if (Context.Guild.Id != 110373943822540800)
                return;

            if (((IGuildUser)Context.User).RoleIds.ToList().Contains(407326634819977217) ||
                ((IGuildUser)Context.User).RoleIds.ToList().Contains(113379036524212224))
            {
                await ChangeRoles(remainder, 110374777914417152);
            }
        }
    }
}