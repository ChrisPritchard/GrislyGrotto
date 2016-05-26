<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:output method="html" indent="yes"/>
    <xsl:template match="/">

      <html>
        <head>
          <title>The Grisly Grotto - Deviant Minds Think Alike</title>
          <link rel="stylesheet" type="text/css" href="/resources/site.css" media="screen" />
          <link rel="shortcut icon" href="/resources/favicon.png" />
          <link rel="alternate" type="application/rss+xml" title="RSS 2.0" href="/Format/RSS" />        </head>
        <body>

          <input type="hidden" id="currentUrl" value="{/Response/@Url}" />
          
          <div>
            
            <div class="middle">
              <div class="header">
                <h1>The Grisly Grotto</h1>
                <h2>Deviant Minds Think Alike</h2>
                <div class="quote">
                  <span>
                    <xsl:value-of select="//Quote/Content"/>
                  </span>
                  <h4>
                    <xsl:value-of select="//Quote/Author"/>
                  </h4>
                </div>
              </div>

              <div class="menu">
                <xsl:choose>
                  <xsl:when test="//LoggedInUser">
                    <span>
                      Welcome <xsl:value-of select="//LoggedInUser"/>.
                    </span>
                    <span>
                      <a href="/Editor">Add new post</a>
                    </span>
                    <span>
                      <a href="/Logout">Logout</a>
                    </span>
                  </xsl:when>
                  <xsl:otherwise>
                    <div id="loginPanel">
                      <form method="post" action="/" autocomplete="false">
                        Username<br/>
                        <input type="text" name="Username" />
                        <br/>
                        Password<br/>
                        <input type="password" name="Password" />
                        <br/>
                        <xsl:if test="//LoginFailed">
                          <p class="error">Username or password incorrect</p>
                        </xsl:if>
                        <input type="submit" value="Login" />
                        <br/>
                        <input type="checkbox" name="RememberMe" />&#160;Remember Me
                      </form>
                    </div>
                    <span id="loginLink">
                      <a href="#">Login</a>
                    </span>
                  </xsl:otherwise>
                </xsl:choose>
                <span>
                  Archives:
                  <select>
                    <option value="/">(Select)</option>
                    <xsl:for-each select="//MonthCount">
                      <option value="/{MonthName}/{Year}">
                        <xsl:value-of select="MonthName"/>, <xsl:value-of select="Year"/> (<xsl:value-of select="PostCount"/>)
                      </option>
                    </xsl:for-each>
                  </select>
                </span>
                <span>
                  Stories:
                  <select>
                    <option value="/">(Select)</option>
                    <xsl:for-each select="//Story">
                      <option value="/Posts/{ID}">
                        <xsl:value-of select="Title"/> (<xsl:value-of select="Author"/>, <xsl:value-of select="WordCount"/> words)
                      </option>
                    </xsl:for-each>
                  </select>
                </span>
                <form method="post" action="/Search">
                  <span>
                    Search:
                    <input type="input" name="SearchTerm" value="{//SearchTerm}" />
                    <input type="submit" value="Go" />
                  </span>
                </form>
              </div>

              <xsl:if test="//RecentPost">
                <div>
                  <table border="0" width="100%">
                    <tr>
                      <td align="left" width="50%">
                        <h2>Christopher</h2>
                        <ul class="recentPosts">
                          <xsl:apply-templates select="//RecentPost[Username = 'Christopher']" />
                        </ul>
                      </td>
                      <td align="right" width="50%">
                        <h2>Peter</h2>
                        <ul class="recentPosts">
                          <xsl:apply-templates select="//RecentPost[Username = 'Peter']" />
                        </ul>
                      </td>
                    </tr>
                  </table>
                </div>
              </xsl:if>

              <div class="content">
                <xsl:choose>
                  <xsl:when test="not(//Editor)">
                    <xsl:for-each select="//Post">
                      <div class="post">
                        <xsl:if test="IsStory = 'true'">
                          <xsl:attribute name="class">
                            post story
                          </xsl:attribute>
                        </xsl:if>
                        <h3>
                          <xsl:value-of select="Title" />
                        </h3>
                        <p>
                          <strong>
                            <xsl:value-of select="Username" />
                          </strong>
                          <br/>
                          <xsl:value-of select="FormattedTimePosted" />
                          <xsl:if test="not(//SinglePost)">
                            <br/>
                            <a href="/Posts/{ID}">Comments (<xsl:value-of select="count(Comments/Comment)"/>)</a>
                          </xsl:if>
                          <xsl:if test="//LoggedInUser = Username">
                            <br/>
                            <a href="/Editor/{ID}">Edit</a>
                          </xsl:if>
                        </p>
                        <div>
                          <xsl:value-of select="Content" disable-output-escaping="yes" />
                        </div>
                      </div>
                      <xsl:choose>
                        <xsl:when test="//SinglePost">
                          <xsl:for-each select="Comments/Comment">
                            <div class="comment">
                              <strong>Commented by <xsl:value-of select="Author" />, <xsl:value-of select="TimeMadeText" /></strong>
                              <br/>
                              <xsl:value-of select="Content" />
                            </div>
                          </xsl:for-each>
                          <div class="comment">
                            <form method="post">
                              <h3>Author: <input type="text" name="Author"/></h3>
                              <p>
                                <textarea name="Content" rows="5" cols="40"/>
                              </p>
                              <input type="submit" value="Add Comment" />
                            </form>
                          </div>
                        </xsl:when>
                        <xsl:otherwise>
                          <p>
                            <a href="/Posts/{ID}">Comments (<xsl:value-of select="count(Comments/Comment)"/>)</a>
                          </p>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:for-each>
                  </xsl:when>
                  <xsl:otherwise>
                    <form method="post">
                      <h3>
                        Title: <input type="text" name="Title" value="{//Post/Title}" />
                      </h3>
                      <p>
                        <textarea id="Editor" name="Content" rows="15" cols="80">
                          <xsl:value-of select="//Post/Content"/>
                        </textarea>
                      </p>
                      <br/>
                      <input type="checkbox" name="IsStory" />
                      <xsl:if test="//IsStory = true">
                        <xsl:attribute name="value">on</xsl:attribute>
                      </xsl:if>
                      &#160;Should be read as story
                      <br/>
                      <input type="submit" value="Submit" />
                      <input type="hidden" name="EditorUser" value="{//LoggedInUser}" />
                    </form>
                  </xsl:otherwise>
                </xsl:choose>
              </div>
            </div>
          </div>

          <script src="/Resources/NicEditor/NicEdit.js" type="text/javascript" />
          <script src="http://code.jquery.com/jquery-1.5.min.js" type="text/javascript" />
          <script src="/Resources/Site.js" type="text/javascript" />
        </body>
      </html>
      
    </xsl:template>

  <xsl:template match="RecentPost">
    <li>
      <a href="/Posts/{ID}">
        <span class="recentTitle">
          <xsl:value-of select="Title"/>
        </span>
        <span>
          <xsl:value-of select="TimeSincePosted"/> ago
        </span>
      </a>
    </li>
  </xsl:template>

</xsl:stylesheet>
