using Octokit;

namespace MetricHunter.Application.Git;

public static class GitConsts
{
    public const string RepositoryInfoFileExtension = ".githunterinfo";

    public static readonly Dictionary<string, Language> LanguagesMap = new()
    {
        { "C#", Language.CSharp },
        { "C++", Language.CPlusPlus },
        { "C", Language.C },
        { "Java", Language.Java },
        { "JavaScript", Language.JavaScript },
        { "Python", Language.Python },
        { "Ruby", Language.Ruby },
        { "TypeScript", Language.TypeScript },
        { "Shell", Language.Shell }
    };
}