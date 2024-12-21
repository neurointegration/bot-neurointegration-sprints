using System.Collections.Immutable;
using Common.Ydb.Fields;


namespace Neurointegration.Api.Storages.Answers;

public class AnswerTableProperties
{
    public readonly string TableName = "user_sprint_answers";

    public readonly Int64YbdField UserId;
    public readonly Int32YbdField SprintReplyNumber;
    public readonly Int32YbdField AnswerNumber;
    public readonly Int64YbdField SprintNumber;
    public readonly Utf8YdbField Answer;
    public readonly Utf8YdbField ScenarioType;
    public readonly DateYdbField Date;

    private readonly IImmutableList<IYdbField> fields;


    public AnswerTableProperties()
    {
        UserId = new Int64YbdField("user_id");
        Date = new DateYdbField("date");
        Answer = new Utf8YdbField("answer");
        ScenarioType = new Utf8YdbField("scenario_type");
        AnswerNumber = new Int32YbdField("answer_number");
        SprintNumber = new Int64YbdField("sprint_number");
        SprintReplyNumber = new Int32YbdField("sprint_reply_number");

        fields = new List<IYdbField>()
        {
            UserId,
            Date,
            Answer,
            ScenarioType,
            AnswerNumber,
            SprintNumber,
            SprintReplyNumber
        }.ToImmutableArray();
    }

    public string Declare()
    {
        return string.Join("\n", fields.Select(x => x.FieldDeclare));
    }

    public string GetSchema()
    {
        return $@"
             CREATE TABLE {TableName} (
                {Date.Name} {Date.Type} NOT NULL,
                {UserId.Name} {UserId.Type} NOT NULL,
                {Answer.Name} {Answer.Type} NOT NULL,
                {ScenarioType.Name} {ScenarioType.Type} NOT NULL,
                {AnswerNumber.Name} {AnswerNumber.Type} NOT NULL,
                {SprintReplyNumber.Name} {SprintReplyNumber.Type} NOT NULL,
                {SprintNumber.Name} {SprintNumber.Type} NOT NULL,
                PRIMARY KEY ({UserId.Name}, {ScenarioType.Name}, {SprintReplyNumber.Name}, {AnswerNumber.Name})
             )
         ";
    }
}