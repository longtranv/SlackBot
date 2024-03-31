namespace SlackBot.Models
{
    public class AttendenceRecord
    {
        public int Id { get; set; }
        public int MemberId { get; set; }  // Assuming this is the foreign key to the User table
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }

        // Other properties as needed...

        // Navigation property to represent the relationship with the User entity
        public Member? Member { get; set; }

    }
}
