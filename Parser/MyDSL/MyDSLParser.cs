
using Sprache;

namespace Parser.MyDSL;

public class Questionnaire
{
    public Questionnaire(IEnumerable<Section> sections)
    {
        Sections = sections;
    }

    public IEnumerable<Section> Sections { get; private set; }
}

public class Section
{
    public Section(string id, string title, IEnumerable<Question> questions)
    {
        Id = id;
        Title = title;
        Questions = questions;
    }

    public string Id { get; private set; }

    public string Title { get; private set; }

    public IEnumerable<Question> Questions { get; private set; }
}

public class Question
{
    public Question(string id, string prompt)
    {
        Id = id;
        Prompt = prompt;
    }

    public string Id { get; private set; }

    public string Prompt { get; private set; }
}

static class MyDSLParser
{
    public static readonly Parser<string> Identifier = Parse.Letter.AtLeastOnce().Text().Token();

    public static readonly Parser<string> QuotedText =
        (from open in Parse.Char('"')
        from content in Parse.CharExcept('"').Many().Text()
        from close in Parse.Char('"')
        select content).Token();

    public static readonly Parser<Question> Question =
        from id in Identifier
        from prompt in QuotedText
        select new Question(id, prompt);

    public static readonly Parser<Section> Section =
        from id in Identifier
        from title in QuotedText
        from lbracket in Parse.Char('[').Token()
        from questions in Question.Many()
        from rbracket in Parse.Char(']').Token()
        select new Section(id, title, questions);

    public static Parser<Questionnaire> Questionnaire =
        Section.Many().Select(sections => new Questionnaire(sections)).End();
}
