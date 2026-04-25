global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using System.Security.Claims;
global using System.Text.Json;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.IdentityModel.Tokens;
global using NovaDrive.Application;
global using NovaDrive.Api.Middleware;
global using NovaDrive.Infrastructure;
global using Serilog;
global using Serilog.Events;


global using NovaDrive.Domain.Interfaces;
global using NovaDrive.Domain.Exceptions;
global using NovaDrive.Application.Services;
global using NovaDrive.Application.DTOs;
global using NovaDrive.Infrastructure.Data;
global using NovaDrive.Api.Extensions;
global using NovaDrive.Api.Endpoints;
global using NovaDrive.Domain.Entities;