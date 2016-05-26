using GrislyGrotto.App.Shared;
using System;
using System.ComponentModel.DataAnnotations;

namespace GrislyGrotto.App.Data
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Comment
    {
        // ReSharper disable once UnusedMember.Global
        public int Id { get; set; }
        public virtual Post Post { get; set; }

        [Required]
        public string Author { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime Date { get; set; }

        public string DateFormatted => Date.Add(Events.NzTimeZone).ToString("hh:mm tt, dddd dd MMMM, yyyy");
    }
}