namespace ASI.Basecode.WebApp.Utils
{
    public enum RoleType
    {
        User = 1,
        SupportAgent = 2,
        SuperAdmin = 3,
        Administrator = 4
    }

    public class Constant
    {
        public const string USER_SUCCESS_TICKET_MESSAGE = "Thank you! Your ticket has been successfully created. Our support team will review it shortly. Please keep your ticket ID for reference. We will get back to you as soon as possible.";

        public static readonly string SUPPORT_AGENT_TICKET_NOTIFICATION =
            "A new ticket has been created by {0} User ID: {1}. Ticket ID: {2}. Subject: {3}. " +
            "Please review the details and respond as soon as possible.";


        //Assign
        public static readonly string TICKET_ASSIGNED_MESSAGE_FOR_ASSIGNED_USER = "A new ticket was assigned to you by your other team with username of: {0}. Please review the details as soon as possible.";
        public static readonly string TICKET_ASSIGNED_MESSAGE_FOR_ASSIGNER = "You've assign a ticket to your team with username of: {0}. The user already notified and assigned the ticket to be resolve";

        //Re-Assign
        public static readonly string TICKET_RE_ASSIGNED_MESSAGE_FOR_ASSIGNED_USER = "An existing ticket was re-assigned to you by your other team with username of: {0}. Please review the details as soon as possible.";
        public static readonly string TICKET_RE_ASSIGNED_MESSAGE_FOR_ASSIGNER = "You've re-assign a ticket to your team with username of: {0}. The user already notified and assigned the ticket to be resolve";

    }
}
