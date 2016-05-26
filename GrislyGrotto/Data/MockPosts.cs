using System;
using System.Linq;
using System.Collections.Generic;
using GrislyGrotto.Data.Primitives;

namespace GrislyGrotto.Data
{
    internal class MockPosts : IPosts
    {
        private static List<Post> posts;

        public MockPosts()
        {
            posts = new List<Post>
                { post0, post1, post2, post3, post4, post5, post6, post7, post8, post9 };
        }

        public IEnumerable<Post> LatestPosts(string user)
        {
            return string.IsNullOrEmpty(user)
                       ? posts.OrderByDescending(p => p.TimePosted).Take(5)
                       : posts.Where(p => p.Author.Equals(user)).OrderByDescending(p => p.TimePosted).Take(5);
        }

        public IEnumerable<Story> PostsThatAreStories(string user)
        {
            return string.IsNullOrEmpty(user)
                       ? posts.Where(p => p.IsStory).OrderByDescending(p => p.TimePosted).Select(p => new Story(p))
                       : posts.Where(p => p.IsStory && p.Author.Equals(user)).OrderByDescending(p => p.TimePosted).Select(p => new Story(p));
        }

        public IEnumerable<Post> PostsForMonth(int year, int month, string user)
        {
            return string.IsNullOrEmpty(user)
                       ? posts.Where(p => p.TimePosted.Year == year && p.TimePosted.Month == month).OrderByDescending(p => p.TimePosted)
                       : posts.Where(p => p.TimePosted.Year == year && p.TimePosted.Month == month && p.Author.Equals(user)).OrderByDescending(p => p.TimePosted);
        }

        public IEnumerable<Post> SearchResults(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();
            return posts.Where(p =>
                    p.Title.ToLower().Contains(searchTerm) || p.Content.ToLower().Contains(searchTerm))
                    .OrderByDescending(p => p.TimePosted);
        }

        public IEnumerable<MonthCount> MonthPostCounts(string user)
        {
            var monthCounts = new List<MonthCount>();
            foreach (var post in posts)
            {
                var coveringMonth = monthCounts.FirstOrDefault(m => m.CoversDate(post.TimePosted));
                if (coveringMonth == null)
                    monthCounts.Add(new MonthCount { Year = post.TimePosted.Year, Month = post.TimePosted.Month, PostCount = 1 });
                else
                    coveringMonth.PostCount++;
            }
            return monthCounts;
        }

        public Post SinglePost(int id)
        {
            return posts.SingleOrDefault(p => p.Id.Equals(id));
        }

        public int AddOrEditPost(Post post)
        {
            if (post.Id.HasValue)
            {
                for (var i = 0; i < posts.Count; i++)
                {
                    if (posts[i].Id != post.Id) 
                        continue;
                    posts[i] = post;
                    return post.Id.Value;
                }
            }

            var newId = posts.Select(p => p.Id.Value).OrderBy(i => i).Last() + 1;
            post.Id = newId;
            posts.Add(post);
            return newId;
        }

        public void AddComment(Comment comment, int postId)
        {
            var post = posts.SingleOrDefault(p => p.Id.Equals(postId));
            var comments = post.Comments.ToList();
            comments.Add(comment);
            post.Comments = comments.ToArray();
        }

        private static readonly Comment comment0 = new Comment
            {
                Author = "Helios",
                Content = @"Sed hendrerit magna et ipsum sodales aliquet. Fusce fringilla vulputate urna a placerat. Aenean eleifend, urna sit amet semper tristique, nibh metus iaculis felis, a malesuada dolor quam eu turpis. Maecenas id ipsum at tellus lacinia imperdiet. Duis pulvinar nibh ut libero malesuada malesuada rhoncus metus mollis. Praesent dui turpis, gravida non dictum quis, aliquam vel diam. Vivamus cursus neque et diam porttitor auctor. Quisque eu metus nulla. Ut at mi a metus tempor congue non in ipsum. Proin metus odio, facilisis eu venenatis et, faucibus in mi. Cras tincidunt augue eget tellus suscipit luctus.",
                TimeMade = new DateTime(2011, 9, 3)
            };

        private static readonly Comment comment1 = new Comment
        {
            Author = "Helios",
            Content = @"In hac habitasse platea dictumst. Etiam venenatis accumsan sapien, vitae ultrices lacus elementum non. Sed in neque quam. Cras a libero neque. Pellentesque non felis ac est blandit tristique. Nullam ligula tellus, tincidunt quis vestibulum ut, eleifend non arcu. Pellentesque in metus elit, id ornare neque. Pellentesque interdum nunc nulla. Etiam eget metus varius lorem euismod vehicula a sed lorem. Aliquam placerat arcu libero. Curabitur blandit metus nec lacus porta egestas. Etiam quis ligula sem. Donec et turpis iaculis ante blandit auctor. Suspendisse potenti. Etiam nec justo massa. Duis fringilla, tortor et ultricies sagittis, est urna venenatis nisl, non venenatis est diam a mauris.",
            TimeMade = new DateTime(2011, 9, 6)
        };

        private static readonly Comment comment2 = new Comment
        {
            Author = "Helios",
            Content = @"Maecenas commodo erat dui, eu sagittis massa. Donec vulputate, mi sit amet iaculis venenatis, dui lorem faucibus velit, ut ultrices eros augue consectetur felis. Cras eu erat vitae enim tempus convallis sed eu est. Donec mattis scelerisque metus eget interdum. Aenean eu blandit arcu. Vivamus ac orci nisi, sed bibendum tellus. Proin dui orci, tincidunt eu porttitor vitae, ullamcorper in lectus.",
            TimeMade = new DateTime(2011, 9, 9)
        };

        private static Post post0 = new Post
        {
            Id = 0,
            Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            Author = "Icarus",
            Content = @"
<p>
Integer ut nibh eu eros sollicitudin elementum tincidunt vel ligula. Aliquam ac sodales sem. Etiam vel dui vitae orci lobortis semper. Integer id lectus nisl, posuere egestas enim. Suspendisse congue mollis nibh, molestie consectetur orci egestas id. Morbi varius, massa et venenatis eleifend, dui eros consectetur velit, et porta ipsum magna ac neque. Nam dapibus, sem vel sollicitudin aliquam, elit nulla lacinia arcu, in laoreet lorem massa in orci. Morbi laoreet vestibulum metus, nec cursus arcu vestibulum consectetur. Donec ac est ante, eget pretium dui. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Vivamus hendrerit, lectus et ullamcorper venenatis, nunc velit ornare nibh, in bibendum purus metus nec velit.
</p>
<p>
Etiam sodales nunc vel neque elementum condimentum. Proin sagittis leo eu urna blandit ut luctus magna facilisis. In est nunc, iaculis sed rutrum nec, tristique eget metus. Nam malesuada lacus nec nisl fringilla sed consectetur tellus posuere. Sed lacus ipsum, egestas nec fermentum sed, dapibus eu massa. Aliquam dapibus augue non purus molestie lobortis. Etiam magna arcu, interdum eu suscipit id, volutpat eget nibh. Pellentesque ut lacus at mi dignissim vehicula. In elementum ipsum tortor, ut ultricies magna. Proin pretium sollicitudin lorem, eu luctus orci imperdiet sit amet. Nulla vestibulum est ut urna tincidunt convallis. Morbi vitae ligula et erat hendrerit hendrerit ut id mauris. Vivamus sed commodo nisl. Proin dictum scelerisque magna, quis fringilla lorem commodo vel. Aenean quis tempor nulla.
</p>
<p>
Sed consectetur sodales iaculis. Aliquam a eros id nibh aliquet auctor quis semper eros. Nam ultrices est in magna fringilla vel ultrices lorem interdum. Vivamus vitae feugiat massa. Donec pretium bibendum nulla, non porta ligula mollis at. Integer leo ipsum, dictum in mollis sit amet, vestibulum vitae felis. Vivamus tincidunt tincidunt lobortis. Etiam ut eros in nisi gravida ullamcorper vel sollicitudin arcu. Aliquam ullamcorper luctus dignissim.
</p>
<p>
Suspendisse id eros lorem, nec venenatis turpis. Suspendisse scelerisque nibh nec lacus sollicitudin non feugiat dui placerat. Nunc tincidunt, diam non feugiat tincidunt, nisl mi dapibus mi, ut porttitor enim nisl sit amet felis. Vestibulum mattis sollicitudin mi. Quisque eget mi a urna fermentum placerat vel at ante. Proin rhoncus ligula in augue volutpat auctor. Proin molestie ultricies enim, id congue felis suscipit convallis. Aenean aliquam sodales porta. Donec dolor libero, rutrum rutrum tristique id, gravida non est. Pellentesque feugiat, urna at ornare blandit, tortor tortor ullamcorper est, id blandit leo diam nec nisl. In non urna turpis. Proin ut augue leo. Aenean magna mi, vehicula vel placerat ac, auctor sit amet metus. Mauris vitae nunc eget mauris feugiat facilisis eget et sem. Quisque id lectus porttitor sapien pellentesque eleifend.
</p>
<p>
Nunc felis felis, accumsan id suscipit sit amet, placerat vitae tortor. Ut semper mattis bibendum. Donec mauris felis, fermentum accumsan ultricies ut, fringilla at velit. Vivamus pulvinar mattis nisl sed convallis. Maecenas risus enim, blandit vitae tincidunt vel, convallis vel augue. Maecenas nunc lacus, pulvinar pretium tincidunt ac, dictum et nisl. Sed arcu nisi, feugiat auctor condimentum eget, fermentum sed odio. Nunc lacus purus, sollicitudin ac pretium vel, placerat sed metus. Nulla nec porta tortor. Vivamus elementum congue libero vel dignissim. Donec est lectus, iaculis sit amet facilisis ut, rutrum ut leo. Phasellus viverra hendrerit nunc vel lacinia. Mauris non tellus urna, et gravida turpis. Integer nec erat ornare dolor porta ornare a nec risus. In vitae tellus risus, eu convallis diam. Morbi fermentum elit a massa congue at molestie sapien auctor.
</p>",
            IsStory = false,
            TimePosted = new DateTime(2011, 4, 4)
        };

        private static Post post1 = new Post
        {
            Id = 1,
            Title = "Aliquam iaculis tincidunt purus, eget commodo tellus molestie id.",
            Author = "Daedalus",
            Content = @"
<p>
Nullam dignissim mollis suscipit. Mauris ut erat metus. Cras nunc turpis, consequat ultrices eleifend id, semper in orci. Curabitur sodales velit vitae nulla feugiat id ultricies metus tristique. Nam pulvinar accumsan purus ultrices porta. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Fusce a eros ac est elementum feugiat. Praesent eget mi eu quam facilisis suscipit. Proin convallis lacus quam, a vehicula erat. Aliquam vitae dolor erat. Proin libero nibh, consectetur ut tincidunt eu, consectetur sit amet mi. Aliquam erat volutpat. Aenean eros erat, ultricies a dapibus nec, auctor in nunc. Etiam tincidunt, ligula quis tempus pretium, quam lacus iaculis quam, at rhoncus nisi eros cursus eros.
</p>
<p>
Donec faucibus purus eu diam vulputate faucibus. Phasellus imperdiet pretium sem a fringilla. Fusce at justo sit amet lacus semper facilisis et in nisi. Morbi gravida euismod mauris sit amet vehicula. Nulla nibh augue, tincidunt a ultrices id, condimentum eu lacus. Phasellus nibh libero, adipiscing eget blandit eu, pretium quis nisl. Fusce porta mauris quis leo tincidunt et adipiscing purus imperdiet. Donec nec nibh risus. Sed malesuada nibh ut nulla fermentum a adipiscing sem vestibulum. Sed id libero odio.
</p>
<p>
Curabitur semper mi sed ipsum iaculis quis luctus ipsum interdum. Quisque non metus est, hendrerit varius sapien. Maecenas eget tortor enim, nec pretium nisi. Donec facilisis urna in lectus malesuada posuere. Vivamus egestas auctor erat non lobortis. Donec cursus mi sed mauris accumsan euismod. Mauris elementum bibendum dui eget blandit. Maecenas viverra accumsan erat non pharetra. Ut pulvinar blandit magna, et euismod urna vehicula ut. Aliquam erat volutpat. Nunc ac lectus neque.
</p>
<p>
Praesent tempor cursus ullamcorper. Morbi hendrerit ligula sed lacus congue non auctor magna auctor. Pellentesque at elit tortor, et malesuada eros. Nulla facilisi. Quisque vitae lacus enim, sed lobortis tellus. Curabitur sit amet turpis nisi. Donec eu enim sit amet quam mattis eleifend. Morbi porttitor ultricies cursus. Nam suscipit, felis a pretium euismod, nisi ipsum dignissim risus, et aliquam turpis augue non metus. Sed mauris ipsum, aliquet vitae porta vel, faucibus nec ligula. Aenean ut bibendum enim.
</p>
<p>
Quisque in ipsum est. Sed tristique malesuada hendrerit. Suspendisse tincidunt mauris porta diam ultricies ornare. Cras vehicula volutpat ornare. Aliquam facilisis ante a lacus vulputate non scelerisque velit ultrices. Sed feugiat elit ac leo mollis faucibus. Vestibulum scelerisque nisl quis tortor posuere egestas sed quis magna.
</p>",
            IsStory = false,
            TimePosted = new DateTime(2011, 4, 12),
            Comments = new[] { comment0 }
        };

        private static Post post2 = new Post
        {
            Id = 2,
            Title = "Pellentesque feugiat vulputate velit, at dignissim tellus lobortis vitae.",
            Author = "Icarus",
            Content = @"
<p>
Nullam feugiat tempus pretium. Nam a sagittis arcu. Praesent quis risus nec nisl vulputate iaculis ut ac purus. Sed in arcu lorem, imperdiet gravida lorem. Maecenas rhoncus sapien eget orci rhoncus vel gravida ligula placerat. Integer ullamcorper dui a ipsum pulvinar vel dignissim diam consectetur.
</p>
<p>
Morbi pretium, sem sit amet molestie semper, ipsum nisl posuere tellus, accumsan fermentum mauris nulla id nibh. Mauris neque tortor, suscipit in commodo eget, porttitor in libero. Nunc tincidunt erat vitae metus dapibus suscipit. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. In ac euismod risus. Aenean rhoncus semper pulvinar. Curabitur tristique ultricies dolor vestibulum commodo. Ut eu lectus nunc. Phasellus lacus felis, suscipit sit amet hendrerit a, lacinia interdum eros. Duis fermentum sapien id lectus dictum rutrum.
</p>
<p>
Suspendisse eu dui vitae sem vehicula fringilla. Duis venenatis ante et tortor facilisis vel condimentum magna sollicitudin. In lobortis tempus condimentum. Sed semper aliquet tellus quis hendrerit. Phasellus sollicitudin tortor nec elit tempor luctus. Ut posuere fringilla neque a tincidunt. Integer neque augue, accumsan eu commodo vitae, lacinia vel massa. In elit erat, faucibus a blandit vel, consequat in nulla. In aliquam vestibulum magna, sed condimentum risus ullamcorper at. Nullam accumsan sapien urna, ac bibendum libero. Nunc ac est felis, at scelerisque sapien.
</p>
<p>
Ut at nunc ipsum, sit amet tristique diam. Nunc sed tristique justo. Nullam non lacus mauris, vel porta purus. Nunc auctor tincidunt arcu in dictum. Mauris eget libero mattis risus sollicitudin egestas. Aenean eget magna quis sapien imperdiet scelerisque. Mauris vestibulum mollis dignissim. Ut sed ipsum at orci semper accumsan. Maecenas ultricies risus nec odio molestie lacinia.
</p>
<p>
Vestibulum velit lectus, vehicula eget varius eget, lacinia eu eros. Nullam non magna eros. Suspendisse ut enim est. Sed tristique lacus nec orci vulputate gravida. Nullam eu facilisis purus. Sed vehicula, dui in euismod auctor, ante augue tincidunt purus, non blandit mauris orci eget tellus. Donec nec metus sit amet erat imperdiet ultrices. Quisque blandit ullamcorper velit ut laoreet. Fusce egestas commodo nisi, pellentesque facilisis lacus blandit quis. Sed diam magna, bibendum vel aliquet sit amet, vehicula a tortor. Pellentesque vehicula tellus sit amet nulla accumsan ornare dignissim ipsum cursus. Aliquam turpis est, ornare vel feugiat quis, egestas sed elit. Sed at congue enim.
</p>",
            IsStory = true,
            TimePosted = new DateTime(2011, 5, 4),
            Comments = new[] { comment0, comment1 }
        };

        private static Post post3 = new Post
        {
            Id = 3,
            Title = "Etiam sed diam arcu, non mollis enim.",
            Author = "Daedalus",
            Content = @"
<p>
Donec tempor justo tortor, at scelerisque arcu. Pellentesque eget purus ipsum. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Quisque vitae ornare est. Proin molestie pulvinar metus a egestas. Morbi libero leo, laoreet quis feugiat ac, tempor vel tellus. Nullam nec odio nec sapien vehicula consequat. Nunc ligula est, commodo vitae aliquam eu, sollicitudin eget purus. In nec libero et dolor feugiat vestibulum sit amet vitae sem. Quisque congue, nunc non pharetra posuere, est quam faucibus libero, vel tempor libero arcu eu metus. Fusce eget urna lectus, non interdum nisi.
</p>
<p>
Vestibulum non leo sed ipsum eleifend congue. Aliquam erat volutpat. Nunc nec orci ipsum. Nullam posuere rhoncus nulla non semper. Proin purus diam, pulvinar sit amet vestibulum a, iaculis et leo. Nullam hendrerit, lectus id mattis pulvinar, dui leo ornare nibh, non rhoncus nunc nunc vel sem. Vestibulum augue nibh, aliquet a venenatis a, condimentum in nisi. Suspendisse hendrerit sem nec dui vestibulum venenatis. Ut vitae odio turpis, sagittis elementum mi.
</p>
<p>
In elit velit, ultrices sed ornare viverra, varius eget arcu. Etiam placerat sagittis mi quis accumsan. Donec nec auctor ante. Quisque ac urna vitae lectus viverra accumsan. Cras luctus, mi ac aliquet cursus, quam elit sodales massa, ut tempor lorem dolor et ante. Quisque vel risus nulla, vitae imperdiet metus. Nulla sit amet ante purus, sit amet pellentesque quam. In semper magna elementum magna lacinia euismod. In eu aliquet lacus. Etiam viverra cursus nulla, vel facilisis massa pulvinar sed. Nulla egestas pretium nibh, eu mattis magna varius non. Maecenas venenatis luctus ante vestibulum euismod. Aliquam nibh tellus, porta sed iaculis et, pretium nec ipsum.
</p>
<p>
Ut arcu urna, accumsan sollicitudin porttitor eget, dignissim vitae orci. Donec id lorem non nulla mollis bibendum. Cras magna est, tempus in suscipit consequat, ornare ut sapien. Integer ultricies risus ut arcu varius nec faucibus nibh accumsan. Etiam vel blandit diam. Mauris a erat eu odio vehicula tempus. Donec euismod, risus ac sagittis tristique, arcu orci eleifend urna, quis tincidunt dui lorem a est. Sed vestibulum metus semper ante luctus lobortis. Praesent egestas lacinia hendrerit. Nunc tempus nisi a nunc tincidunt gravida vel vel arcu. Mauris massa libero, eleifend rhoncus tristique a, venenatis vitae risus.
</p>
<p>
Aenean dictum augue et orci ultrices porta. Ut ante risus, consequat in placerat eget, eleifend semper magna. Morbi vehicula mauris vel ligula ultricies sit amet ultrices sem iaculis. Vivamus scelerisque eros semper arcu viverra feugiat. Aliquam eget scelerisque enim. Fusce dictum, lacus at posuere commodo, arcu lectus bibendum enim, nec tristique libero velit imperdiet metus. Vivamus ac dui vitae lacus luctus dapibus.
</p>",
            IsStory = false,
            TimePosted = new DateTime(2011, 5, 12),
            Comments = new[] { comment0, comment1, comment2 }
        };

        private static Post post4 = new Post
        {
            Id = 4,
            Title = "Proin sit amet ante orci, et facilisis purus.",
            Author = "Icarus",
            Content = @"
<p>
Sed at odio sit amet magna molestie pharetra quis et est. Duis sem odio, commodo nec facilisis id, ullamcorper in libero. Praesent at enim ac leo venenatis sagittis nec in erat. Nulla malesuada nisi vitae leo scelerisque a euismod est sagittis. Pellentesque pretium, turpis at tristique ornare, dui sem cursus purus, a fermentum dolor lorem ut justo. Duis porta suscipit quam quis feugiat. Nulla et erat lectus, accumsan scelerisque eros. Donec euismod nulla at enim lacinia id scelerisque felis accumsan. Sed pellentesque porttitor lectus, et hendrerit ligula rhoncus a. Fusce id eros enim, feugiat feugiat magna. Nunc non dui erat. Donec fermentum semper lorem, sit amet eleifend ante posuere sed.
</p>
<p>
Vestibulum imperdiet, diam eget malesuada consequat, lectus ligula tempus eros, ut venenatis metus eros vulputate libero. Vivamus dolor dui, pulvinar eu lacinia venenatis, ornare tincidunt tortor. Duis vel ornare felis. Aliquam venenatis egestas urna, non lacinia sem sodales a. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Pellentesque venenatis, massa quis varius imperdiet, velit nibh rutrum lectus, vitae rutrum ligula odio in erat. Morbi a nisl ante. Etiam non facilisis quam. Maecenas eget ante eu nibh malesuada lobortis sed in justo.
</p>
<p>
Donec et sagittis purus. Pellentesque viverra, lorem in rutrum rhoncus, neque arcu vestibulum tortor, in laoreet purus nisl eu lectus. Nulla venenatis varius placerat. Curabitur laoreet tellus vitae lectus iaculis dictum. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Maecenas commodo, diam eleifend scelerisque laoreet, leo lectus aliquam leo, vel interdum tortor sem in nulla. Vestibulum vehicula orci lorem. Aenean rutrum, ligula in porta gravida, enim libero bibendum mauris, eget pharetra arcu risus ac velit. Donec facilisis lorem ac lacus hendrerit tempus. Donec id tortor a magna aliquam lacinia in ut mi. Morbi eu mollis eros. Ut et pretium eros. Aenean adipiscing auctor orci, at mollis sapien semper vel. Aliquam erat volutpat. Vestibulum faucibus lectus id quam dictum accumsan. Aliquam erat volutpat.
</p>
<p>
Sed sit amet ipsum et magna rutrum congue. Etiam ut massa justo. Morbi dui libero, posuere et aliquet in, pharetra vitae nisl. In augue tortor, porta sed aliquam eu, molestie id felis. Etiam volutpat hendrerit nibh, sed dignissim arcu consequat vel. Duis sagittis faucibus nisi in egestas. Sed vitae ipsum diam. Sed at porta massa. Nullam enim sapien, scelerisque a tempus eu, tincidunt ut elit. Proin feugiat pulvinar fringilla. Donec mi sapien, malesuada vitae condimentum vel, pulvinar vitae dui. Vestibulum odio arcu, ornare at egestas id, cursus scelerisque urna. Morbi ligula diam, faucibus et ullamcorper quis, rutrum nec quam. Donec convallis, ante eu tincidunt dapibus, nulla mi mollis augue, a laoreet nunc massa in nisl. Sed ornare nisi a dolor dapibus id viverra enim faucibus.
</p>
<p>
Morbi aliquam pulvinar diam, ut mattis diam ullamcorper ut. Quisque iaculis lorem in sem ornare ultrices. Fusce rhoncus tincidunt mattis. Quisque aliquam aliquam molestie. Donec ante turpis, sollicitudin non adipiscing non, tristique vitae quam. Quisque nec nunc leo, in tincidunt felis. Vivamus sed nibh at mi facilisis faucibus sit amet in risus. Mauris id ultricies arcu. Aenean ipsum enim, sagittis in venenatis in, tempus auctor quam. Nam congue, risus id consectetur ullamcorper, ante metus elementum neque, ac pulvinar nunc nisl sed odio. Praesent nec orci nec diam pellentesque rutrum. Quisque hendrerit ipsum in urna tempor ornare.
</p>",
            IsStory = false,
            TimePosted = new DateTime(2011, 6, 4),
            Comments = new[] { comment0 }
        };

        private static Post post5 = new Post
        {
            Id = 5,
            Title = "Nam semper venenatis erat, eget fringilla quam rhoncus quis.",
            Author = "Daedalus",
            Content = @"
<p>
Nulla auctor justo quis orci mollis eu convallis erat mattis. Mauris ut lorem ut sapien congue dapibus. Aliquam sapien arcu, hendrerit a auctor sit amet, interdum ullamcorper augue. In quis lacus risus. Integer eget mattis nunc. Integer a tortor id massa ullamcorper consequat in vel dolor. Nam vulputate sem ac nisl gravida tincidunt. Nulla pulvinar suscipit odio vel venenatis. Suspendisse eros mauris, lobortis a vulputate at, dictum ut diam. Etiam lectus massa, ornare sit amet accumsan sit amet, imperdiet eget lorem. Cras interdum ipsum luctus tortor scelerisque tempor. Sed at nisl at nunc condimentum rhoncus. Proin vehicula iaculis tellus, at hendrerit massa pharetra in. Mauris at metus vitae enim ultrices ornare at vel odio.
</p>
<p>
Sed congue, mauris et bibendum vulputate, metus enim sodales nulla, id tempus elit velit non risus. Mauris non enim adipiscing purus adipiscing viverra id at nibh. Nam auctor ligula a orci pulvinar convallis. In hac habitasse platea dictumst. Duis lobortis risus nec tellus auctor ullamcorper. In hac habitasse platea dictumst. Nulla facilisi. Aenean interdum, nibh a aliquam semper, neque velit auctor tortor, eget elementum neque tortor vitae leo. Vivamus elementum ornare urna vel aliquam. Maecenas vitae nisi sed tellus lacinia accumsan. Etiam mi sapien, rutrum sit amet sodales a, posuere ac ante. Sed sodales vulputate pellentesque. Maecenas et dolor sit amet nulla bibendum posuere. Quisque vel condimentum augue. Pellentesque tincidunt malesuada ultricies.
</p>
<p>
Aenean viverra neque vel est viverra nec bibendum ligula gravida. Suspendisse vestibulum, orci quis commodo varius, sapien turpis porttitor velit, sit amet hendrerit est leo eu nunc. Etiam auctor condimentum libero eu eleifend. Nunc at molestie nisl. Phasellus placerat ligula eu metus aliquam sed accumsan lorem dictum. Praesent eu lorem justo. Quisque cursus lacus et ante tincidunt egestas. Nunc nibh tellus, malesuada a bibendum a, rutrum sit amet elit. Proin quam dolor, sagittis eu tincidunt ut, imperdiet non mauris. Mauris nibh enim, eleifend vel faucibus nec, eleifend id purus. Aenean a nisl id quam rhoncus tempus vel quis ante. Sed massa ipsum, condimentum eu aliquam et, blandit ut erat.
</p>
<p>
Duis lobortis interdum dui, ut feugiat dui convallis ac. Maecenas eleifend pulvinar eros, a condimentum purus dictum at. Proin dignissim, justo eget gravida aliquet, lorem ante porta nisi, in convallis sapien urna sit amet nisi. Suspendisse potenti. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Etiam hendrerit libero vel purus adipiscing ac tempor nulla tincidunt. Maecenas ornare pharetra nulla, vitae ultricies sem adipiscing sit amet. Sed vel turpis lectus. Integer a risus eget quam eleifend viverra at a orci. Aliquam erat volutpat. Nunc velit sapien, mollis et lobortis vitae, ultrices ac leo. Vestibulum adipiscing faucibus malesuada.
</p>
<p>
Ut et ipsum tempus turpis porta tempus. Integer arcu arcu, vulputate quis laoreet sit amet, molestie nec enim. Cras eu mauris lacus. Mauris sodales venenatis molestie. Curabitur convallis hendrerit lorem, interdum iaculis nibh varius id. Suspendisse id justo pharetra quam bibendum tempor. Quisque magna eros, placerat a malesuada nec, consequat et dui. Nulla pellentesque interdum tempus. Mauris pretium rutrum sagittis. Nunc semper dapibus lectus at tempor.
</p>",
            IsStory = true,
            TimePosted = new DateTime(2011, 6, 12),
            Comments = new[] { comment0, comment1 }
        };

        private static Post post6 = new Post
        {
            Id = 6,
            Title = "Nam in dui ac eros elementum bibendum.",
            Author = "Icarus",
            Content = @"
<p>
Nunc sed enim odio. Ut eros nibh, elementum rutrum ornare in, sagittis et tortor. Aenean semper, purus vel tristique sollicitudin, massa nulla hendrerit sapien, sit amet ullamcorper risus ipsum vitae ligula. Integer a lacus quis sem accumsan luctus. Cras ultricies aliquet diam, id viverra erat iaculis sed. Donec aliquam nibh eros, a fringilla nisl. Morbi id massa quam.
</p>
<p>
Phasellus scelerisque, justo vitae laoreet rhoncus, justo lacus condimentum est, a auctor neque neque eu erat. Quisque viverra placerat sem sagittis fermentum. Fusce suscipit quam id nisi imperdiet at laoreet mi auctor. Duis pulvinar erat eu lacus aliquet porta. Quisque vel quam ac est gravida dictum. Vivamus consectetur ultrices ante, nec luctus ipsum sollicitudin sed. Vivamus pellentesque, metus vitae ornare congue, est felis posuere diam, non ultricies nulla odio et lacus. Cras at nunc arcu.
</p>
<p>
Donec arcu nisi, auctor sed porta et, rhoncus in nisi. Phasellus tempus dolor at lacus tempus faucibus. Suspendisse aliquam dignissim turpis a congue. Quisque consectetur vestibulum metus at facilisis. Proin at augue et nulla aliquam vestibulum sit amet ut neque. Integer vitae neque eu ipsum sollicitudin adipiscing. Sed in eros quis nulla hendrerit tincidunt. Donec consequat laoreet erat sed auctor. Quisque augue libero, imperdiet sit amet suscipit et, mollis vitae nisi. Nullam elementum dignissim nisi, quis rhoncus velit sollicitudin et.
</p>
<p>
In consectetur pulvinar orci. Nunc lacinia tincidunt sem, eget convallis sem egestas nec. Nunc elementum, libero sed posuere convallis, leo urna porta felis, eget venenatis massa metus vel dui. Sed bibendum lectus in dolor pretium blandit. Pellentesque tempus varius pulvinar. Aenean justo sapien, luctus nec pretium sed, auctor sed diam. Curabitur consectetur augue vitae ligula pretium pellentesque fermentum sem vehicula. Vestibulum sed tincidunt tellus. Vivamus ut nisi arcu. Cras risus justo, imperdiet sit amet tempus quis, venenatis lobortis diam. Maecenas quis dolor at purus viverra consectetur. Phasellus non pulvinar justo. Phasellus pretium, nunc a lacinia hendrerit, magna libero varius mauris, id sagittis nisi purus a risus.
</p>
<p>
Etiam eget mi quis ante aliquet condimentum mollis in est. Duis ac odio ipsum. Nulla facilisi. Aenean eget dolor purus. Maecenas at lectus lectus. Mauris risus nisl, suscipit at placerat ac, mattis a massa. Donec sed odio sed odio viverra ultricies dictum in est. Cras eget elit ligula, sed sagittis quam. Suspendisse varius dictum mi, eu feugiat leo auctor non. Ut et mauris in ipsum laoreet ullamcorper a sit amet enim. Fusce lorem nisl, tristique at lobortis eu, scelerisque sit amet eros. Nulla facilisi.
</p>",
            IsStory = false,
            TimePosted = new DateTime(2011, 7, 4),
            Comments = new[] { comment0, comment1, comment2 }
        };

        private static Post post7 = new Post
        {
            Id = 7,
            Title = "Aliquam elementum orci ipsum, in vestibulum lectus.",
            Author = "Daedalus",
            Content = @"
<p>
Donec molestie nisl a nulla pharetra vel varius diam auctor. In varius justo eu nunc molestie viverra sit amet et purus. Donec nec dui nibh, iaculis cursus tortor. Vestibulum et ante quam. Sed eleifend augue vel dui rhoncus fermentum. Sed sed odio id leo tempus scelerisque. Etiam suscipit scelerisque elit, et scelerisque mauris mattis id. Morbi ut risus pharetra tortor posuere suscipit.
</p>
<p>
Sed ut mattis tellus. Integer dictum auctor velit vitae venenatis. Mauris dui massa, molestie vitae aliquet ac, commodo et nunc. Morbi quam elit, feugiat a posuere sed, iaculis vel purus. Sed tempus nunc vitae risus condimentum consequat. In hac habitasse platea dictumst. Vestibulum quis facilisis lacus. Donec vitae augue purus. Vivamus fermentum mi arcu. Cras vitae nulla a nisi convallis tincidunt. Curabitur in lectus ligula, rutrum rutrum diam. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. In ac ligula felis.
</p>
<p>
Duis vestibulum dictum nibh at consequat. Aliquam commodo, velit ut pretium ornare, nisi diam blandit nibh, non tempus sem nunc in ligula. Vivamus molestie felis quis lacus molestie semper. Etiam accumsan hendrerit facilisis. Sed luctus turpis non ante suscipit ullamcorper et sit amet enim. Mauris commodo dui non risus molestie semper sagittis sem luctus. Cras imperdiet hendrerit nisl et consectetur. Nunc eget odio augue. Nunc sodales ipsum vitae enim rhoncus vel cursus dolor viverra. Vivamus arcu libero, pharetra non consectetur a, tempus in nisl. Nunc at justo a orci blandit volutpat.
</p>
<p>
Aenean suscipit tellus sit amet lacus dictum venenatis. Praesent vehicula dolor dictum augue tempor ut suscipit neque vehicula. Mauris interdum eleifend eros ut rutrum. Duis malesuada quam at nisi egestas ultrices. Vivamus vitae lectus nibh. Morbi aliquet aliquam velit, nec bibendum massa pharetra quis. Donec nec placerat nisl. Aliquam id ipsum nec arcu cursus convallis at a nunc.
</p>
<p>
Aliquam aliquam enim quis neque tincidunt ullamcorper. Sed scelerisque felis at nunc faucibus consectetur. Proin et libero ornare dolor tempor auctor non non enim. Suspendisse et quam purus. Cras mollis semper massa vel porta. Maecenas est justo, mattis eget congue ac, iaculis sit amet nisi. Nunc rhoncus lobortis massa, vitae fermentum lacus semper vel. Integer a quam ut nisl rutrum sagittis volutpat ac lorem. Praesent sit amet lacus dui. Nam id libero mauris, non posuere est. Nulla mauris mi, auctor vel accumsan id, lobortis sed metus. Nam sagittis erat non urna ultrices elementum. Proin ullamcorper blandit risus eu auctor. Vestibulum tempus molestie mollis. Curabitur neque velit, convallis vitae malesuada at, imperdiet sodales enim.
</p>",
            IsStory = false,
            TimePosted = new DateTime(2011, 7, 12),
            Comments = new[] { comment0 }
        };

        private static Post post8 = new Post
        {
            Id = 8,
            Title = "Sed commodo sollicitudin risus, varius convallis massa tempor tempus.",
            Author = "Icarus",
            Content = @"
<p>
In laoreet tortor luctus purus convallis vel dapibus tortor venenatis. Phasellus pharetra, ligula et varius convallis, risus ligula ullamcorper augue, a lacinia nulla lectus sit amet urna. Nullam eget velit ante, vitae tempor magna. Proin a sapien dui. Ut vitae metus est. Ut interdum pharetra bibendum. Aliquam erat volutpat.
</p>
<p>
Donec leo leo, luctus eget sodales eget, varius quis dui. Aliquam turpis lectus, pellentesque id tincidunt id, iaculis sit amet odio. Vestibulum posuere quam ac metus imperdiet vulputate. Aliquam eu ipsum mi. Praesent interdum enim porttitor justo pretium mollis. Donec risus sapien, sodales a dignissim vel, aliquam ac nisi. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Phasellus bibendum mattis orci eu sodales. Curabitur tempor varius erat nec pellentesque. Nulla sollicitudin gravida lorem et tristique. Suspendisse potenti.
</p>
<p>
Nulla facilisi. Quisque ligula nulla, luctus molestie pretium id, pulvinar a augue. Vivamus a nisl velit. Aenean at erat in eros venenatis ultrices. Ut a sollicitudin justo. Nulla facilisi. Ut sed orci ut est vulputate rhoncus in non sapien. Aenean et tellus quam. Maecenas vel nisl turpis. Ut metus dolor, suscipit ut tristique nec, auctor id orci. Proin sapien quam, fringilla hendrerit convallis in, volutpat at orci. Nulla at fermentum mauris. Nunc et enim tellus.
</p>
<p>
Mauris fringilla gravida neque quis malesuada. Nam scelerisque laoreet augue a lobortis. Vestibulum semper lacinia nibh sit amet scelerisque. Duis tincidunt tristique nulla a aliquet. Integer orci augue, pretium id blandit et, varius at dui. Aliquam mollis semper massa, vel gravida elit congue non. Pellentesque arcu augue, sagittis vel egestas at, mattis elementum libero. Nullam lobortis volutpat orci eu dapibus. Vestibulum ac pulvinar nisl. Vestibulum eros magna, dapibus ut tincidunt eu, commodo id tortor. Praesent et massa felis. Mauris convallis sollicitudin dui, ut dignissim lacus iaculis vitae.
</p>
<p>
Phasellus in interdum quam. Integer tempor interdum sem, non consectetur turpis laoreet aliquam. Suspendisse potenti. Duis id enim diam. Integer sit amet nisi arcu. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Aenean at metus at risus volutpat rutrum sed non est. Fusce aliquam, metus eget elementum laoreet, metus nisi pellentesque quam, sollicitudin pretium lorem lacus non tortor. Aenean quis pellentesque dui. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Etiam nulla sem, faucibus id convallis ac, tempus quis erat. Praesent at pretium sapien. Nulla eu turpis magna, et facilisis odio. Sed adipiscing sodales augue nec ultricies. Donec congue sollicitudin arcu, scelerisque accumsan nisl tempus sed.
</p>",
            IsStory = true,
            TimePosted = new DateTime(2011, 8, 4),
            Comments = new[] { comment0, comment1 }
        };

        private static Post post9 = new Post
        {
            Id = 9,
            Title = "Curabitur vehicula tellus id dolor rutrum id lobortis enim interdum.",
            Author = "Daedalus",
            Content = @"
<p>
Cras lorem quam, faucibus ut congue vel, tincidunt sed tellus. In iaculis, enim nec tristique mollis, felis odio rhoncus eros, sit amet dictum est diam et felis. Maecenas eget metus ac turpis fermentum malesuada. Nullam a felis non mauris faucibus blandit. Morbi tristique iaculis ante ac sollicitudin. Aenean accumsan justo ut purus porttitor mattis. Proin vehicula fermentum felis, eget blandit felis consequat sed. Quisque porttitor accumsan nibh ut elementum. Sed eu nisl suscipit sapien blandit commodo. Morbi non molestie nibh. Curabitur bibendum, leo sed ultricies fringilla, urna mauris vestibulum ante, quis tincidunt odio lectus ac lectus. Ut metus diam, faucibus eu luctus a, elementum eu lectus. Quisque laoreet, turpis nec feugiat pulvinar, nisl purus iaculis justo, in varius nulla tellus quis erat. Etiam a lorem sed felis dignissim fermentum.
</p>
<p>
Maecenas vel neque urna, ac tristique nisi. Fusce ac placerat massa. Sed id ante arcu. Quisque accumsan tempus est, non malesuada sapien eleifend mollis. Donec quis velit vel odio accumsan laoreet sed ut magna. Duis in augue sit amet felis cursus pellentesque nec iaculis tellus. Donec fermentum est nec mi egestas vitae feugiat nibh molestie. Phasellus luctus, magna at dignissim volutpat, leo elit cursus dui, eu pharetra tellus nisi quis massa. Suspendisse feugiat nulla eget nisi sollicitudin sollicitudin. Quisque porttitor adipiscing lorem sed iaculis. Ut quam velit, vehicula et dignissim pulvinar, posuere et sapien. Vivamus id ligula sit amet ante sollicitudin pretium. Proin vel pellentesque leo. Sed velit ipsum, bibendum eget placerat eu, eleifend sit amet dolor.
</p>
<p>
Vestibulum lacus eros, varius vitae bibendum in, auctor at mauris. Nulla eget metus ac lacus malesuada ultricies sed nec magna. Pellentesque ultricies sollicitudin lorem, sit amet viverra turpis rutrum in. Nullam sagittis sodales neque et tempus. In condimentum bibendum eros vel lobortis. Proin eget lacus et quam pellentesque laoreet. Aliquam quis nisl turpis. Aenean ut lorem ligula, sit amet adipiscing enim.
</p>
<p>
In lobortis dictum eros, et elementum ante condimentum non. Curabitur interdum tortor a enim accumsan ut molestie justo ornare. Donec tempus erat ac est pellentesque sit amet placerat massa dictum. In pulvinar quam in est commodo vulputate. Sed eu magna erat. Etiam convallis ultrices ligula, eget tempor nisl auctor ac. Aenean a purus est, id scelerisque ipsum. Morbi urna ligula, vestibulum id convallis elementum, sagittis non lacus. Suspendisse potenti. Aliquam in justo lacus. Sed sed mauris leo, eu posuere felis. Nullam posuere varius elit, sed rhoncus risus porta eget.
</p>
<p>
Nulla congue ultrices lacinia. Nam dictum, ligula id suscipit imperdiet, nisi ligula fringilla justo, eget porttitor tellus lacus vel purus. Vestibulum fringilla dignissim dolor at rhoncus. Praesent vulputate elementum diam a imperdiet. Pellentesque id justo eu felis fermentum suscipit vel et felis. Maecenas fermentum libero id lectus elementum vitae tristique quam facilisis. Etiam sit amet arcu non nisi dictum lacinia quis id tortor. Etiam facilisis imperdiet metus sit amet elementum. Nam in tincidunt libero.
</p>",
            IsStory = false,
            TimePosted = new DateTime(2011, 8, 12),
            Comments = new[] { comment0, comment1, comment2 }
        };
    }
}
