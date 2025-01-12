using System.Collections.Immutable;
using Common.Ydb.Fields;
using Common.Ydb.Schema;


namespace Neurointegration.Api.Storages.Answers;

public class AnswerTableSchema: BaseTableSchema
{
    public override string TableName => "user_sprint_answers";

    public readonly Int64YbdField UserId;
    public readonly Int32YbdField SprintReplyNumber;
    public readonly Int32YbdField AnswerNumber;
    public readonly Int64YbdField SprintNumber;
    public readonly Utf8YdbField Answer;
    public readonly Utf8YdbField ScenarioType;
    public readonly DateYdbField Date;

    public override IImmutableList<YdbField> Fields { get; }


    public AnswerTableSchema()
    {
        UserId = new Int64YbdField("user_id", FieldConditions.NotNull, FieldConditions.IsPrimaryKey);
        Date = new DateYdbField("date", FieldConditions.NotNull);
        Answer = new Utf8YdbField("answer", FieldConditions.NotNull);
        ScenarioType = new Utf8YdbField("scenario_type", FieldConditions.NotNull, FieldConditions.IsPrimaryKey);
        AnswerNumber = new Int32YbdField("answer_number", FieldConditions.NotNull, FieldConditions.IsPrimaryKey);
        SprintNumber = new Int64YbdField("sprint_number", FieldConditions.NotNull, FieldConditions.IsPrimaryKey);
        SprintReplyNumber = new Int32YbdField("sprint_reply_number", FieldConditions.NotNull, FieldConditions.IsPrimaryKey);

        Fields = new List<YdbField>()
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
}