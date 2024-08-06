using RazorLight;

namespace DotBoil.TemplateEngine
{
    internal class RazorRenderer : IRazorRenderer
    {
        private readonly RazorLightEngine _engine;

        public RazorRenderer(RazorLightEngine engine)
        {
            _engine = engine;
        }

        public async Task<string> RenderAsync(string templateName)
        {
            return await _engine.CompileRenderAsync(templateName, new { });
        }

        public async Task<string> RenderAsync<TModel>(string templateName, TModel model) where TModel : class
        {
            return await _engine.CompileRenderAsync<TModel>(templateName, model);
        }
    }
}
