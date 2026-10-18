// Kod u skripti NE nosi using direktive. Ovaj fajl ih daje globalno.

// ---------- nedelja 1-3: osnove, kontroleri, validacija ----------
global using System.ComponentModel.DataAnnotations;
global using System.Diagnostics;                    // Stopwatch, Activity
global using System.Linq.Expressions;               // Expression<Func<...>>
global using System.Security.Claims;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Diagnostics;      // IExceptionHandler
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Filters;      // IAsyncActionFilter
global using Microsoft.AspNetCore.Mvc.ModelBinding; // ModelStateDictionary
global using Microsoft.Extensions.Localization;     // IStringLocalizer
global using Microsoft.Extensions.Options;          // IOptions<T>
global using Scalar.AspNetCore;                     // paket Scalar.AspNetCore

// ---------- nedelja 4-6: EF Core (tranzitivno iz MiniShop.Data), slojevi ----------
global using Microsoft.EntityFrameworkCore;

// ---------- slojevi (nedelja 6) ----------
global using MiniShop.Domain.Entities;
global using MiniShop.Application;
global using MiniShop.Application.Abstractions;
global using MiniShop.Application.Common;
global using MiniShop.Application.Dtos;
global using MiniShop.Application.Mapping;
global using MiniShop.Application.Services;
global using MiniShop.Data;

// ---------- namespace-ovi projekta MiniShop.Api ----------
global using MiniShop.Api.Errors;
global using MiniShop.Api.Extensions;
global using MiniShop.Api.Filters;
global using MiniShop.Api.Middleware;
global using MiniShop.Api.Options;

global using Asp.Versioning;
global using Microsoft.OpenApi;
global using Microsoft.AspNetCore.Localization;

global using System.Net.Http.Json;                     // ReadFromJsonAsync
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.Extensions.Caching.Memory;      // IMemoryCache
global using Microsoft.Extensions.Http.Resilience;     // paket Microsoft.Extensions.Http.Resilience
global using Polly;                                    // DelayBackoffType, dolazi uz Resilience
global using MiniShop.Api.Clients;
global using MiniShop.Api.Http;
global using MiniShop.Api.Hubs;
global using MiniShop.Api.Storage;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Authentication.JwtBearer;

global using System.IdentityModel.Tokens.Jwt;             // JwtSecurityToken, claim imena
global using System.Security.Cryptography;                // SHA256, RandomNumberGenerator
global using Microsoft.IdentityModel.Tokens;              // SymmetricSecurityKey, Base64UrlEncoder
global using MiniShop.Api.Auth;

global using Microsoft.AspNetCore.OutputCaching;       // AddOutputCache, [OutputCache], IOutputCacheStore
global using Microsoft.Extensions.Caching.Hybrid;      // HybridCacheEntryOptions u Program.cs
global using MiniShop.Application.Caching;
global using MiniShop.Api.Caching;
global using System.Threading.Channels;                    // Channel<OrderJob>
global using System.Threading.RateLimiting;                // RateLimitPartition, MetadataName
global using Microsoft.AspNetCore.RateLimiting;            // [EnableRateLimiting]
global using Microsoft.Extensions.Diagnostics.HealthChecks;   // HealthCheckResult
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;   // HealthCheckOptions
global using MiniShop.Application.Orders;
global using MiniShop.Api.Workers;

global using Serilog;                               // paket Serilog.AspNetCore
global using Serilog.Formatting.Compact;            // paket Serilog.Formatting.Compact
global using OpenTelemetry.Resources;               // paket OpenTelemetry.Extensions.Hosting
global using OpenTelemetry.Trace;
global using OpenTelemetry.Metrics;
