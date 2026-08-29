using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MangaTracker.Api.OpenApi;

public class JwtBearerDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document, 
        OpenApiDocumentTransformerContext context, 
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        // Definir el esquema Bearer JWT
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Ingrese su token JWT. Ejemplo: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        };

        // Aplicar el requisito de seguridad a TODOS los endpoints por defecto
        // document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
        // document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        // {
        //     {
        //         new OpenApiSecurityScheme
        //         {
        //             Reference = new OpenApiReference
        //             {
        //                 Type = ReferenceType.SecurityScheme,
        //                 Id = "Bearer"
        //             }
        //         },
        //         Array.Empty<string>()
        //     }
        // });

        return Task.CompletedTask;
    }
}