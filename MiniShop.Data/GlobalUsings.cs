// MiniShop.Data: EF Core + apstrakcije iz Application sloja
global using System.Linq.Expressions;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;   // EntityTypeBuilder
global using Microsoft.Extensions.Configuration;                // IConfiguration
global using Microsoft.Extensions.DependencyInjection;
global using MiniShop.Domain.Entities;
global using MiniShop.Application.Abstractions;
global using MiniShop.Application.Common;
global using MiniShop.Application.Dtos;
global using MiniShop.Application.Mapping;
global using MiniShop.Data.Repositories;
global using Microsoft.AspNetCore.Identity;                        // UserManager, RoleManager
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;    // IdentityDbContext
global using Microsoft.AspNetCore.Identity;                        // UserManager, RoleManager
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;    // IdentityDbContext
global using Microsoft.Extensions.Configuration;                   // IConfiguration
global using Microsoft.Extensions.DependencyInjection;             // GetRequiredService