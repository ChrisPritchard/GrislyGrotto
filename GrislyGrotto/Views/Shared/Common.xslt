<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:output method="html" indent="yes" />

    <xsl:template match="/">
      
        <html>
            <head>
                <title>
                    Grisly Grotto - Deviant Minds Think Alike
                </title>
                <link rel="alternate" type="application/atom+xml" title="Atom 1.0" href="/Feed/Atom" />
                <link rel="alternate" type="application/rss+xml" title="RSS 2.0" href="/Feed/Rss" />

                <link type="text/css" rel="stylesheet" href="/Content/grislygrotto.css" />
                <script type="text/javascript" src="/Scripts/jquery-1.3.1.min.js" />
                <script type="text/javascript" src="/Scripts/jquery.validate.js" />
                <script type="text/javascript" src="/Scripts/Shared/Common.js" />
            </head>
            <body>
                <div class="page">

                    <div class="header" id="headerDiv">
                        <h1 class="headertext">The Grisly Grotto</h1>
                        <h2 class="headertext">Deviant Minds Think Alike</h2>
                    </div>
                  
                    <div class="quote">
                        <span class="quotetext">
                            <xsl:value-of select="ViewData/Quote/." />
                        </span>
                        <br/>
                        -<xsl:value-of select="ViewData/Quote/@Author" />
                    </div>

                    <xsl:call-template name="AuthorLinks" />

                    <div class="menu">
                        <xsl:call-template name="UserBox" />
                        <xsl:call-template name="BlogHistory" />
                    </div>

                    <div>
                        <xsl:apply-templates />
                    </div>

                    <div class="footer">
                        Site coded and maintained by <a href="mailto:havoc8844@hotmail.com">Christopher Pritchard</a>, 2009. New Zealand Forever
                    </div>

                </div>

            </body>
        </html>

    </xsl:template>

    <xsl:template name="UserBox">
        <xsl:choose>
            <xsl:when test="ViewData/AuthorDetails/Author/@LoggedIn">
                <p>
                    <h3>
                        Welcome <xsl:value-of select="ViewData/AuthorDetails/Author[@LoggedIn]/@Fullname"/>
                    </h3>
                    <p>
                        <a href="/User/Logout">Logout</a>
                    </p>
                    <p>
                        <a href="/Blog/Editor">Post a new Blog</a>
                    </p>
                </p>
            </xsl:when>
            <xsl:otherwise>
                <form id="Login" method="post" action="/User/Login">
                    <p>
                        <label for="Username">Username:&#160;</label>
                        <input type="text" id="Username" name="Username" />
                    </p>
                    <p>
                        <label for="Password">Password:&#160;</label>
                        <input type="password" id="Password" name="Password" />
                    </p>
                    <input type="submit" value="Login" />
                </form>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template name="BlogHistory">
        <p>
            <h3 class="historyheading">Recent Posts</h3>
            <ul>
                <xsl:for-each select="ViewData/History/RecentEntries/Entry">
                    <li>
                        <a href="/Blog/Specific/{@BlogID}">
                            <xsl:value-of select="@Title"/>
                            <br/>
                            <span class="howrecent">
                                <xsl:value-of select="@HowRecent"/>
                            </span>
                        </a>
                    </li>
                </xsl:for-each>
            </ul>
        </p>

        <p>
          <h3 class="historyheading">History</h3>
            <ul>
                <xsl:for-each select="ViewData/History/MonthBlogCounts/Month">
                    <li>
                        <a href="/Blog/{@Year}/{@Month}">
                            <xsl:if test="/ViewData/AuthorDetails/Author[@Current]">
                                <xsl:attribute name="href">
                                    /Blog/<xsl:value-of select="@Year" />/<xsl:value-of select="@Month" />/<xsl:value-of select="/ViewData/AuthorDetails/Author[@Current]/@Fullname" />
                                </xsl:attribute>
                            </xsl:if>
                            <xsl:value-of select="@Month"/>&#160;<xsl:value-of select="@Year"/>&#160;(<xsl:value-of select="@Count"/>)</a>
                    </li>
                </xsl:for-each>
            </ul>
        </p>
    </xsl:template>

    <xsl:template name="AuthorLinks">
        <div class="authorlinks">
            <xsl:choose>
                <xsl:when test="ViewData/AuthorDetails/Author/@Current">
                    <a href="/Blog/Latest">All</a>
                </xsl:when>
                <xsl:otherwise>
                    All
                </xsl:otherwise>
            </xsl:choose>
            <xsl:for-each select="ViewData/AuthorDetails/Author">
                &#160;
                <xsl:choose>
                    <xsl:when test="@Current">
                        <xsl:value-of select="@Fullname"/>
                    </xsl:when>
                    <xsl:otherwise>
                        <a href="/Blog/Latest/{@Fullname}">
                            <xsl:value-of select="@Fullname"/>
                        </a>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:for-each>
        </div>
    </xsl:template>

</xsl:stylesheet>
