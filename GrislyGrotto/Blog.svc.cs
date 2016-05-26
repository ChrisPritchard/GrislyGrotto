using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using System.Xml.Linq;

namespace GrislyGrotto
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class Blog
    {
        readonly GrislyGrottoEntitiesAzure data;
        readonly Random random;
        readonly TimeZoneInfo newZealandTime;

        const string editorStateKey = "editorState";

        readonly Dictionary<CredentialsDto, string> users = new Dictionary<CredentialsDto, string>
        {
            {new CredentialsDto {Username = "aquinas", Password = "***REMOVED***"}, "Christopher"},
            {new CredentialsDto {Username = "pdc", Password = "***REMOVED***"}, "Peter"},
        };

        public Blog()
        {
            data = new GrislyGrottoEntitiesAzure();
            random = new Random();
            newZealandTime = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
        }

        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public KeyValuePair<string, string> Quote()
        {
            var quotesXml = XElement.Load(HttpContext.Current.Server.MapPath("/resources/quotes.xml"));
            var index = random.Next(0, quotesXml.Elements().Count());
            var quote = quotesXml.Elements().ElementAt(index);
            var author = quote.Attribute("Author");
            return new KeyValuePair<string, string>(author != null ? author.Value : "Anonymous", quote.Value);
        }

        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public PostDto[] LatestPosts()
        {
            var allComments = data.Comments.GroupBy(c => c.PostID).ToDictionary(c => c.Key, c => c.Count());
            return data.Posts.OrderByDescending(p => p.Created).Take(5).ToArray()
                .Select(p => PostDto.FromPostAndCommentCount(p, allComments.ContainsKey(p.ID) ? allComments[p.ID] : 0))
                .ToArray();
        }

        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Post/{id}")]
        [OperationContract]
        public PostDto Post(string id)
        {
            var parsedId = SafeParse(id);
            var post = data.Posts.SingleOrDefault(p => p.ID == parsedId);
            return post == null ? null : PostDto.FromPostAndComments(post, data.Comments.Where(c => c.PostID == parsedId).ToArray());
        }

        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public MonthCount[] Archives()
        {
            return data.Posts.Select(p => p.Created)
                .GroupBy(c => new {c.Month, c.Year})
                .OrderByDescending(p => p.Key.Year)
                .ThenByDescending(p => p.Key.Month)
                .Select(g => new MonthCount {Year = g.Key.Year, Month = g.Key.Month, PostCount = g.Count()})
                .ToArray();
        }

        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public Story[] Stories()
        {
            return data.Posts.Where(p => p.Type.Equals("Story"))
                .ToArray()
                .Select(Story.FromPost)
                .OrderByDescending(s => s.TimePosted).ToArray();
        }

        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Month/{year}/{month}")]
        [OperationContract]
        public PostDto[] Month(string year, string month)
        {
            var yearNum = SafeParse(year);
            var monthNum = month.AsMonthNum();

            var allComments = data.Comments.GroupBy(c => c.PostID).ToDictionary(c => c.Key, c => c.Count());
            return data.Posts.Where(p => p.Created.Year == yearNum && p.Created.Month == monthNum)
                .OrderBy(p => p.Created).ToArray()
                .Select(p => PostDto.FromPostAndCommentCount(p, allComments.ContainsKey(p.ID) ? allComments[p.ID] : 0))
                .ToArray();
        }

        [OperationContract]
        [WebGet(UriTemplate = "/Search/{searchTerm}", ResponseFormat = WebMessageFormat.Json)]
        public PostDto[] Search(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();

            var allComments = data.Comments.GroupBy(c => c.PostID).ToDictionary(c => c.Key, c => c.Count());
            return data.Posts.Where(p => p.Title.Contains(searchTerm) || p.Content.Contains(searchTerm))
                .OrderByDescending(p => p.Created).ToArray()
                .Select(p => PostDto.FromPostAndCommentCount(p, allComments.ContainsKey(p.ID) ? allComments[p.ID] : 0))
                .ToArray();
        }

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public bool CheckAuthentication(CredentialsDto credentials)
        {
            return users.Any(u => u.Key.IsMatch(credentials));
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "/CheckAuthenticationForPost/{postId}", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public bool CheckAuthenticationForPost(CredentialsDto credentials, string postId)
        {
            var parsedId = int.Parse(postId);
            var post = data.Posts.SingleOrDefault(p => p.ID == parsedId);
            return users.Any(u => u.Key.IsMatch(credentials) && u.Value.Equals(post.Author));
        }

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json)]
        public void SaveEditorState(PostDto post)
        {
            HttpContext.Current.Session[editorStateKey] = post;
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        public PostDto GetEditorState()
        {
            var savedData = HttpContext.Current.Session[editorStateKey];
            return savedData != null ? (PostDto) savedData : null;
        }

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json)]
        public void AddOrEditPost(PostRequest request)
        {
            var isExistingPost = request.Post.Id != 0;
            var userIsValid = isExistingPost
                ? CheckAuthenticationForPost(request.Credentials, request.Post.Id.ToString())
                : CheckAuthentication(request.Credentials);

            if(!userIsValid)
                return;

            request.Post.Title = request.Post.Title.Replace("\n", string.Empty).Trim();
            request.Post.Content = request.Post.Content.Replace("\n", string.Empty).Trim();

            if(isExistingPost)
            {
                var post = data.Posts.SingleOrDefault(p => p.ID == request.Post.Id);
                if (post == null)
                    return;
                post.Title = request.Post.Title;
                post.Content = request.Post.Content;
                post.Type = request.Post.Type;
            }
            else
            {
                var newPost = new Post
                {
                    Author = users.Single(u => u.Key.IsMatch(request.Credentials)).Value,
                    Created = TimeZoneInfo.ConvertTime(DateTime.Now, newZealandTime),
                    Title = request.Post.Title,
                    Content = request.Post.Content,
                    Type = request.Post.Type
                };
                data.Posts.Add(newPost);
            }

            data.SaveChanges();
            HttpContext.Current.Session[editorStateKey] = null;
        }

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json)]
        public void DeletePost(DeletePostRequest request)
        {
            if(!CheckAuthenticationForPost(request.Credentials, request.Id.ToString()))
                return;
            var post = data.Posts.SingleOrDefault(p => p.ID == request.Id);
            if (post == null)
                return;

            data.Posts.Remove(post);
            data.Comments.Where(c => c.PostID == request.Id).ToList().ForEach(c => data.Comments.Remove(c));
            data.SaveChanges();

            HttpContext.Current.Session[editorStateKey] = null;
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "/AddComment/{postId}", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json)]
        public void AddComment(CommentDto comment, string postId)
        {
            var post = SafeParse(postId);
            if(post == 0)
                return;

            comment.Text = comment.Text.Trim();

            data.Comments.Add(new Comment { Author = comment.Author, Created = TimeZoneInfo.ConvertTime(DateTime.Now, newZealandTime), Text = comment.Text, PostID = int.Parse(postId) });
            data.SaveChanges();
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "/DeleteComment/{commentId}")]
        public void DeleteComment(string commentId)
        {
            var id = SafeParse(commentId);
            var comment = data.Comments.SingleOrDefault(c => c.ID == id);
            if(comment == null)
                return;
            data.Comments.Remove(comment);
            data.SaveChanges();
        }

        private static int SafeParse(string number)
        {
            int result;
            return int.TryParse(number, out result) ? result : 0;
        }

        public class Story
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public DateTime TimePosted { get; set; }
            public int WordCount { get; set; }

            public static Story FromPost(Post basePost)
            {
                return new Story
                {
                    Id = basePost.ID,
                    Title = basePost.Title,
                    Author = basePost.Author,
                    TimePosted = basePost.Created,
                    WordCount = basePost.Content.Split(' ').Length
                };
            }
        }

        public class MonthCount
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public string MonthName { get { return Month.AsMonthName(); } set { Month = value.AsMonthNum(); } }
            public int PostCount { get; set; }
        }

        public class CredentialsDto
        {
            public string Username { get; set; }
            public string Password { get; set; }

            public bool IsMatch(CredentialsDto other)
            {
                return Username.Equals(other.Username) && Password.Equals(other.Password);
            }
        }

        public class PostRequest
        {
            public CredentialsDto Credentials { get; set; }
            public PostDto Post { get; set; }
        }

        public class DeletePostRequest
        {
            public CredentialsDto Credentials { get; set; }
            public int Id { get; set; }
        }

        public class PostDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Created { get; set; }
            public string Content { get; set; }

            public int CommentCount { get; set; }
            public CommentDto[] Comments { get; set; }

            public string Type { get; set; }

            public static PostDto FromPostAndComments(Post basePost, Comment[] comments)
            {
                var post = FromPostAndCommentCount(basePost, comments.Length);
                post.Comments = comments.OrderBy(c => c.Created).Select(c => new CommentDto
                {
                    Id = c.ID,
                    Author = c.Author,
                    Created = c.Created.ToString("dddd, d MMMM yyyy, hh:mm tt"),
                    Text = c.Text
                }).ToArray();
                return post;
            }

            public static PostDto FromPostAndCommentCount(Post basePost, int commentCount)
            {
                return new PostDto
                {
                    Id = basePost.ID,
                    Title = basePost.Title,
                    Author = basePost.Author,
                    Created = basePost.Created.ToString("dddd, d MMMM yyyy, hh:mm tt"),
                    Content = basePost.Content,
                    CommentCount = commentCount,
                    Type = basePost.Type
                };
            }
        }

        public class CommentDto
        {
            public int Id { get; set; }
            public string Author { get; set; }
            public string Created { get; set; }
            public string Text { get; set; }   
        }
    }
}
