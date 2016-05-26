using System;
using System.Collections.Generic;
using System.Linq;
using GrislyGrotto.Framework.Data.Primitives;

namespace GrislyGrotto.Framework.Data.Moqs
{
    public class MoqPostData : IPostData
    {
        private readonly List<Post> moqPosts;

        public MoqPostData()
        {
            moqPosts = new List<Post>
                       {
                           new Post
                           {
                               ID = 1,
                               Username = "Peter",
                               TimePosted = new DateTime(2010, 12, 30),
                               Title = "An offer",
                               Content = @"I got a letter in the post inviting me to sell my Telecom shares. It was very detailled, advising how much money I would get.<div>In the bulk of the letter it advised the price. 30s on the internet showed that the price being offered was 65% of the current market price.</div><div>But the offer is only available for about 10 days.</div><div>Which means I have 10 days to decide whether to gift this stranger $ 4,000, who went through all the effort of looking me up on the share registry, printing off a pro forma letter, and then sending it to me the post.</div><div>Sometimes it is hard to make up my mind on these issues.</div><div>I have to admit though I do not know whether to be insulted because he thought I was that stupid, angry that he tried to rip me off or cynical that anyone who actually loses money deserves to do so for being that stupid.</div><div>Maybe some poor old grandma will be fleeced and then their hulking grandson will stomp on him.</div><div>The guy who sent it is apparently a discharged bankrupt and has been targeting a large number of shareholders. Because he emphasises the amount of money, he is trying to catch people who may be short of money at christmas and do not read the detail. It is not illegal - there is nothing wrong in offerring a below market price and he even advises people to get financial advice before signing (although I suspect the aim is to make people trust him enough not to do so).</div><div>Ah Christmas, the time of good will and love for all people. This guy is a scumbag though because, regardless of the stupidity of his customers, it is only about his greed and not the owners of the shares. He is simply targeting vulnerable people in a crude spam approach.</div>",
                               IsStory = false
                           },
                           new Post
                           {
                               ID = 2,
                               Username = "Christopher",
                               TimePosted = new DateTime(2010, 6, 30),
                               Title = "Persistance and Rain",
                               Content = @"<p>Its raining today. Lightly, with a fine white mist obsucring vision in the distance. The water beyond a few hundred meters of the harbor is not visible, and even the top of some of the tallest buildings are shrouded by the clouds. This kind of weather is my ideal by far, only surpassed by more extreme rain. I know that some people find this odd, but to be honest I find a bright sunny day where you are NOT drinking to be dry, dead. In a wet world everything is so vibrant and alive, as well as being slow enough that it can be taken in in all its glorius detail. I always feel most serene when it rains.
</p><p>
This helps, because I just spent $610 on an eye exam and a new pair of glasses. Im fine about it, really. I thought it would be around $400, like my last pair, but no matter. The new set look good I think, and it will be nice to be able to see properly when needed and with ease. So yeah, awesome.
</p><p><strong>Before I get into a WoW rant, I thought I better mention that my article on WPF/XNA is now up on <a href=""http://www.geekzone.co.nz/vs2008/4777"">Geekzone</a>, presently sans my bio. My lovely mug should be up there shortly however. Alot of respect at work right now, which would be good except that I forgot all about the annual Provoke birthday party on friday, a day Im scheduled to head up to Taupo (gonna have to work something out, and its gonna cost me :( ).</strong></p><p>
Got another character to level 70 in WoW over the weekend, my trusty Drenai Warrior Teroen. Once I hit the magic number my enthusiasm immediately flagged. Basically at 70 you can do two things: You can get epics through instancing, or through PvP. My brothers are big on PvP (even if their arena ratings are a joke) and my friends in Palmy are big on instancing (even if they cant PvP to save their lives). I dont like instancing that much, despite doing a Kara run last night that scored me two epic one handed weapons and an epic thrown. Take Kara for example, which is one of the supreme instances to run. People who think theyre elite run Kara quite often, scheduling weekly or biweekly Kara runs. They talk like theyre tough, but ultimately running an instance takes as much skill as fighting a monster. You need some group knowledge, but its static, theres no realy dynamic thinking like you need when fighting head to head against other players. Instance running to PvP is like Comp Stomping to FFA's, in RTS terminology.
</p><p>
However now Im 70, the slog to get decent PvP gear is a long one. YOu need over 16,000 honour points to buy a single piece of epic arena gear, or at least several weeks/months of actual arena with a good team. This is hard enough, without the fact that the Alliance are a a bunch of little girls. I find it rare that Alliance wins a battleground with random groups, and organized groups have been changed so they only fight other such groups which makes organised PvP a less appealing alternative. So what to do? Usually my kneejerk reaction is to start another character but this time, dammit, I have a level 70 Warrior, perhaps the hardest and meanest class in the game. Covered in inch thick plate and wielding a massive axe he would be an armoured jugganaught, so its going to be hard to top that. I need to not blame the class and just get into the thick of it.
</p><p>
The problem is, of course, WoW is for the weak. There are sooo many better games out there, whose only flaw is that theyre not persistant. If there was a decent XBox Live service for Windows, where all the games you played were stood against your unique score, with your achievements listed like badges of honour, singleplayer would be much more viable. You may have an epic one handed sword from a Kara drop, but I have Hitman Bloodmoney completed on Professional Silent Assassin. How badass would that be? Clearly Microsoft needs to assume control of the PC gaming industry.
</p><p>
Chris</p>",
                               IsStory = true
                           },
                           new Post
                           {
                               ID = 3,
                               Username = "Peter",
                               TimePosted = new DateTime(2010, 4, 30),
                               Title = "I have to work with some people",
                               Content = @"<p>Today's <a title=""overkill"" href=""http://http://www.stuff.co.nz/technology/digital-living/4474911/UFB-could-broadcast-500-TV-channels"" target="""">article </a>&nbsp;talks about thye wonderful world of UFB where you can watch 500 TV channels by transmitting them over an RF overlay on the PON (Passive Optical Network) technology used to provide service to residential customers.</p>
<p>I have to confess I am a little biased about this technology. It is like buying a top of the line PC with all the latest features and then running solitaire (or in Chris's parlance, WoW).</p>
<p>RF Overlay works by using the fibre as a transmission medium. Much like DSL it frequency shifts the Video channels, which are broadcast over an analogue spectrum (probably digitally encoded), over the fibre between the exchange and the customer site. It requires a device to insert the frequency in the exchange (one per 32 lines) and then one in each house to receive it.</p>
<p>Ignoring the anachronistic technology, the main difficulties with this service is:</p>
<ul>
<li>
<div>The cost of the exchange device, and then the cost of each household device. This is incremental to broadband and typically costs ~ $ 10 per month.</div></li>
<li>
<div>The service only works over the last 20k, so you need to get the video signals to the exchanges first.<br>You could use satellite of course, but then you would not need RF overlay - people could just use a satellite.<br>You could transport analogue channels across the core. This is NOT cheap.<br>You could use IP multicast to reduce the cost of channels across the core. However why not then use this to deliver to end users?</div></li></ul>
<p>The bandwidth available on RF overlay is significantly higher than satellite. It would be a viable alternative if you (a) had demand for a lot more channels AND (b) it was viable to turn off the satellite - but without 100% penetration of fibre the latter is unlikely.</p>
<p>Of course the reality is that by the time fibre becomes sufficiently widespread for this to be economical, the demand for broadcast TV is probably almost zero - live events mainly.</p>
<p>And would it not be better to use the new technology as intended? Implement IP multicast - which works seamlessly over the DSL network as well as fibre - subject to line speed of course (you might need to reduce video quality on lower lines although codec improvements means it might be possible to expand coverage). Multicast has limits - it can take 5-10 seconds to change channels for example, but it works seamlessly over the technology.</p>
<p>This smacks to me of people (hopefully deliberately, it would be scary if they genuinely thought this) trying to raise the profile of UFB and get people interested.</p>
<p>However not sure why I would, as a taxpayer, be willing to subsidise a service so people could get a lot more tv channels. Is this truly how we are going to drive NZ towards a more valuable economy?</p>",
                               IsStory = true
                           },
                           new Post
                           {
                               ID = 4,
                               Username = "Christopher",
                               TimePosted = new DateTime(2010, 4, 20),
                               Title = "Christopher Denton",
                               Content = @"Watched Tron Legacy last night. Was pretty awesome :) Will need a few days to let it all sink in, but I definitely enjoyed it. Kind of thinking I should go see it again, even though admittedly its story was a touch weak and the computer concepts within were kind of ridiculous (just like the original :) Graphics and set pieces were amazing, and I liked the characters and the action. All you need from a nice summer blockbuster :)<div><br></div><div>I also finished Deus Ex Invisible War last night. My opinion on this game has improved markedly, now that I have a better handle on what I don't like about it relative to the original. One thing about it almost reminds me of Ayn Rand: a world recovering from a global disaster, with nations and peoples only barely carrying on while a small group of people have different philosophies on how to rule them. Its definitely quite dark - the world has already fallen. And of course its shorter and quite a bit less complicated as the original. Your character kind of feels more powerful though :) I got a&nbsp;bio-modification&nbsp;that allowed me to become cloaked from organic and mechanical entities for an extensive period of time, only being dropped when I fire my guns, but being able to be instantly&nbsp;re-assumed. As a result, some battles simply resulted in me dropping out of cloak for a split second to fire then dissaparing again, leaving even my most powerful enemies unable to damage me.</div><div><br></div><div>Looking forward to Tron 3 and Deus Ex Revolution :)</div>",
                               IsStory = false
                           },
                           new Post
                           {
                               ID = 5,
                               Username = "Peter",
                               TimePosted = new DateTime(2011, 1, 5),
                               Title = "Tis' the season",
                               Content = @"It's Thursday today. I know this because the rubbish is being collected even as i type (well maybe a min ago).<div>It's also mum's birthday. The girls are taking her out and I will be entertaining dad. Unsure what yet, probably nothing spectacular, maybe a meal in the foodcourt or something.</div><div>This holiday is not progressing rapidly or slowly. Sort of the plan till after Christmas. I will be visiting Mum and Dad in the new year, mainly to set up their iPod touch I got them for christmas. I hope that works out ok.</div><div>Quite a quiet Christmas this year. Bernie, Barry and Antoinette seem to have been focussed on rebuilding the inside of Bernadette's new house (which I have yet to be seen). Seems a strange time of the year to do this, especially as Bernadette could have moved in and then done it from the inside.</div><div>I bought a Kinnect yesterday but not sure exactly how I will set it up. Perhaps a project for this afternoon or tomorrow. As a concept though it looks good and I played it at Judy's, entertaining and fun.</div>",
                               IsStory = false
                           },
                           new Post
                           {
                               ID = 6,
                               Username = "Christopher",
                               TimePosted = new DateTime(2009, 8, 1),
                               Title = "Just wire it to my spine!",
                               Content = @"Just finished a long bath. Tell me, is it pushing the boundaries of Metrosexuality or Eccentricity or whatever my love for baths, cigars, vests and waistcoats (of which I got a new one today! Yay!) is classified as, when I have a bath filled with tiny bits of glitter? Probably. It wasn't intensional anyway, just one of the bath bombs I had that smelled nice actually happened to be a dispersal device for glitter armageddon. The experience was...novel.&nbsp;<div><br></div><div>Anyway, took a half day today due to lack of work, tiredness and general 'hey, i can totally take a half day for no real reason'-ness. Removed both Alpha Protocol and New Vegas from my PC so they won't be distractions, and I am trying to enjoy Just Cause 2. So far, I am finding it hard: rampant, uncontrolled&nbsp;destruction&nbsp;in a huge sandbox world is not really for me; it appeals a little, don't get me wrong, like hijacking a plane then flying it into a silo, parachuting out at the last minute to grab a mounted gun, detach it and hoe down a squad of soldiers frantically trying to take me out. But thats short term fun; I grow tired of it quickly. So instead I have been exploring on the net.</div><div><br></div><div>There was this article where this woman has been enhancing her self with wetware&nbsp;modifications, for example &nbsp;implanting magnetically responsive plates underneath her fingertips so she can sense and feel the strength of electromagnetic fields. That appeals to me on many levels. I kind of want to investigate getting a RFID chip implanted in my hands or something, so I can unlock my house, car and computer systems with just a wave of my hand. That would be pretty cool, the first steps to&nbsp;trans-humanism&nbsp;that don't simply involve a dependency on mechanical devices. A wireless storage system, say, 100 gigs or so, surgically implanted in my abdomen and powered by bioelectric energy would be pretty awesome too, though its possible the technology for that might not yet be readily available :)</div><div><br></div><div>Alongside the cigars and waistcoat I bought today I also bought a collection of&nbsp;Nietzsche&nbsp;writings. I've always wanted to read him, and now thanks to Ayn Rand I have a hankering for some philosophy. In the past I have always reasoned that borrowing someone&nbsp;else's&nbsp;philosophy is cheating, and thats why I haven't read as much as I should have in this area. After all, say you read Ayn Rand and then become a zealous Randian as a result, isn't that similar to reading a religious text and joining a religion? It smells of people not thinking for themselves, just accepting the perspective of someone wiser. I know this isn't all that true, that philosophy is something that attempts to convince you, rather than demand acceptance based on faith, but still. However, since AS and The Fountainhead, I have found that the philosophy I absorbed has simply become a set of tools to use to evaluate the world, both my actions and those of others. This is valuable, and I am hoping I can expand this newly available arsenal with&nbsp;Nietzsche's writings. After all, he is one of the more famous and more heavily debated philosophers. Hmm. We will have to see.</div>",
                               IsStory = false
                           },
                           new Post
                           {
                               ID = 7,
                               Username = "Peter",
                               TimePosted = new DateTime(2007, 11, 1),
                               Title = "A tale of two stories",
                               Content = @"<p>Dick Smith of Australia has lamented the greed Aussie businessmen who refused to give away 20% of their fortune this christmas to the needy. They could easily afford this.</p>
<p>This suggests to me that Dick Smith is a total dork, not only for expecting them to give it away but for even suggesting it. This is like the US billionaires giving away the bulk of their fortune when they die - I suspect Dick Smith saw him getting similar kudos for implementing a scheme in Australia.</p>
<p>First off Australia has an extensive welfare system and it could be argued that these rich fat cats were key contributors, if not directly, then indirectly through the salaries of the people they employ, the revenue of the companies and even just the increase in wealth in Australia as a whole.</p>
<p>Secondly it assumes that the only way for these people to contribute is however Dick Smith thinks they should contribute. Why should he be considered the judge of these things.</p>
<p>The second story is about a burglar who stole $ 28k worth of stuff from a number of homes (presuming the actual total was much higher and he did not get caught for all of his crimes) including a commonwealth gold medal he tried to get melted down. This person actually comes from a good home with a supportive family but was, in the judge's words, too lazy to actually earn a living. He had access to a family business and could have worked for a living but chose not to.</p>
<p>The gold medal was earned by someone through hard effort and I personally would consider that they, and in fact the whole country, would have a strong emotional attachment to this. Something the burglar discarded in his callous behaviour.</p>
<p>So with this callous behaviour it was decided to sentence him to 12 months home detention. WTF.</p>
<p>Why not sentence him to three years? Apparently he showed genuine remorse but I would find it incredibly difficult to believe in&nbsp;someone who showed remorse only after they were caught.</p>
<p>He burgled one house twice, three months after the first in order to be able to steal the stuff that replaced the first offence. His behaviour was, according to the judge, 'brazen, predatory and planned'.</p>
<p>Let him rot in jail. Ok this is partly about revenge but someone who is like this obviously did not care about others. How is this going to control his behaviour</p>",
                               IsStory = false
                           },
                           new Post
                           {
                               ID = 8,
                               Username = "Christopher",
                               TimePosted = new DateTime(2010, 3, 5),
                               Title = "The Christmashead",
                               Content = @"I am about to go out to hopefully buy the last of my Christmas presents. I have been out both yesterday and the day before, although oddly each time I only bought a single gift, plus a single item for myself. So hard to buy gifts, even with so few people to buy for, simply because I want to get them something they will like while at the same time buying things that I can easily transport on foot to and from my house. Bah. Tomorrow are the standard Christmas celebrations with my family, and on boxing day I think I am doing something with friends from work. New Years as well for that matter - should be exciting, as I've never had a New Years party before with anyone other than family.<div><br></div><div>Other than that, still WoWing away, watching a few movies. Presently downloading Fallout New Vegas off steam, that I bought in their annual epic Christmas sale for a song. All to the good. Might get drunk later :) Oh, and I am reading through The Fountainhead by Ayn Rand, of which I read half last night in one long session. This book is amazing, if a lot softer and shorter than Atlas Shrugged.</div>",
                               IsStory = false
                           },
                           new Post
                           {
                               ID = 9,
                               Username = "Peter",
                               TimePosted = new DateTime(2006, 6, 21),
                               Title = "The end is nigh",
                               Content = @"<p>2 days of work left and then a 3 week holiday (24 days actually, including 4 weekends, 4 public holidays and 2 company holidays.</p>
<p>I have some ideas for what I want to achieve during this break however last time I achieved little of that. The main goal is to relax and forget about work.</p>
<p>I do have some plans regarding the house, getting rid of junk and generally tidying up. I have started this in parts but still have the kitchen to complete in the main living area, making some small changes to the computer area (moving printer to wall to make more use of space). Also continuing the consolidation of the DVD collection.</p>
<p>Today I have two meetings, 9-12 and 2-4, which might be a little heav y going. However interesting stuff. I also have one outstanding action point to address, a technical review of a document, and some minor christmas shopping. After work I intend to do a 'christmas shop' which no doubt will be insane and then I have to make sure the car is gassed up etc.</p>
<p>Then I can forget about all of this until the visa bills arrive.<br></p>",
                               IsStory = true
                           },
                           new Post
                           {
                               ID = 10,
                               Username = "Christopher",
                               TimePosted = new DateTime(2010, 6, 24),
                               Title = "Two Point One",
                               Content = @"I played and beat Alpha Protocol over the last few days. The game got mixed reviews, and playing it I can understand why, although overall I thoroughly enjoyed it. Basically its Alias the TV series as a computer game: You are a covert ops agent, who can use gadgets, combat and stealth (including both silenced pistols and hand to hand combat) to finish missions. Combined with a very neat real time conversation system, where you talk with people and as they are talking you can pick the position to take next (like suave, professional or angry for example) and the conversation proceeds without interruption. The Alpha Protocol organisation operates completely off the radar, and soon enough you are cut off completely, officially a rogue agent travelling to Rome, Moscow, Taiwan and Saudi Arabia on a mission to uncover corruption and conspiracies worldwide. Pretty fun. I took the stealth approach, with one of the cooler missions accomplished where I elected to sneak around and disable every guard using silent combat take downs, either non lethal or lethal depending on the affiliation of each individual. The camera issues and instability obviously cost it quite a bit however, and since they elected to go with the checkpoint system rather than letting you save, this means that sometimes trying to do a mission perfectly can be totally impossible, since the slightest mistake is either non recoverable, or requires considerable repetition from the last checkpoint. Anyways, cool.<div><br></div><div>Next, Just Cause 2. I should note that over the holidays my speakers started crackling more and more, which became super annoying. I went and bought replacements, only to find out the actual cause was that my drivers for my sound card needed updating /facepalm. There should be an app that googles literally everything I do, so my decisions are always made with the most information available :) (actually thats a pretty fricken cool idea). But, whats interesting is that I bought 2.1 instead of 5.1 speakers (two speakers and sub woofer, missing rear speakers and a center speaker that 5.1 has), but of a higher quality than my old set, from the same people: Logitech. The sound quality is the same as far as I can tell, though the models are nicer; ultimately 2.1 is perfectly adequate for my needs, and now my desk is a good deal less cluttered which is ultimately resulting in a more peaceful frame of mind. Nice.</div>",
                               IsStory = false
                           }
                       };
        }

        public IEnumerable<Post> LatestPosts(int count, string user = null)
        {
            if (!string.IsNullOrEmpty(user))
                return moqPosts
                    .Where(p => p.Username.Equals(user))
                    .OrderByDescending(p => p.TimePosted)
                    .Take(count);

            return moqPosts
                .OrderByDescending(p => p.TimePosted)
                .Take(count);
        }

        public IEnumerable<Post> PostsByStatus(string status, string user = null)
        {
            if (!string.IsNullOrEmpty(user))
                return moqPosts
                    .Where(p => p.Username.Equals(user))
                    .OrderByDescending(p => p.TimePosted);

            return moqPosts
                .OrderByDescending(p => p.TimePosted);
        }

        public IEnumerable<Post> PostsForMonth(int year, int month, string user = null)
        {
            if (!string.IsNullOrEmpty(user))
                return moqPosts
                    .Where(p => p.TimePosted.Year == year && p.TimePosted.Month == month && p.Username.Equals(user))
                    .OrderByDescending(p => p.TimePosted);

            return moqPosts
                .Where(p => p.TimePosted.Year == year && p.TimePosted.Month == month)
                .OrderByDescending(p => p.TimePosted);
        }

        public IEnumerable<Post> SearchResults(string searchTerm)
        {
            return
                moqPosts.Where
                (p => p.Title.IndexOf(searchTerm, StringComparison.InvariantCultureIgnoreCase) >= 0 
                    || p.Content.IndexOf(searchTerm, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        public IEnumerable<MonthCount> MonthPostCounts(string user = null)
        {
            if (!string.IsNullOrEmpty(user))
                return moqPosts
                    .Where(p => p.Username.Equals(user))
                    .GroupBy(p => new { p.TimePosted.Year, p.TimePosted.Month})
                    .Select(g => new MonthCount {Year = g.Key.Year, Month = g.Key.Month, PostCount = g.Count()});

            return moqPosts
                    .GroupBy(p => new { p.TimePosted.Year, p.TimePosted.Month })
                    .Select(g => new MonthCount { Year = g.Key.Year, Month = g.Key.Month, PostCount = g.Count() });
        }

        public Post SinglePost(int id)
        {
            return moqPosts.SingleOrDefault(p => p.ID == id);
        }

        public int AddOrEditPost(Post post)
        {
            if (post.ID.HasValue)
            {
                moqPosts[moqPosts.FindIndex(p => p.ID == post.ID)] = post;
                return post.ID.Value;
            }

            post.ID = moqPosts.Select(p => p.ID.Value).OrderBy(id => id).Last() + 1;
            moqPosts.Add(post);
            return post.ID.Value;
        }

        public void AddComment(Comment comment, int postID)
        {
            var post = SinglePost(postID);
            if(post == null)
                return;
            post.Comments = post.Comments.Concat(new[] {comment}).ToArray();
            AddOrEditPost(post);
        }
    }
}