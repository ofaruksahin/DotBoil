# DotBoil

DotBoil is a development initiative based on .NET 9 Preview. The name is derived from the words "Dotnet" and "Boilerplate". The purpose of the DotBoil project is to automate the fundamental integrations that a developer typically implements at the start of every project. With DotBoil, I aim to have your project ready for coding in just a matter of minutes.

## Current Integrations

At this point, the following integrations have been automated for developers:

| Integration            | Description                                                                                              |
|------------------------|----------------------------------------------------------------------------------------------------------|
| **Caching**            | Solves caching issues in your project using Redis.                                                       |
| **CORS**               | Allows for quick and easy configuration of CORS settings.                                                |
| **EntityFrameworkCore** | Simplifies database operations using the repository pattern and supports interceptor structures.         |
| **Email Integrations**  | Quickly set up email integration for notifications and alerts to your users.                             |
| **Health Check**        | Monitor the health of your project's dependencies, as well as the overall health of your project.        |
| **Logging**            | Implement logging with Serilog infrastructure, fully compatible with custom sinks.                       |
| **AutoMapper**         | Create AutoMapper profiles and eliminate mapping issues easily.                                           |
| **MassTransit**        | Use the inbox & outbox pattern for queuing, ensuring smooth messaging with RabbitMQ and MySQL.            |
| **Swagger**            | Document your project quickly and efficiently.                                                           |
| **Template Engine**    | Use Razor syntax to create email and message templates in your project.                                  |
| **FluentValidator**    | Automate validation for the models in your project.                                                      |
| **API Versioning**     | Automate API versioning and handle deprecation of versions seamlessly.                                   |

## Upcoming Packages

The following packages are planned for future integration:

| Package                 | Description                                                                                              |
|-------------------------|----------------------------------------------------------------------------------------------------------|
| **Localization**        | Provides multilingual support for applications using Redis and MySQL.                                   |
| **Parameters**          | Manages application parameters using Redis and MySQL.                                                  |
| **Pagination and Sorting** | Implements sorting and pagination operations in database tables using EntityFrameworkCore.             |
| **Authentication & Authorization** | Creates a membership system based on tenant and OAuth 2.0 protocol, handling authorization for created memberships. |

## Goals

The primary goal of DotBoil is to simplify the project setup process, saving developers valuable time by reducing repetitive setup tasks. Whether you are starting a new API or web application, DotBoil provides a solid foundation to begin your development efforts immediately.
