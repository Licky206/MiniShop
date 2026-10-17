// MiniShop.Tests (nedelja 6); Xunit dolazi iz .csproj (sablon)
global using Moq;
global using FluentAssertions;                // paket FluentAssertions 7.x
global using FluentValidation.TestHelper;     // deo paketa FluentValidation
global using MiniShop.Domain.Entities;
global using MiniShop.Application.Abstractions;
global using MiniShop.Application.Common;
global using MiniShop.Application.Dtos;
global using MiniShop.Application.Services;
global using MiniShop.Application.Validators;