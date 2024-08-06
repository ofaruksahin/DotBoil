namespace DotBoil.TemplateEngine
{
    public interface IRazorRenderer
    {
        Task<string> RenderAsync(string templateName);
        Task<string> RenderAsync<TModel>(string templateName, TModel model) where TModel : class;
    }
}
