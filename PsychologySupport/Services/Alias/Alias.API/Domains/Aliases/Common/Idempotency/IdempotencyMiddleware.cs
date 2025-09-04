namespace Alias.API.Domains.Aliases.Common.Idempotency;

public class IdempotencyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IdempotencyStore store)
    {
        var isIdEmp = context.GetEndpoint()?.Metadata?.GetMetadata<IdempotentEndpoint>() is not null;

        if (!isIdEmp)
        {
            await next(context);
            return;
        }

        var key = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(key))
        {
            await next(context);
            return;
        }

        var scopeKey = $"{context.Request.Path}|{key}";
        //Nếu idempotency key đã tồn tại => trả về response của lần trước nằm trong cache
        //thay vì thực hiện lại pipeline lần nữa
        if (store.TryGet(scopeKey, out var cached))
        {
            context.Response.StatusCode = cached.Item1;
            await context.Response.WriteAsync(cached.Item2);
            return;
        }

        //Nếu idempotency chưa tồn tại thì ghi vô store
        var originalBodyStream = context.Response.Body;
        using var mem = new MemoryStream();
        context.Response.Body = mem;
        await next(context);
        mem.Position = 0;
        using var reader = new StreamReader(mem);
        var body = await reader.ReadToEndAsync();
        store.Set(scopeKey, context.Response.StatusCode, body);
        mem.Position = 0;
        await mem.CopyToAsync(originalBodyStream);
    }
}