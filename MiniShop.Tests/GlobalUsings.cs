global using System.Net;
global using System.Net.Http.Json;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.Data.Sqlite;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Infrastructure;     // IDbContextOptionsConfiguration<T>
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;   // RemoveAll
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Options;
global using System.Security.Claims;                     // OrdersAuthorizationTests (N11)
global using Microsoft.AspNetCore.Authorization;         // OrdersAuthorizationTests (N11)
global using Xunit;
global using Moq;
global using FluentAssertions;
global using FluentValidation.TestHelper;
global using MiniShop.Api.Auth;
global using MiniShop.Api.Options;
global using MiniShop.Application.Abstractions;
global using MiniShop.Application.Common;
global using MiniShop.Application.Dtos;
global using MiniShop.Application.Services;
global using MiniShop.Application.Validators;
global using MiniShop.Data;
global using MiniShop.Domain.Entities;