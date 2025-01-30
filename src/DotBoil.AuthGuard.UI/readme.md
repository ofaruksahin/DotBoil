## Appsettings.json
```json
{
  "DotBoil": {
    "Localization": {
      "Caching": {
        "ConnectionString": "192.168.1.10:6379,password=123456",
        "ExpireInHour": 12
      },
      "Persistence": {
        "ConnectionString": "server=192.168.1.10;uid=root;pwd=123456;database=DotBoilAuthGuard"
      }
    },
    "Parameters": {
      "Caching": {
        "ConnectionString": "192.168.1.10:6379,password=123456",
        "ExpireInHour": 12
      },
      "Persistence": {
        "ConnectionString": "server=192.168.1.10;uid=root;pwd=123456;database=DotBoilAuthGuard"
      }
    },
    "Caching": {
      "Redis": {
        "Endpoints":[
          {
            "IpAddress": "192.168.1.10",
            "Port": 6379
          }
        ],
        "Password": "123456"
      }
    },
    "Logging": {
      "FileSink": {
        "LogFileName": "/Users/omerfaruksahin/Desktop/logs/AuthGuard",
        "RollingInterval": "Day"
      }
    },
    "Mediator": {
      "Pipelines": [
        "DotBoil.Validator.ValidationBehaviour"
      ]
    },
    "EFCore": {
      "Contexts": [
        {
          "TypeName": "DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts.DotBoilAuthGuardDbContext",
          "Interceptors": [
            "DotBoil.EFCore.Interceptors.AuditInterceptor",
            "DotBoil.MassTransit.Persistence.MassTransitDbContextSaveChangesInterceptor"
          ]
        }
      ]
    },
    "AuthGuard": {
      "DbContext": {
        "ConnectionString": "server=192.168.1.10;uid=root;pwd=123456;database=DotBoilAuthGuard"
      },
      "JwtOptions": {
        "SecretKey": "",
        "Issuer": "https://localhost:7088",
        "Audience": "https://localhost:7088",
        "AccessTokenExpirationMinutes": 60,
        "RefreshTokenExpirationMinutes": 240
      }
    },
    "Cors": {
      "PolicyName": "Default",
      "Origins": [
        "http://localhost:3000", "https://localhost:3000"
      ]
    },
    "AvailableLanguage": [
      {
        "Shortcut": "EN",
        "Name" : "English",
        "Icon": "flag-icon-us"
      },
      {
        "Shortcut": "TR",
        "Name": "Türkçe",
        "Icon": "flag-icon-tr"
      }
    ],
    "ExternalSignInManagers": [
      {
        "Logo": "fa fa-github",
        "ServiceName": "Github",
        "AuthorizationEndpoint": "",
        "TokenEndpoint": "",
        "UserInfoEndpoint": "https://api.github.com/user",
        "EmailIdentifier": "email",
        "UsernameIdentifier": "email",
        "NameIdentifier": "name",
        "SurnameIdentifier": "",
        "RedirectUrl": "https://localhost:3000"
      }
    ],
    "MessageBroker": {
      "MassTransit": {
        "Persistence": {
          "PersistenceType" : "MySql",
          "MySql": {
            "ConnectionString": "server=192.168.1.10;uid=root;pwd=123456;database=DotBoilAuthGuard"
          }
        },
        "RabbitMq": {
          "Host": "192.168.1.10",
          "Port": 5672,
          "Username": "root",
          "Password": "123456"
        }
      }
    }
  }
}
```