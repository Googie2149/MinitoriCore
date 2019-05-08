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
using System.Drawing;
using MinitoriCore.Preconditions;
using System.Net;

namespace MinitoriCore.Modules.Splatoon
{
    // [RequireGuild(568302640371335168)]
    public class MapRotation : MinitoriModule
    {
        private RankedService rankedService;
        private Config config;
        private CommandService commands;
        private IServiceProvider services;

        public MapRotation(RankedService _rankedService, CommandService _commands, IServiceProvider _services, Config _config)
        {
            rankedService = _rankedService;
            commands = _commands;
            services = _services;
            config = _config;
        }

        [Hide]
        [Command("map", RunMode = RunMode.Async)]
        public async Task SelectMap()
        {
            string stage = "";

            if (!Directory.Exists("./Images/Splatoon/"))
            {
                Directory.CreateDirectory("./Images/Splatoon/");
            }

            Random asdf = new Random(); // Todo: Implement better rng
            int fileCount = Directory.GetFiles("./Images/Splatoon/", "*.png").Count();

            if (fileCount == 0)
            {
                await RespondAsync("Something went wrong and I have no maps in my list!");
                return;
            }
            else if (fileCount == 1)
                stage = Directory.GetFiles("./Images/Splatoon/", "*.png").FirstOrDefault();
            else if (fileCount > 1)
                stage = Directory.GetFiles("./Images/Splatoon/", "*.png").ToList().OrderBy(x => asdf.Next()).FirstOrDefault();

            EmbedBuilder builder = new EmbedBuilder();

            builder.ImageUrl = $"attachment://{stage}";
            builder.Title = "ImageUrl Option Formatting Test";

            await Context.Channel.SendFileAsync($"./Images/Splatoon/{stage}", embed: builder.Build());

            builder = new EmbedBuilder();

            builder.ThumbnailUrl = $"attachment://{stage}";
            builder.Title = "ThumbnailUrl Option Formatting Test";

            await Context.Channel.SendFileAsync($"./Images/Splatoon/{stage}", embed: builder.Build());
        }
    }
}