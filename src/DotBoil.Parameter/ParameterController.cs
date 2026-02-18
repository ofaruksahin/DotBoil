using System.Net;
using System.Reflection;
using DotBoil.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotBoil.Parameter;

public class ParameterController : BaseController
{
    private static readonly Dictionary<string, Type> ClrTypeAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["string"] = typeof(string),
        ["bool"] = typeof(bool),
        ["boolean"] = typeof(bool),
        ["byte"] = typeof(byte),
        ["sbyte"] = typeof(sbyte),
        ["short"] = typeof(short),
        ["int16"] = typeof(short),
        ["ushort"] = typeof(ushort),
        ["uint16"] = typeof(ushort),
        ["int"] = typeof(int),
        ["int32"] = typeof(int),
        ["uint"] = typeof(uint),
        ["uint32"] = typeof(uint),
        ["long"] = typeof(long),
        ["int64"] = typeof(long),
        ["ulong"] = typeof(ulong),
        ["uint64"] = typeof(ulong),
        ["float"] = typeof(float),
        ["single"] = typeof(float),
        ["double"] = typeof(double),
        ["decimal"] = typeof(decimal),
        ["char"] = typeof(char),
        ["guid"] = typeof(Guid),
        ["datetime"] = typeof(DateTime),
        ["datetimeoffset"] = typeof(DateTimeOffset),
        ["timespan"] = typeof(TimeSpan)
    };

    private static readonly MethodInfo? GetParameterValueMethod =
        typeof(IParameterManager).GetMethods()
            .FirstOrDefault(method =>
                method.Name == nameof(IParameterManager.GetParameterValue) &&
                method.IsGenericMethodDefinition &&
                method.GetParameters().Length == 4);

    private readonly IParameterManager _parameterManager;

    public ParameterController(IParameterManager parameterManager)
    {
        _parameterManager = parameterManager;
    }
    
    /// <summary>
    /// Public bir parametreyi getirmek için kullanılır.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
    public async Task<BaseResponse> GetParameter([FromBody] GetParameterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BaseResponse.Response(HttpStatusCode.BadRequest, "Name alanı zorunludur.");

        if (string.IsNullOrWhiteSpace(request.ClrType))
            return BaseResponse.Response(HttpStatusCode.BadRequest, "ClrType alanı zorunludur.");

        var clrType = ResolveClrType(request.ClrType);
        if (clrType is null)
            return BaseResponse.Response(HttpStatusCode.BadRequest, $"ClrType desteklenmiyor: {request.ClrType}");

        if (GetParameterValueMethod is null)
            return BaseResponse.Response(HttpStatusCode.InternalServerError, "GetParameterValue metodu bulunamadı.");

        var genericMethod = GetParameterValueMethod.MakeGenericMethod(clrType);
        var methodTask = (Task?)genericMethod.Invoke(_parameterManager, [request.TenantId, request.Section, request.Name, true]);

        if (methodTask is null)
            return BaseResponse.Response(HttpStatusCode.InternalServerError, "Parametre değeri okunamadı.");

        await methodTask.ConfigureAwait(false);
        var data = methodTask.GetType().GetProperty("Result")?.GetValue(methodTask);

        return BaseResponse.Response(data, HttpStatusCode.OK);
    }

    private static Type? ResolveClrType(string clrType)
    {
        var normalizedClrType = clrType.Trim();

        if (ClrTypeAliases.TryGetValue(normalizedClrType, out var aliasClrType))
            return aliasClrType;

        return Type.GetType(normalizedClrType, throwOnError: false, ignoreCase: true);
    }
}

public class GetParameterRequest
{
    public int TenantId { get; set; }
    public string Section { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ClrType { get; set; } = string.Empty;
}
