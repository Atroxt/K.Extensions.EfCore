# EfCore Extension

![EfCore.Extension](Icon.png)

[![Build](https://github.com/Atroxt/K.Extensions.EfCore/actions/workflows/build.yml/badge.svg)](https://github.com/Atroxt/K.Extensions.EfCore/actions/workflows/build.yml)
![Nuget](https://img.shields.io/nuget/v/K.Extensions.EfCore?logo=nuget)
![Nuget](https://img.shields.io/nuget/dt/K.Extensions.EfCore?logo=nuget&label=Downloads)
# K.Extensions.EfCore
is a .NET library that enables you to implement interfaces for your entities to support soft deletes and other functionalities.

## Introduction
Since some of these actions are frequently used repeatedly, this extension library was created.

This library provides extended functionalities in conjunction with EntityFramework. It includes mechanisms for automatic logging of creation and modification times as well as soft deleting of entities.

## How It Works

First, add the package.

### Implementing the Interfaces
To use the extended functions, your entity classes should implement the corresponding interfaces.

```csharp

using System;
using K.Extensions.Datetime.Models;

public class MyEntity : IAmAuditAble, IAmSoftdeleteAble, IHaveCreationTime, IHaveModificationTime
{
    public int Id { get; set; }
    public DateTime CreationTime { get; private set; }
    public DateTime? ModificationTime { get; private set; }
    public DateTime? DeletionTime { get; private set; }
    public bool IsDeleted { get; private set; }
}

```

### Configuring the DbContext
Extend your DbContext from BaseDbContext to enable audit and timestamp functionality.

```csharp
using K.Extensions.Datetime;
using Microsoft.EntityFrameworkCore;

public class MyDbContext : BaseDbContext
{
    public DbSet<MyEntity> MyEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyDbContext).Assembly);
    }
}
```

Using BaseDbContext also creates audit logging in the database. The audit logs include:

```csharp
public Guid Id { get; set; }
public string EntityName { get; set; }
public string EntityId { get; set; }
public string Operation { get; set; }
public string Changes { get; set; }
public DateTime Timestamp { get; set; }
```

There is also the option to use only the interceptors in your DbContext.
For this, the corresponding interceptors should be included in your dependency injection setup.

```csharp
var sp = new ServiceCollection()
    .AddSingleton<SoftDeleteInterceptor>()
    .AddSingleton<ModificationTimeInterceptor>()
    .AddSingleton<CreationTimeInterceptor>()
    .BuildServiceProvider();

    var options = new DbContextOptionsBuilder<InterceptorDbContext>()
    ....
    .AddInterceptors(
        sp.GetRequiredService<SoftDeleteInterceptor>(),
        sp.GetRequiredService<ModificationTimeInterceptor>(),
        sp.GetRequiredService<CreationTimeInterceptor>()
        )
    ...

```
When using the interceptors without BaseDbContext, audit logging is not included. For this, you might need to implement your own audit logging mechanism.

### Using the Interceptors
The interceptors are automatically applied when you create or update your entities in the database.