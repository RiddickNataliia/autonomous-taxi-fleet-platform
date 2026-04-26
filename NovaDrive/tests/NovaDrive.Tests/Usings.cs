global using Xunit;
global using FluentAssertions;
global using System.Net;
global using System.Net.Http.Json;

// MongoDB
global using MongoDB.Bson;
global using MongoDB.Bson.Serialization;
global using MongoDB.Bson.Serialization.Serializers;
global using MongoDB.Driver;
global using Testcontainers.MongoDb;

// Postgres / EF
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.EntityFrameworkCore;
global using Testcontainers.PostgreSql;
global using NovaDrive.Infrastructure.Data;

// WebApplicationFactory
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

// Domain
global using NovaDrive.Domain.Entities;
global using NovaDrive.Domain.Enums;
global using NovaDrive.Domain.ValueObjects;
global using NovaDrive.Domain.Exceptions;
global using NovaDrive.Domain.Services;

// Infrastructure
global using NovaDrive.Infrastructure.Repositories;