﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ducker.Commands.Attributes;
using ducker.Database;
using ducker.DiscordData;
using ducker.Logs;

namespace ducker.Commands.AdministrationModule;

public partial class AdministrationCommands
{
    [Command("mute")]
    [Description("Mute mentioned member")]
    [RequireAdmin]
    public async Task MuteCommand(CommandContext msg, DiscordMember member,
        [RemainingText] string reason = "noneReason")
    {
        var muteRoleId = DB.GetId(msg.Guild.Id, "muteRoleId");
        if (muteRoleId == 0)
        {
            var muteRole =
                await msg.Guild.CreateRoleAsync("Muted", Permissions.None, DiscordColor.DarkGray, false, false);

            var channels = await msg.Guild.GetChannelsAsync();

            foreach (var channel in channels)
                await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages);

            DB.Update(msg.Guild.Id, "muteRoleId", muteRole.Id.ToString());

            await member.GrantRoleAsync(msg.Guild.GetRole(DB.GetId(msg.Guild.Id, "muteRoleId")));
            await msg.Message.CreateReactionAsync(DiscordEmoji.FromName(msg.Client, Bot.RespondEmojiName));
            await Log.Audit(msg.Guild, $"{msg.Member.Mention} muted {member.Mention}.", reason);
        }
        else
        {
            try
            {
                await member.GrantRoleAsync(msg.Guild.GetRole(DB.GetId(msg.Guild.Id, "muteRoleId")));
            }
            catch
            {
                var muteRole =
                    await msg.Guild.CreateRoleAsync("Muted", Permissions.None, DiscordColor.DarkGray, false, false);

                var channels = await msg.Guild.GetChannelsAsync();

                foreach (var channel in channels)
                    await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages);

                DB.Update(msg.Guild.Id, "muteRoleId", muteRole.Id.ToString());

                await member.GrantRoleAsync(msg.Guild.GetRole(DB.GetId(msg.Guild.Id, "muteRoleId")));
            }

            await msg.Message.CreateReactionAsync(DiscordEmoji.FromName(msg.Client, Bot.RespondEmojiName));
            await Log.Audit(msg.Guild, $"{msg.Member.Mention} muted {member.Mention}.", reason);
        }
    }

    [Command("mute")]
    public async Task MuteCommand(CommandContext msg, [RemainingText] string text)
    {
        await msg.Channel.SendMessageAsync(Embed.IncorrectCommand(msg, "mute <member> <reason>"));
    }
}