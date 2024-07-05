using K.Extensions.EfCore.Build.Services;

namespace K.Extensions.EfCore.Build
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var scriptGenerationService = new ScriptGenerationService();
            scriptGenerationService.GenerateBuildScript();
        }
    }
}
