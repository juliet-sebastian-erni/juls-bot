namespace EchoBot.Bots
{
    public class ConversationFlow
{
    // Identifies the last question asked.
    public enum Question
    {
        Name,
        Age,
        Email,
        Number,
        DateTimeOfAppointment,
        Service,
        None, 
    }

    // The last question asked.
    public Question LastQuestionAsked { get; set; } = Question.None;
}
}