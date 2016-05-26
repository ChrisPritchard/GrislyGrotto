using System;
using GrislyGrotto.Infrastructure.Domain;
using NUnit.Framework;

namespace GrislyGrotto.UnitTests.Infrastructure.Domain
{
    [TestFixture]
    public class CommentTests
    {
        [Test]
        public void Should_Initialize_Properties_Correctly()
        {
            var comment = new Comment(666, new DateTime(1984, 6, 24), "Chris", "Sample Test");

            Assert.That(comment.PostID == 666);
            Assert.That(comment.EntryDate.ToString("dd MMM yyyy").Equals("24 Jun 1984"));
            Assert.That(comment.Author.Equals("Chris"));
            Assert.That(comment.Text.Equals("Sample Test"));
        }
    }
}
