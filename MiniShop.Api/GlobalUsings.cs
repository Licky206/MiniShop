// Kod u skripti NE nosi using direktive. Ovaj fajl ih daje globalno,
// pa se kod iz skripte kopira i odmah kompajlira. Redove dodajemo
// po nedeljama; redovi za pakete koji jos nisu dodati ostaju
// zakomentarisani (inace greska CS0246).

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

// ---------- nedelja 4-6: EF Core, slojevi, validacija ----------
// global using Microsoft.EntityFrameworkCore;                  // paket (nedelja 4)
// global using Microsoft.EntityFrameworkCore.Metadata.Builders;
// global using FluentValidation;                               // paket (nedelja 6)

// ---------- namespace-ovi projekta MiniShop.Api ----------
global using MiniShop.Api.Dtos;
global using MiniShop.Api.Errors;
global using MiniShop.Api.Extensions;
global using MiniShop.Api.Filters;
global using MiniShop.Api.Middleware;
global using MiniShop.Api.Options;
global using MiniShop.Api.Services;
