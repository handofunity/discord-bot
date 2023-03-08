// Global using directives

global using System;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Threading;
global using Hangfire;
global using Hangfire.PostgreSql;
global using HoU.GuildBot.BLL;
global using HoU.GuildBot.Core;
global using HoU.GuildBot.DAL;
global using HoU.GuildBot.DAL.Database;
global using HoU.GuildBot.DAL.Discord;
global using HoU.GuildBot.DAL.UNITS;
global using HoU.GuildBot.Keycloak;
global using HoU.GuildBot.Shared.BLL;
global using HoU.GuildBot.Shared.DAL;
global using HoU.GuildBot.Shared.Objects;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Serilog;
global using Serilog.Exceptions;
global using Serilog.Exceptions.Core;
global using Serilog.Exceptions.Destructurers;
global using Serilog.Exceptions.EntityFrameworkCore.Destructurers;