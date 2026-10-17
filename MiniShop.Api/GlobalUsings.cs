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